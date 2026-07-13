using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace KH.WMS.Core.Logging;


/// <summary>
/// 日志服务接口（自动识别模块）
/// </summary>
public interface ILoggerService
{
    /// <summary>
    /// 记录详细跟踪日志 - 每一行SQL执行、详细流程跟踪
    /// </summary>
    void LogVerbose(string message, params object?[] args);

    /// <summary>
    /// 记录调试日志 - 方法参数、中间变量
    /// </summary>
    void LogDebug(string message, params object?[] args);

    /// <summary>
    /// 记录信息日志 - 用户登录、单据创建
    /// </summary>
    void LogInfo(string message, params object?[] args);

    /// <summary>
    /// 记录警告日志 - API调用慢、缓存未命中，需关注
    /// </summary>
    void LogWarning(string message, params object?[] args);

    /// <summary>
    /// 记录错误日志 - HTTP 500、业务异常，但系统可继续
    /// </summary>
    void LogError(string message, params object?[] args);

    /// <summary>
    /// 记录错误日志（带异常）- HTTP 500、业务异常，但系统可继续
    /// </summary>
    void LogError(Exception exception, string message, params object?[] args);

    /// <summary>
    /// 记录致命错误日志 - 数据库连接失败、磁盘满，系统无法继续
    /// </summary>
    void LogFatal(string message, params object?[] args);

    /// <summary>
    /// 记录致命错误日志（带异常）- 数据库连接失败、磁盘满，系统无法继续
    /// </summary>
    void LogFatal(Exception exception, string message, params object?[] args);

    /// <summary>
    /// 记录操作日志
    /// </summary>
    void LogOperation(string operation, string? userName = null, long? userId = null, object? data = null);

    /// <summary>
    /// 记录业务日志
    /// </summary>
    void LogBusiness(string businessType, string message, object? data = null);

    /// <summary>
    /// 记录性能日志
    /// </summary>
    void LogPerformance(string operation, long elapsedMs, object? data = null);

    /// <summary>
    /// 使用上下文记录日志（可手动指定模块）
    /// </summary>
    void Log(LogContext context, string message, params object?[] args);

    // ==================== 支持指定文件名的重载方法 ====================

    /// <summary>
    /// 记录信息日志到指定文件
    /// </summary>
    void LogInfoToFile(string fileName, string message, params object?[] args);

    /// <summary>
    /// 记录错误日志到指定文件
    /// </summary>
    void LogErrorToFile(string fileName, string message, params object?[] args);

    /// <summary>
    /// 记录错误日志到指定文件（带异常）
    /// </summary>
    void LogErrorToFile(string fileName, Exception exception, string message, params object?[] args);

    /// <summary>
    /// 记录操作日志到指定文件
    /// </summary>
    void LogOperationToFile(string fileName, string operation, string? userName = null, long? userId = null, object? data = null);

    /// <summary>
    /// 记录业务日志到指定文件
    /// </summary>
    void LogBusinessToFile(string fileName, string businessType, string message, object? data = null);
}
