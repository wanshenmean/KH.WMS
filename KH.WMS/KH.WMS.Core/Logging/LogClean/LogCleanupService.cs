using KH.WMS.Core.Caching;
using KH.WMS.Core.Constants;
using KH.WMS.Core.Database;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KH.WMS.Core.Logging.LogClean;

/// <summary>
/// 日志清理服务实现
/// </summary>
public class LogCleanupService : ILogCleanupService
{
    private readonly LogCleanupOptions _options;
    private readonly ILogger<LogCleanupService> _logger;
    private readonly IDbContext? _dbContext;
    private readonly ICacheService _cacheService;

    public LogCleanupService(
        IOptions<LogCleanupOptions> options,
        ILogger<LogCleanupService> logger,
        ICacheService cacheService,
        IDbContext? dbContext = null)
    {
        _options = options.Value;
        _logger = logger;
        _cacheService = cacheService;
        _dbContext = dbContext;
    }

    /// <summary>
    /// 清理文件日志
    /// </summary>
    public async Task CleanupFileLogsAsync()
    {
        try
        {
            if (!Directory.Exists(_options.LogPath))
            {
                _logger.LogDebug("日志目录不存在: {Path}", _options.LogPath);
                return;
            }

            var retentionDays = GetLogRetainDays();
            var cutoffDate = DateTime.Now.AddDays(-retentionDays);
            var deletedFiles = new List<string>();
            var totalSize = 0L;

            // 获取所有日志文件
            var logFiles = Directory.GetFiles(_options.LogPath, "*.txt", SearchOption.AllDirectories);
            _logger.LogDebug("找到 {Count} 个日志文件", logFiles.Length);

            foreach (var file in logFiles)
            {
                var fileInfo = new FileInfo(file);

                // 检查文件是否过期
                if (fileInfo.LastWriteTime < cutoffDate)
                {
                    try
                    {
                        totalSize += fileInfo.Length;
                        File.Delete(file);
                        deletedFiles.Add(file);
                        _logger.LogDebug("删除过期日志文件: {File}, 大小: {Size}MB, 最后修改: {LastWrite}",
                            Path.GetFileName(file),
                            (fileInfo.Length / 1024.0 / 1024.0).ToString("F2"),
                            fileInfo.LastWriteTime);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "删除日志文件失败: {File}", file);
                    }
                }

                // 检查文件大小是否超过限制
                if (fileInfo.Length > _options.MaxFileSizeMB * 1024 * 1024)
                {
                    try
                    {
                        // 文件过大，创建备份并清空
                        var backupPath = file.Replace(".txt", $"_backup_{DateTime.Now:yyyyMMddHHmmss}.txt");
                        File.Move(file, backupPath);

                        _logger.LogDebug("文件过大，已备份: {File} -> {Backup}", Path.GetFileName(file), Path.GetFileName(backupPath));

                        // 删除备份
                        if (File.Exists(backupPath))
                        {
                            File.Delete(backupPath);
                            deletedFiles.Add(backupPath);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "处理大文件失败: {File}", file);
                    }
                }
            }

            // 清理空目录
            CleanEmptyDirectories(_options.LogPath);

            _logger.LogInformation("文件日志清理完成: 删除 {Count} 个文件, 释放 {Size}MB",
                deletedFiles.Count,
                (totalSize / 1024.0 / 1024.0).ToString("F2"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "文件日志清理失败");
            throw;
        }
    }

    /// <summary>
    /// 清理数据库日志
    /// </summary>
    public async Task CleanupDatabaseLogsAsync()
    {
        //if (_dbContext == null)
        //{
        //    _logger.LogDebug("未配置数据库上下文，跳过数据库日志清理");
        //    return;
        //}

        //if (!_options.CleanupDatabaseLogs)
        //{
        //    _logger.LogDebug("数据库日志清理未启用");
        //    return;
        //}

        //try
        //{
        //    var cutoffDate = DateTime.Now.AddDays(-_options.RetentionDays);
        //    var totalDeleted = 0;

        //    // 使用仓储删除过期日志
        //    var logRepo = _dbContext.GetRepository<dynamic>();

        //    // 分批删除，避免一次性删除太多
        //    while (true)
        //    {
        //        // 删除指定时间之前的日志
        //        var deleteSql = $@"
        //            DELETE FROM {_options.DatabaseLogTable}
        //            WHERE CreatedTime < @CutoffDate
        //            LIMIT @_options.DatabaseCleanupBatchSize";

        //        try
        //        {
        //            var deletedCount = await _dbContext.Db.Ado.ExecuteCommandAsync(
        //                deleteSql,
        //                new { CutoffDate = cutoffDate, BatchSize = _options.DatabaseCleanupBatchSize });

        //            if (deletedCount == 0)
        //                break;

        //            totalDeleted += deletedCount;
        //            _logger.LogDebug("已删除 {Count} 条数据库日志", deletedCount);

        //            // 避免锁表，稍微延迟
        //            await Task.Delay(100);
        //        }
        //        catch (Exception ex)
        //        {
        //            _logger.LogError(ex, "删除数据库日志失败");
        //            break;
        //        }
        //    }

        //    _logger.LogInformation("数据库日志清理完成: 删除 {Count} 条记录", totalDeleted);
        //}
        //catch (Exception ex)
        //{
        //    _logger.LogError(ex, "数据库日志清理失败");
        //    throw;
        //}
    }

    /// <summary>
    /// 执行全部清理
    /// </summary>
    public async Task CleanupAllAsync()
    {
        _logger.LogInformation("开始日志清理: 保留天数={Days}, 最大文件大小={Size}MB",
            _options.RetentionDays,
            _options.MaxFileSizeMB);

        // 清理文件日志
        await CleanupFileLogsAsync();

        // 清理数据库日志
        await CleanupDatabaseLogsAsync();

        _logger.LogInformation("日志清理完成");
    }

    /// <summary>
    /// 获取日志统计信息
    /// </summary>
    public async Task<LogStatistics> GetStatisticsAsync()
    {
        var statistics = new LogStatistics();

        try
        {
            // 文件日志统计
            if (Directory.Exists(_options.LogPath))
            {
                var logFiles = Directory.GetFiles(_options.LogPath, "*.txt", SearchOption.AllDirectories);
                statistics.FileCount = logFiles.Length;
                statistics.FileSizeMB = logFiles.Sum(f => new FileInfo(f).Length) / 1024.0m / 1024.0m;

                if (logFiles.Length > 0)
                {
                    var fileInfos = logFiles.Select(f => new FileInfo(f));
                    statistics.OldestLogDate = fileInfos.Min(f => f.LastWriteTime);
                    statistics.NewestLogDate = fileInfos.Max(f => f.LastWriteTime);
                }
            }

            // 数据库日志统计
            if (_dbContext != null && _options.CleanupDatabaseLogs)
            {
                try
                {
                    var countSql = $"SELECT COUNT(*) FROM {_options.DatabaseLogTable}";
                    statistics.DatabaseLogCount = await _dbContext.Db.Ado.SqlQuerySingleAsync<long>(countSql);
                }
                catch
                {
                    // 表可能不存在
                }
            }

            _logger.LogDebug("日志统计: 文件数={FileCount}, 文件大小={Size}MB, 数据库记录数={DBCount}",
                statistics.FileCount,
                statistics.FileSizeMB.ToString("F2"),
                statistics.DatabaseLogCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取日志统计失败");
        }

        return statistics;
    }

    /// <summary>
    /// 从缓存读取日志保留天数，读取失败则使用 appsettings.json 中的默认值
    /// </summary>
    private int GetLogRetainDays()
    {
        var value = _cacheService.Get<string>(CacheConstants.SysParam.GetKey("LOG_RETAIN_DAYS"));
        if (!string.IsNullOrEmpty(value) && int.TryParse(value, out var days) && days > 0)
            return days;
        return _options.RetentionDays;
    }

    /// <summary>
    /// 清理空目录
    /// </summary>
    private void CleanEmptyDirectories(string parentDir)
    {
        try
        {
            foreach (var dir in Directory.GetDirectories(parentDir))
            {
                CleanEmptyDirectories(dir);

                if (!Directory.EnumerateFileSystemEntries(dir).Any())
                {
                    Directory.Delete(dir);
                    _logger.LogDebug("删除空目录: {Dir}", dir);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "清理空目录失败: {Dir}", parentDir);
        }
    }
}
