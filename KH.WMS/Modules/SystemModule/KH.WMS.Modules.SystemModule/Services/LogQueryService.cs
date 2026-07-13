using System.Text.RegularExpressions;
using KH.WMS.Core.DependencyInjection.ServiceLifetimes;
using KH.WMS.Modules.SystemModule.DTOs;
using KH.WMS.Modules.SystemModule.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KH.WMS.Modules.SystemModule.Services;

/// <summary>
/// 文本日志查询服务：解析 Serilog 文件日志（按天 log-{yyyyMMdd}.txt），
/// 支持按 日期范围/级别/LogType/关键字/RequestId/UserId 筛选 + 分页。
/// </summary>
[RegisteredService(ServiceType = typeof(ILogQueryService))]
public class LogQueryService : ILogQueryService
{
    private readonly string _logDirectory;

    /// <summary>匹配 Serilog 文件模板：
    /// {Timestamp} [Level] [ModuleCode] [LogType:x] [TenantId:x] [UserId:x] [RequestId:x] Message</summary>
    private static readonly Regex LineRegex = new(
        @"^(?<ts>\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\.\d{3} [+-]\d{2}:\d{2}) \[(?<lvl>\w{3})\] \[(?<mod>[^\]]*)\] \[LogType:(?<logtype>[^\]]*)\] \[TenantId:(?<tenant>[^\]]*)\] \[UserId:(?<user>[^\]]*)\] \[RequestId:(?<req>[^\]]*)\] (?<msg>.*)$",
        RegexOptions.Compiled);

    private static readonly Regex DateFileRegex = new(@"^log-(\d{8})\.txt$", RegexOptions.Compiled);

    /// <summary>单次查询最多扫描的天数，防止 OOM 与超长耗时</summary>
    private const int MaxScanDays = 7;

    public LogQueryService(IConfiguration configuration)
    {
        var logPath = configuration["Serilog:LogPath"] ?? "Logs";
        // Serilog 文件 sink 的相对路径基于当前工作目录（Environment.CurrentDirectory），
        // 读取也必须用同一基准——否则开发时日志写在 项目目录/Logs，读取却去 bin/.../Logs。
        _logDirectory = Path.Combine(Environment.CurrentDirectory, logPath);
    }

    /// <inheritdoc />
    public Task<List<string>> GetLogDatesAsync()
    {
        var dates = new List<string>();
        if (!Directory.Exists(_logDirectory))
            return Task.FromResult(dates);

        foreach (var file in Directory.EnumerateFiles(_logDirectory, "log-*.txt"))
        {
            var name = Path.GetFileName(file);
            var m = DateFileRegex.Match(name);
            if (m.Success)
            {
                var d = m.Groups[1].Value;
                dates.Add($"{d[0..4]}-{d[4..6]}-{d[6..8]}");
            }
        }
        // 降序（最新在前）
        return Task.FromResult(dates.OrderByDescending(x => x).ToList());
    }

    /// <inheritdoc />
    public async Task<LogQueryResult> QueryAsync(LogQueryRequest request)
    {
        var result = new LogQueryResult();
        if (!Directory.Exists(_logDirectory))
            return result;

        // 选定日期范围（默认近 3 天）
        var allDates = await GetLogDatesAsync();
        var start = (request.StartDate?.Date ?? DateTime.Today.AddDays(-2));
        var end = (request.EndDate?.Date ?? DateTime.Today);
        var selectedDates = allDates.Where(d =>
        {
            return DateTime.TryParse(d, out var dt) && dt.Date >= start && dt.Date <= end;
        }).ToList();

        // 安全限制：最多扫描 MaxScanDays 天
        if (selectedDates.Count > MaxScanDays)
            selectedDates = selectedDates.Take(MaxScanDays).ToList();

        var entries = new List<LogEntry>();
        foreach (var date in selectedDates)
        {
            var file = Path.Combine(_logDirectory, $"log-{date.Replace("-", "")}.txt");
            if (!File.Exists(file)) continue;

            await ParseFileAsync(file, entries);
        }

        // 内存筛选
        var filtered = entries.AsEnumerable();
        if (request.Levels is { Count: > 0 })
            filtered = filtered.Where(e => request.Levels.Contains(e.Level, StringComparer.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(request.LogType))
            filtered = filtered.Where(e => e.LogType.Contains(request.LogType, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var kw = request.Keyword;
            filtered = filtered.Where(e =>
                e.Message.Contains(kw, StringComparison.OrdinalIgnoreCase)
                || (e.Exception?.Contains(kw, StringComparison.OrdinalIgnoreCase) ?? false));
        }
        if (!string.IsNullOrWhiteSpace(request.RequestId))
            filtered = filtered.Where(e => !string.IsNullOrEmpty(e.RequestId)
                && e.RequestId.Contains(request.RequestId, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrWhiteSpace(request.UserId))
            filtered = filtered.Where(e => !string.IsNullOrEmpty(e.UserId)
                && e.UserId.Contains(request.UserId, StringComparison.OrdinalIgnoreCase));

        // 倒序（最新在前）+ 分页
        var sorted = filtered.OrderByDescending(e => e.Timestamp).ToList();
        result.Total = sorted.Count;
        var pageSize = request.PageSize <= 0 ? 50 : request.PageSize;
        var pageIndex = request.PageIndex <= 0 ? 1 : request.PageIndex;
        result.Items = sorted
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToList();
        return result;
    }

    /// <summary>
    /// 流式逐行解析单个日志文件：匹配模板的行作为新条目起始，
    /// 不匹配的行归并到上一条的 Exception（异常堆栈多行）。
    /// </summary>
    private async Task ParseFileAsync(string file, List<LogEntry> entries)
    {
        LogEntry? current = null;
        // File.ReadLines 流式枚举，不一次性加载大文件到内存
        foreach (var raw in File.ReadLines(file))
        {
            // 去除行首可能的 BOM
            var line = raw.TrimStart('﻿');
            var m = LineRegex.Match(line);
            if (m.Success)
            {
                if (current != null)
                    entries.Add(current);
                current = ParseEntry(m);
            }
            else if (current != null)
            {
                // 异常堆栈/续行归并
                if (string.IsNullOrEmpty(line))
                    current.Exception += "\n";
                else
                    current.Exception = (current.Exception ?? string.Empty) + line + "\n";
            }
        }
        if (current != null)
            entries.Add(current);

        await Task.CompletedTask;
    }

    private static LogEntry ParseEntry(Match m)
    {
        return new LogEntry
        {
            Timestamp = DateTime.Parse(m.Groups["ts"].Value),
            Level = m.Groups["lvl"].Value,
            ModuleCode = m.Groups["mod"].Value,
            LogType = m.Groups["logtype"].Value,
            TenantId = NullIfEmpty(m.Groups["tenant"].Value),
            UserId = NullIfEmpty(m.Groups["user"].Value),
            RequestId = NullIfEmpty(m.Groups["req"].Value),
            Message = m.Groups["msg"].Value,
        };
    }

    private static string? NullIfEmpty(string s)
        => string.IsNullOrWhiteSpace(s) || s == "null" ? null : s;

    // ==================== 文件浏览模式（原始内容）====================

    private static readonly Regex DateInNameRegex = new(@"(\d{8})\.txt$", RegexOptions.Compiled);
    // 匹配行首时间戳后的级别标识，如 "... +08:00 [ERR] ..."
    private static readonly Regex LevelInLineRegex = new(@"^\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\.\d{3} [+-]\d{2}:\d{2} \[(\w{3})\]", RegexOptions.Compiled);

    /// <inheritdoc />
    public Task<List<LogFileInfo>> GetLogFilesAsync()
    {
        var result = new List<LogFileInfo>();
        if (!Directory.Exists(_logDirectory))
            return Task.FromResult(result);

        // 根目录文件：按前缀分类
        foreach (var file in Directory.EnumerateFiles(_logDirectory, "*.txt"))
        {
            var name = Path.GetFileName(file);
            var category = name.StartsWith("error-", StringComparison.OrdinalIgnoreCase) ? "错误日志"
                         : name.StartsWith("warning-", StringComparison.OrdinalIgnoreCase) ? "警告日志"
                         : name.StartsWith("log-", StringComparison.OrdinalIgnoreCase) ? "全部日志"
                         : name.StartsWith("debug-", StringComparison.OrdinalIgnoreCase) ? "调试日志"
                         : "其他";
            AddFileInfo(result, category, name, file);
        }

        // 递归所有子目录（wms 模块日志、custom 自定义日志等），按目录名分类
        foreach (var dir in Directory.EnumerateDirectories(_logDirectory))
        {
            var dirName = Path.GetFileName(dir);
            var category = dirName switch
            {
                "wms" => "模块日志",
                "custom" => "自定义日志",
                _ => dirName
            };
            foreach (var file in Directory.EnumerateFiles(dir, "*.txt", SearchOption.AllDirectories))
            {
                // 相对路径（如 wms/Inbound-20260625.txt），供 EnumerateAllLines 拼接
                var relPath = Path.GetRelativePath(_logDirectory, file).Replace('\\', '/');
                AddFileInfo(result, category, relPath, file);
            }
        }

        result = result.OrderByDescending(x => x.Date).ThenBy(x => x.Category).ToList();
        return Task.FromResult(result);
    }

    private static void AddFileInfo(List<LogFileInfo> result, string category, string fileName, string fullPath)
    {
        result.Add(new LogFileInfo
        {
            Category = category,
            FileName = fileName,
            Date = ExtractDateFromName(fileName),
            SizeKB = new FileInfo(fullPath).Length / 1024
        });
    }

    /// <inheritdoc />
    public Task<LogContentResult> GetLogContentAsync(LogContentRequest request)
    {
        var result = new LogContentResult();
        if (request?.FileNames == null || request.FileNames.Count == 0)
            return Task.FromResult(result);

        var take = request.LineCount <= 0 ? 500 : request.LineCount;
        var skip = request.StartLine < 0 ? 0 : request.StartLine;
        var levels = request.Levels;
        var keyword = request.Keyword;

        // 流式枚举所有文件行 → 过滤 → 跳过 → 取 take+1（多取 1 判断 hasMore），延迟执行取够即停
        var page = EnumerateAllLines(request.FileNames)
            .Where(x => MatchLine(x.Line, levels, keyword, request.LogType, request.RequestId, request.ModuleCode, request.StartTime, request.EndTime))
            .Skip(skip)
            .Take(take + 1)
            .ToList();

        result.HasMore = page.Count > take;
        var pageItems = result.HasMore ? page.Take(take).ToList() : page;
        result.Lines = pageItems.Select((x, i) => new LogLine
        {
            LineNo = skip + i + 1,
            SourceFile = x.File,
            Content = x.Line.TrimStart('﻿')
        }).ToList();
        return Task.FromResult(result);
    }

    /// <summary>逐文件流式枚举 (file, line)，避免一次性加载大文件</summary>
    private IEnumerable<(string File, string Line)> EnumerateAllLines(List<string> fileNames)
    {
        foreach (var fileName in fileNames)
        {
            if (!IsSafeFileName(fileName)) continue;
            var path = Path.Combine(_logDirectory, fileName);
            if (!File.Exists(path)) continue;
            // 用 FileShare.ReadWrite 打开，避免与 Serilog 正在写入的文件锁冲突（"文件被另一个进程使用"）
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var reader = new StreamReader(stream, System.Text.Encoding.UTF8);
            while (reader.ReadLine() is { } line)
                yield return (fileName, line);
        }
    }

    /// <summary>防路径穿越：仅允许 Logs 根或 wms/ 子目录下的 .txt</summary>
    private static bool IsSafeFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName)) return false;
        if (Path.IsPathRooted(fileName)) return false;
        if (fileName.Contains("..", StringComparison.Ordinal)) return false;
        return fileName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase);
    }

    private static bool MatchLine(string line, List<string>? levels, string? keyword, string? logType, string? requestId, string? moduleCode, DateTime? startTime = null, DateTime? endTime = null)
    {
        if (!string.IsNullOrWhiteSpace(keyword)
            && !line.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            return false;
        if (levels is { Count: > 0 })
        {
            var lvl = ExtractLevel(line);
            // 续行（无级别标识）在有级别过滤时也保留，便于看完整异常堆栈
            if (lvl != null && !levels.Contains(lvl, StringComparer.OrdinalIgnoreCase))
                return false;
        }
        if (!string.IsNullOrWhiteSpace(logType)
            && !line.Contains($"[LogType:{logType}]", StringComparison.OrdinalIgnoreCase))
            return false;
        if (!string.IsNullOrWhiteSpace(requestId)
            && !line.Contains($"[RequestId:{requestId}]", StringComparison.OrdinalIgnoreCase))
            return false;
        if (!string.IsNullOrWhiteSpace(moduleCode)
            && !line.Contains($"[{moduleCode}]", StringComparison.OrdinalIgnoreCase))
            return false;
        // 时间范围过滤：解析行首时间戳，续行（无时间戳）保留以查看完整堆栈
        if (startTime.HasValue || endTime.HasValue)
        {
            var ts = ExtractTimestamp(line);
            if (ts.HasValue)
            {
                if (startTime.HasValue && ts.Value < startTime.Value) return false;
                if (endTime.HasValue && ts.Value > endTime.Value) return false;
            }
        }
        return true;
    }

    /// <summary>从行首提取时间戳，格式如 2026-06-30 14:10:00.000 +08:00</summary>
    private static DateTime? ExtractTimestamp(string line)
    {
        if (string.IsNullOrEmpty(line) || line.Length < 25) return null;
        // 行首格式: 2026-06-30 14:10:00.000 +08:00 [INF] ...
        var m = LineRegex.Match(line);
        if (!m.Success) return null;
        return DateTime.Parse(m.Groups["ts"].Value);
    }

    private static string? ExtractLevel(string line)
    {
        var m = LevelInLineRegex.Match(line);
        return m.Success ? m.Groups[1].Value : null;
    }

    private static string ExtractDateFromName(string name)
    {
        var m = DateInNameRegex.Match(name);
        return m.Success ? $"{m.Groups[1].Value[0..4]}-{m.Groups[1].Value[4..6]}-{m.Groups[1].Value[6..8]}" : string.Empty;
    }
}
