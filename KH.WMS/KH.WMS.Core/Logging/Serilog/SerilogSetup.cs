using Serilog;
using Serilog.Events;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using KH.WMS.Core.Logging.Serilog.Enricher;
using KH.WMS.Core.Logging.LogEnums;
using System.Collections.Generic;
using Serilog.Core;

namespace KH.WMS.Core.Logging.Serilog;

/// <summary>
/// Serilog 配置
/// </summary>
public static class SerilogSetup
{
    /// <summary>
    /// 配置 Serilog（支持文件大小限制）
    /// </summary>
    public static LoggerConfiguration ConfigureSerilog(
        string appName,
        string logDirectory = "Logs",
        LogEventLevel minimumLevel = LogEventLevel.Information,
        int retentionDays = 30,
        int maxFileSizeMB = 100,
        string? logFileName = null,
        string? errorFileName = null,
        string? warningFileName = null)
    {
        // 确保日志目录存在
        EnsureLogDirectoryExists(logDirectory);

        var fileSizeLimitBytes = maxFileSizeMB * 1024 * 1024L;

        // 使用自定义文件名或默认值
        var logFile = string.IsNullOrEmpty(logFileName) ? "log-.txt" : $"{logFileName}-.txt";
        var errorFile = string.IsNullOrEmpty(errorFileName) ? "error-.txt" : $"{errorFileName}-.txt";
        var warningFile = string.IsNullOrEmpty(warningFileName) ? "warning-.txt" : $"{warningFileName}-.txt";

        return new LoggerConfiguration()
            .MinimumLevel.Is(minimumLevel)
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.With(new LogEnricher())
            .Enrich.WithProperty("Application", appName)
            .WriteTo.Console(outputTemplate: GetConsoleOutputTemplate())
            .WriteTo.File(
                path: $"{logDirectory}/{logFile}",
                rollingInterval: RollingInterval.Day,
                outputTemplate: GetFileOutputTemplate(),
                restrictedToMinimumLevel: LogEventLevel.Information,
                encoding: System.Text.Encoding.UTF8,
                fileSizeLimitBytes: fileSizeLimitBytes,
                retainedFileCountLimit: retentionDays,
                rollOnFileSizeLimit: true,
                flushToDiskInterval: TimeSpan.FromSeconds(1))
            .WriteTo.File(
                path: $"{logDirectory}/{errorFile}",
                rollingInterval: RollingInterval.Day,
                outputTemplate: GetFileOutputTemplate(),
                restrictedToMinimumLevel: LogEventLevel.Error,
                encoding: System.Text.Encoding.UTF8,
                fileSizeLimitBytes: fileSizeLimitBytes,
                retainedFileCountLimit: retentionDays,
                rollOnFileSizeLimit: true,
                flushToDiskInterval: TimeSpan.FromSeconds(1))
            .WriteTo.File(
                path: $"{logDirectory}/{warningFile}",
                rollingInterval: RollingInterval.Day,
                outputTemplate: GetFileOutputTemplate(),
                restrictedToMinimumLevel: LogEventLevel.Warning,
                encoding: System.Text.Encoding.UTF8,
                fileSizeLimitBytes: fileSizeLimitBytes,
                retainedFileCountLimit: retentionDays,
                rollOnFileSizeLimit: true,
                flushToDiskInterval: TimeSpan.FromSeconds(1));
    }

    /// <summary>
    /// 配置 Serilog（使用 SerilogOptions）
    /// </summary>
    public static LoggerConfiguration ConfigureSerilog(
        string appName,
        SerilogOptions options)
    {
        return ConfigureSerilog(
            appName,
            options.LogDirectory,
            GetMinimumLogLevelFromString(options.MinimumLevel),
            options.RetentionDays,
            options.MaxFileSizeMB,
            options.CustomLogFileName,
            options.CustomErrorFileName,
            options.CustomWarningFileName);
    }

    /// <summary>
    /// 添加 Serilog 到主机（完整版，支持文件大小限制和自定义文件名）
    /// </summary>
    public static IHostBuilder AddSerilog(
        this IHostBuilder hostBuilder,
        string appName,
        string logDirectory = "Logs",
        int retentionDays = 30,
        int maxFileSizeMB = 100,
        string? logFileName = null,
        string? errorFileName = null,
        string? warningFileName = null)
    {
        return hostBuilder.UseSerilog((context, services, loggerConfiguration) =>
        {
            var config = context.Configuration;
            var appNameValue = config["App:AppName"] ?? appName;

            // 从配置读取或使用默认值
            var logPath = config["Serilog:LogPath"] ?? logDirectory;
            var retention = int.Parse(config["Serilog:RetentionDays"] ?? retentionDays.ToString());
            var maxSize = int.Parse(config["Serilog:MaxFileSizeMB"] ?? maxFileSizeMB.ToString());
            var fileSizeLimitBytes = maxSize * 1024 * 1024L;

            // 是否输出到控制台
            var writeToConsole = bool.Parse(config["Serilog:WriteToConsole"] ?? "true");

            // 从配置读取自定义文件名（支持通过配置文件设置）
            var customLogFileName = config["Serilog:FileNames:Log"] ?? logFileName;
            var customErrorFileName = config["Serilog:FileNames:Error"] ?? errorFileName;
            var customWarningFileName = config["Serilog:FileNames:Warning"] ?? warningFileName;

            // 使用自定义文件名或默认值
            var logFile = string.IsNullOrEmpty(customLogFileName) ? "log-.txt" : $"{customLogFileName}-.txt";
            var errorFile = string.IsNullOrEmpty(customErrorFileName) ? "error-.txt" : $"{customErrorFileName}-.txt";
            var warningFile = string.IsNullOrEmpty(customWarningFileName) ? "warning-.txt" : $"{customWarningFileName}-.txt";

            // 确保日志目录存在
            EnsureLogDirectoryExists(logPath);

            loggerConfiguration
                .ReadFrom.Configuration(config)
                .ReadFrom.Services(services)
                .MinimumLevel.Is(GetMinimumLogLevel(config))
                .MinimumLevel.Override("Microsoft", GetModuleLogLevel(config, "Microsoft"))
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore", GetModuleLogLevel(config, "EntityFrameworkCore"))
                .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
                .MinimumLevel.Override("System", GetModuleLogLevel(config, "System"))
                .Enrich.FromLogContext()
                .Enrich.With(new LogEnricher())
                .Enrich.With(new ModuleLogEnricher())
                .Enrich.With(new LogTypeEnricher())
                .Enrich.With(new UserLogEnricher(services.GetRequiredService<IHttpContextAccessor>()))
                .Enrich.With(new CorrelationIdEnricher(services.GetRequiredService<IHttpContextAccessor>()))
                .Enrich.WithProperty("Application", appNameValue)
                .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName);

            // 根据配置决定是否输出到控制台
            if (writeToConsole)
            {
                loggerConfiguration.WriteTo.Console(outputTemplate: GetConsoleOutputTemplate());
            }

            // 文件输出
            loggerConfiguration
                // 普通日志 - 只记录 Information 级别
                .WriteTo.File(
                    path: Path.Combine(logPath, logFile),
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: GetFileOutputTemplate(),
                    restrictedToMinimumLevel: LogEventLevel.Information,
                    encoding: System.Text.Encoding.UTF8,
                    fileSizeLimitBytes: fileSizeLimitBytes,
                    retainedFileCountLimit: retention,
                    rollOnFileSizeLimit: true,
                    flushToDiskInterval: TimeSpan.FromSeconds(1))
                // 错误日志 - 只记录 Error 及 Fatal 级别
                .WriteTo.Logger(lc => lc
                    .Filter.ByIncludingOnly(e => e.Level >= LogEventLevel.Error)
                    .WriteTo.File(
                        path: Path.Combine(logPath, errorFile),
                        rollingInterval: RollingInterval.Day,
                        outputTemplate: GetFileOutputTemplate(),
                        encoding: System.Text.Encoding.UTF8,
                        fileSizeLimitBytes: fileSizeLimitBytes,
                        retainedFileCountLimit: retention,
                        rollOnFileSizeLimit: true,
                        flushToDiskInterval: TimeSpan.FromSeconds(1)))
                // 警告日志 - 只记录 Warning 级别（不包括 Error）
                .WriteTo.Logger(lc => lc
                    .Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Warning)
                    .WriteTo.File(
                        path: Path.Combine(logPath, warningFile),
                        rollingInterval: RollingInterval.Day,
                        outputTemplate: GetFileOutputTemplate(),
                        encoding: System.Text.Encoding.UTF8,
                        fileSizeLimitBytes: fileSizeLimitBytes,
                        retainedFileCountLimit: retention,
                        rollOnFileSizeLimit: true,
                        flushToDiskInterval: TimeSpan.FromSeconds(1)))
                // 调试日志 - 只记录 Debug 级别（方法参数/SQL语句/调用链），开发 MinimumLevel=Debug 时生效
                .WriteTo.Logger(lc => lc
                    .Filter.ByIncludingOnly(e => e.Level == LogEventLevel.Debug)
                    .WriteTo.File(
                        path: Path.Combine(logPath, "debug-.txt"),
                        rollingInterval: RollingInterval.Day,
                        outputTemplate: GetFileOutputTemplate(),
                        encoding: System.Text.Encoding.UTF8,
                        fileSizeLimitBytes: fileSizeLimitBytes,
                        retainedFileCountLimit: retention,
                        rollOnFileSizeLimit: true,
                        flushToDiskInterval: TimeSpan.FromSeconds(1)));

            // 按模块分离日志（可选）
            WriteModuleLogs(loggerConfiguration, logPath, fileSizeLimitBytes, retention);

            // 按自定义文件名分离日志
            WriteFileNameLogs(loggerConfiguration, logPath, fileSizeLimitBytes, retention);
        });
    }

    /// <summary>
    /// 按自定义文件名写入日志
    /// </summary>
    private static void WriteFileNameLogs(LoggerConfiguration loggerConfiguration, string logDirectory, long fileSizeLimitBytes, int retentionDays)
    {
        // 使用 Map 动态路由到不同的文件
        loggerConfiguration.WriteTo.Map(
            // 从日志事件中获取 FileName 属性
            logEvent =>
            {
                if (logEvent.Properties.TryGetValue("FileName", out var fileNameValue))
                {
                    return fileNameValue.ToString().Replace("\"", "");
                }
                return "default";  // 没有指定文件名
            },
            // 配置每个路由的目标
            (fileName, wt) =>
            {
                if (fileName == "default")
                {
                    // 默认不写入（已经有主日志文件了）
                    wt.Sink(new NullSink());
                }
                else
                {
                    // 创建自定义文件目录
                    var customLogDir = Path.Combine(logDirectory, "custom");
                    EnsureLogDirectoryExists(customLogDir);

                    // 写入到指定文件名的日志文件
                    wt.File(
                        path: Path.Combine(customLogDir, $"{fileName}-.txt"),
                        rollingInterval: RollingInterval.Day,
                        outputTemplate: GetFileOutputTemplate(),
                        restrictedToMinimumLevel: LogEventLevel.Information,
                        encoding: System.Text.Encoding.UTF8,
                        fileSizeLimitBytes: fileSizeLimitBytes,
                        retainedFileCountLimit: retentionDays,
                        rollOnFileSizeLimit: true,
                        flushToDiskInterval: TimeSpan.FromSeconds(1));
                }
            });
    }

    /// <summary>
    /// 添加 Serilog 到主机（使用 SerilogOptions）
    /// </summary>
    public static IHostBuilder AddSerilog(
        this IHostBuilder hostBuilder,
        string appName,
        SerilogOptions options)
    {
        return AddSerilog(
            hostBuilder,
            appName,
            options.LogDirectory,
            options.RetentionDays,
            options.MaxFileSizeMB,
            options.CustomLogFileName,
            options.CustomErrorFileName,
            options.CustomWarningFileName);
    }

    /// <summary>
    /// 按模块写入日志（动态路由，只写入实际使用的模块）
    /// </summary>
    private static void WriteModuleLogs(LoggerConfiguration loggerConfiguration, string logDirectory, long fileSizeLimitBytes, int retentionDays)
    {
        var wmsLogDir = Path.Combine(logDirectory, "wms");
        EnsureLogDirectoryExists(wmsLogDir);

        // 使用 Map 动态路由到不同的文件
        loggerConfiguration.WriteTo.Map(
            // 从日志事件中获取 ModuleCode 属性
            logEvent =>
            {
                if (logEvent.Properties.TryGetValue("ModuleCode", out var moduleValue))
                {
                    var moduleCode = moduleValue.ToString().Replace("\"", "");
                    // 只处理 WMS 模块（见 WmsLogModules）
                    if (int.TryParse(moduleCode, out var code) && WmsLogModules.IsWmsModuleCode(code))
                    {
                        return moduleCode;  // 返回模块代码作为路由键
                    }
                }
                return "default";  // 默认路由
            },
            // 配置每个路由的目标
            (moduleCode, wt) =>
            {
                if (moduleCode == "default")
                {
                    // 默认不写入（已经有主日志文件了）
                    wt.Sink(new NullSink());
                }
                else
                {
                    // 根据模块代码获取模块名称
                    var moduleName = GetModuleNameByCode(moduleCode);
                    wt.File(
                        path: Path.Combine(wmsLogDir, $"{moduleName}-.txt"),
                        rollingInterval: RollingInterval.Day,
                        outputTemplate: GetFileOutputTemplate(),
                        restrictedToMinimumLevel: LogEventLevel.Information,
                        encoding: System.Text.Encoding.UTF8,
                        fileSizeLimitBytes: fileSizeLimitBytes,
                        retainedFileCountLimit: retentionDays,
                        rollOnFileSizeLimit: true,
                        flushToDiskInterval: TimeSpan.FromSeconds(1));
                }
            });
    }

    /// <summary>
    /// 根据模块代码获取模块名称
    /// </summary>
    private static string GetModuleNameByCode(string moduleCode)
    {
        return WmsLogModules.CodeToName.TryGetValue(moduleCode, out var moduleName) ? moduleName : "Unknown";
    }

    /// <summary>
    /// 确保日志目录存在
    /// </summary>
    private static void EnsureLogDirectoryExists(string logDirectory)
    {
        if (!Directory.Exists(logDirectory))
        {
            Directory.CreateDirectory(logDirectory);
        }
    }

    /// <summary>
    /// 获取控制台输出模板
    /// </summary>
    private static string GetConsoleOutputTemplate()
    {
        return "[{Timestamp:HH:mm:ss} {Level:u3}] [{ModuleCode}] {Message:lj}{NewLine}{Exception}";
    }

    /// <summary>
    /// 获取文件输出模板
    /// </summary>
    private static string GetFileOutputTemplate()
    {
        return "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{ModuleCode}] [LogType:{LogType}] [TenantId:{TenantId}] [UserId:{UserId}] [RequestId:{RequestId}] {Message:lj}{NewLine}{Exception}";
    }

    /// <summary>
    /// 获取最低日志级别
    /// </summary>
    private static LogEventLevel GetMinimumLogLevel(IConfiguration config)
    {
        var level = config["Serilog:MinimumLevel:Default"];
        return level switch
        {
            "Verbose" => LogEventLevel.Verbose,
            "Debug" => LogEventLevel.Debug,
            "Information" => LogEventLevel.Information,
            "Warning" => LogEventLevel.Warning,
            "Error" => LogEventLevel.Error,
            "Fatal" => LogEventLevel.Fatal,
            _ => LogEventLevel.Information
        };
    }

    /// <summary>
    /// 获取模块日志级别
    /// </summary>
    private static LogEventLevel GetModuleLogLevel(IConfiguration config, string module)
    {
        var level = config[$"Serilog:MinimumLevel:Override:{module}"];
        return level switch
        {
            "Verbose" => LogEventLevel.Verbose,
            "Debug" => LogEventLevel.Debug,
            "Information" => LogEventLevel.Information,
            "Warning" => LogEventLevel.Warning,
            "Error" => LogEventLevel.Error,
            "Fatal" => LogEventLevel.Fatal,
            _ => LogEventLevel.Warning
        };
    }

    /// <summary>
    /// 从字符串获取日志级别
    /// </summary>
    private static LogEventLevel GetMinimumLogLevelFromString(string level)
    {
        return level switch
        {
            "Verbose" => LogEventLevel.Verbose,
            "Debug" => LogEventLevel.Debug,
            "Information" => LogEventLevel.Information,
            "Warning" => LogEventLevel.Warning,
            "Error" => LogEventLevel.Error,
            "Fatal" => LogEventLevel.Fatal,
            _ => LogEventLevel.Information
        };
    }
}

/// <summary>
/// 空 Sink（用于不写入的默认路由）
/// </summary>
public class NullSink : ILogEventSink
{
    public void Emit(LogEvent logEvent)
    {
        // 什么都不做，丢弃日志事件
    }
}


