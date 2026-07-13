using Serilog;
using Serilog.Events;

namespace KH.WMS.Core.Logging.Serilog;

/// <summary>
/// Serilog 日志配置扩展
/// </summary>
public static class SerilogFileConfiguration
{
    /// <summary>
    /// 配置文件日志（支持文件大小限制）
    /// </summary>
    public static LoggerConfiguration ConfigureFileLogging(
        this LoggerConfiguration loggerConfiguration,
        string logPath = "Logs",
        int retentionDays = 30,
        int maxFileSizeMB = 100,
        LogEventLevel restrictedToMinimumLevel = LogEventLevel.Information)
    {
        // 确保日志目录存在
        if (!Directory.Exists(logPath))
        {
            Directory.CreateDirectory(logPath);
        }

        var fileSizeLimitBytes = maxFileSizeMB * 1024 * 1024;

        // 配置文件输出
        loggerConfiguration.WriteTo.File(
            path: Path.Combine(logPath, "log-.txt"),
            rollingInterval: RollingInterval.Day,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{ModuleCode}] [LogType:{LogType}] [TenantId:{TenantId}] [UserId:{UserId}] [RequestId:{RequestId}] {Message:lj}{NewLine}{Exception}",
            encoding: System.Text.Encoding.UTF8,
            restrictedToMinimumLevel: restrictedToMinimumLevel,
            fileSizeLimitBytes: fileSizeLimitBytes,
            retainedFileCountLimit: retentionDays,
            rollOnFileSizeLimit: true,
            flushToDiskInterval: TimeSpan.FromSeconds(1)
        );

        // 配置错误日志
        loggerConfiguration.WriteTo.File(
            path: Path.Combine(logPath, "error-.txt"),
            rollingInterval: RollingInterval.Day,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{ModuleCode}] [LogType:{LogType}] [TenantId:{TenantId}] [UserId:{UserId}] [RequestId:{RequestId}] {Message:lj}{NewLine}{Exception}",
            encoding: System.Text.Encoding.UTF8,
            restrictedToMinimumLevel: LogEventLevel.Error,
            fileSizeLimitBytes: fileSizeLimitBytes,
            retainedFileCountLimit: retentionDays,
            rollOnFileSizeLimit: true,
            flushToDiskInterval: TimeSpan.FromSeconds(1)
        );

        // 配置警告日志
        loggerConfiguration.WriteTo.File(
            path: Path.Combine(logPath, "warning-.txt"),
            rollingInterval: RollingInterval.Day,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{ModuleCode}] [LogType:{LogType}] [TenantId:{TenantId}] [UserId:{UserId}] [RequestId:{RequestId}] {Message:lj}{NewLine}{Exception}",
            encoding: System.Text.Encoding.UTF8,
            restrictedToMinimumLevel: LogEventLevel.Warning,
            fileSizeLimitBytes: fileSizeLimitBytes,
            retainedFileCountLimit: retentionDays,
            rollOnFileSizeLimit: true,
            flushToDiskInterval: TimeSpan.FromSeconds(1)
        );

        return loggerConfiguration;
    }

    /// <summary>
    /// 配置文件日志（支持自定义文件名）
    /// </summary>
    /// <param name="loggerConfiguration">日志配置器</param>
    /// <param name="logPath">日志目录路径</param>
    /// <param name="logFileName">自定义日志文件名（不含扩展名），为 null 则使用默认值 "log"</param>
    /// <param name="errorFileName">自定义错误日志文件名（不含扩展名），为 null 则使用默认值 "error"</param>
    /// <param name="warningFileName">自定义警告日志文件名（不含扩展名），为 null 则使用默认值 "warning"</param>
    /// <param name="retentionDays">日志保留天数</param>
    /// <param name="maxFileSizeMB">单个文件最大大小（MB）</param>
    /// <param name="restrictedToMinimumLevel">最低日志级别</param>
    public static LoggerConfiguration ConfigureFileLoggingWithCustomNames(
        this LoggerConfiguration loggerConfiguration,
        string logPath = "Logs",
        string? logFileName = null,
        string? errorFileName = null,
        string? warningFileName = null,
        int retentionDays = 30,
        int maxFileSizeMB = 100,
        LogEventLevel restrictedToMinimumLevel = LogEventLevel.Information)
    {
        // 确保日志目录存在
        if (!Directory.Exists(logPath))
        {
            Directory.CreateDirectory(logPath);
        }

        var fileSizeLimitBytes = maxFileSizeMB * 1024 * 1024;

        // 使用自定义文件名或默认值
        var logFile = string.IsNullOrEmpty(logFileName) ? "log-.txt" : $"{logFileName}-.txt";
        var errorFile = string.IsNullOrEmpty(errorFileName) ? "error-.txt" : $"{errorFileName}-.txt";
        var warningFile = string.IsNullOrEmpty(warningFileName) ? "warning-.txt" : $"{warningFileName}-.txt";

        // 配置文件输出
        loggerConfiguration.WriteTo.File(
            path: Path.Combine(logPath, logFile),
            rollingInterval: RollingInterval.Day,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{ModuleCode}] [LogType:{LogType}] [TenantId:{TenantId}] [UserId:{UserId}] [RequestId:{RequestId}] {Message:lj}{NewLine}{Exception}",
            encoding: System.Text.Encoding.UTF8,
            restrictedToMinimumLevel: restrictedToMinimumLevel,
            fileSizeLimitBytes: fileSizeLimitBytes,
            retainedFileCountLimit: retentionDays,
            rollOnFileSizeLimit: true,
            flushToDiskInterval: TimeSpan.FromSeconds(1)
        );

        // 配置错误日志
        loggerConfiguration.WriteTo.File(
            path: Path.Combine(logPath, errorFile),
            rollingInterval: RollingInterval.Day,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{ModuleCode}] [LogType:{LogType}] [TenantId:{TenantId}] [UserId:{UserId}] [RequestId:{RequestId}] {Message:lj}{NewLine}{Exception}",
            encoding: System.Text.Encoding.UTF8,
            restrictedToMinimumLevel: LogEventLevel.Error,
            fileSizeLimitBytes: fileSizeLimitBytes,
            retainedFileCountLimit: retentionDays,
            rollOnFileSizeLimit: true,
            flushToDiskInterval: TimeSpan.FromSeconds(1)
        );

        // 配置警告日志
        loggerConfiguration.WriteTo.File(
            path: Path.Combine(logPath, warningFile),
            rollingInterval: RollingInterval.Day,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{ModuleCode}] [LogType:{LogType}] [TenantId:{TenantId}] [UserId:{UserId}] [RequestId:{RequestId}] {Message:lj}{NewLine}{Exception}",
            encoding: System.Text.Encoding.UTF8,
            restrictedToMinimumLevel: LogEventLevel.Warning,
            fileSizeLimitBytes: fileSizeLimitBytes,
            retainedFileCountLimit: retentionDays,
            rollOnFileSizeLimit: true,
            flushToDiskInterval: TimeSpan.FromSeconds(1)
        );

        return loggerConfiguration;
    }

    /// <summary>
    /// 使用 SerilogOptions 配置文件日志（支持自定义文件名）
    /// </summary>
    /// <param name="loggerConfiguration">日志配置器</param>
    /// <param name="options">日志配置选项</param>
    public static LoggerConfiguration ConfigureFileLogging(
        this LoggerConfiguration loggerConfiguration,
        SerilogOptions options)
    {
        return ConfigureFileLoggingWithCustomNames(
            loggerConfiguration,
            logPath: options.LogDirectory,
            logFileName: options.CustomLogFileName,
            errorFileName: options.CustomErrorFileName,
            warningFileName: options.CustomWarningFileName,
            retentionDays: options.RetentionDays,
            maxFileSizeMB: options.MaxFileSizeMB);
    }
}

