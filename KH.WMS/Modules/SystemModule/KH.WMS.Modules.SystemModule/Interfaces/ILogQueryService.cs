using KH.WMS.Modules.SystemModule.DTOs;

namespace KH.WMS.Modules.SystemModule.Interfaces;

/// <summary>
/// 文本日志查询服务：读取 Serilog 文件日志，按条件筛选分页
/// </summary>
public interface ILogQueryService
{
    /// <summary>按条件查询日志（分页）</summary>
    Task<LogQueryResult> QueryAsync(LogQueryRequest request);

    /// <summary>获取可用的日志日期列表（来自文件名，降序）</summary>
    Task<List<string>> GetLogDatesAsync();

    /// <summary>获取所有日志文件列表（分类+文件名+日期+大小，供前端文件树）</summary>
    Task<List<LogFileInfo>> GetLogFilesAsync();

    /// <summary>读取原始日志内容（多文件拼接 + 分页 + 关键字/级别过滤）</summary>
    Task<LogContentResult> GetLogContentAsync(LogContentRequest request);
}
