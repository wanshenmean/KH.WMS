namespace KH.WMS.Core.Logging.LogClean;


/// <summary>
/// 日志清理服务接口
/// </summary>
public interface ILogCleanupService
{
    /// <summary>
    /// 清理文件日志
    /// </summary>
    Task CleanupFileLogsAsync();

    /// <summary>
    /// 清理数据库日志
    /// </summary>
    Task CleanupDatabaseLogsAsync();

    /// <summary>
    /// 执行全部清理
    /// </summary>
    Task CleanupAllAsync();

    /// <summary>
    /// 获取日志统计信息
    /// </summary>
    Task<LogStatistics> GetStatisticsAsync();
}
