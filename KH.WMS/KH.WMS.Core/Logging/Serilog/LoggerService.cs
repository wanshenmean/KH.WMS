using System;
using System.Text.Json;
using KH.WMS.Core.Constants;
using KH.WMS.Core.DependencyInjection.ServiceLifetimes;
using KH.WMS.Core.Logging.LogEnums;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SerilogLogContext = Serilog.Context.LogContext;

namespace KH.WMS.Core.Logging.Serilog;

/// <summary>
/// 日志服务实现（自动识别模块）
/// </summary>
[RegisteredService(Lifetime = ServiceLifetime.Scoped, WithoutInterceptor = true)]
public class LoggerService : ILoggerService
{
    private readonly ILogger<LoggerService> _logger;
    private readonly IHttpContextAccessor? _httpContextAccessor;

    public LoggerService(ILogger<LoggerService> logger, IHttpContextAccessor? httpContextAccessor = null)
    {
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public void LogVerbose(string message, params object?[] args)
    {
        var module = LogModuleDetector.DetectModule();
        WriteLog(LogLevel.Trace, message, module, null, args);
    }

    public void LogDebug(string message, params object?[] args)
    {
        var module = LogModuleDetector.DetectModule();
        WriteLog(LogLevel.Debug, message, module, null, args);
    }

    public void LogInfo(string message, params object?[] args)
    {
        var module = LogModuleDetector.DetectModule();
        WriteLog(LogLevel.Information, message, module, null, args);
    }

    public void LogWarning(string message, params object?[] args)
    {
        var module = LogModuleDetector.DetectModule();
        WriteLog(LogLevel.Warning, message, module, null, args);
    }

    public void LogError(string message, params object?[] args)
    {
        var module = LogModuleDetector.DetectModule();
        WriteLog(LogLevel.Error, message, module, null, args);
    }

    public void LogError(Exception exception, string message, params object?[] args)
    {
        var module = LogModuleDetector.DetectModule();
        var context = CreateLogContext(LogLevel.Error, module);
        context.Type = LogType.Exception;

        using (PushFileName(null))
        {
            if (args != null && args.Length > 0)
            {
                var messageTemplate = $"[{GetModuleCode(module)}] {message}";
                _logger.LogError(exception, messageTemplate, args);
            }
            else
            {
                _logger.LogError(exception, "[{ModuleCode}] {Message}", GetModuleCode(module), message);
            }
        }
    }

    public void LogFatal(string message, params object?[] args)
    {
        var module = LogModuleDetector.DetectModule();
        WriteLog(LogLevel.Critical, message, module, null, args);
    }

    public void LogFatal(Exception exception, string message, params object?[] args)
    {
        var module = LogModuleDetector.DetectModule();
        var context = CreateLogContext(LogLevel.Critical, module);
        context.Type = LogType.Exception;

        using (PushFileName(null))
        {
            if (args != null && args.Length > 0)
            {
                var messageTemplate = $"[{GetModuleCode(module)}] {message}";
                _logger.LogCritical(exception, messageTemplate, args);
            }
            else
            {
                _logger.LogCritical(exception, "[{ModuleCode}] {Message}", GetModuleCode(module), message);
            }
        }
    }

    public void LogOperation(string operation, string? userName = null, long? userId = null, object? data = null)
    {
        var module = LogModuleDetector.DetectModule();
        var context = CreateLogContext(LogLevel.Information, module);
        context.Type = LogType.Operation;
        context.Operation = operation;
        context.UserName = userName ?? context.UserName;
        context.UserId = userId ?? context.UserId;

        var dataJson = data != null ? JsonSerializer.Serialize(data) : null;

        using (PushFileName(null))
        {
            _logger.LogInformation("[{ModuleCode}] [OPERATION] {Operation} by {UserName} (UserId:{UserId}) {Data}",
                GetModuleCode(module), operation, context.UserName, context.UserId, dataJson ?? "");
        }
    }

    public void LogBusiness(string businessType, string message, object? data = null)
    {
        var module = LogModuleDetector.DetectModule();
        var context = CreateLogContext(LogLevel.Information, module);
        context.Type = LogType.Business;
        context.Operation = businessType;

        var dataJson = data != null ? JsonSerializer.Serialize(data) : null;

        using (PushFileName(null))
        {
            _logger.LogInformation("[{ModuleCode}] [BUSINESS:{Type}] {Message} {Data}",
                GetModuleCode(module), businessType, message, dataJson ?? "");
        }
    }

    public void LogPerformance(string operation, long elapsedMs, object? data = null)
    {
        var module = LogModuleDetector.DetectModule();
        var context = CreateLogContext(LogLevel.Information, module);
        context.Type = LogType.Performance;
        context.Operation = operation;
        context.Data["ElapsedMs"] = elapsedMs;
        context.Data["ElapsedSeconds"] = elapsedMs / 1000.0;

        if (elapsedMs > 3000)
        {
            context.Level = LogLevelType.Warning;
        }

        var dataJson = data != null ? JsonSerializer.Serialize(data) : null;
        var logLevel = elapsedMs > 3000 ? LogLevel.Warning : LogLevel.Information;

        using (PushFileName(null))
        {
            _logger.Log(logLevel, "[{ModuleCode}] [PERFORMANCE] {Operation} 耗时 {Elapsed}ms {Data}",
                GetModuleCode(module), operation, elapsedMs, dataJson ?? "");
        }
    }

    public void Log(LogContext context, string message, params object?[] args)
    {
        var logLevel = ConvertLogLevel(context.Level);
        var messageFormat = $"[{GetModuleCode(context.Module)}] [LogType:{context.Type}] {message}";

        var state = new Dictionary<string, object?>
        {
            ["UserId"] = context.UserId,
            ["UserName"] = context.UserName,
            ["TenantId"] = context.TenantId,
            ["RequestId"] = context.RequestId,
            ["Operation"] = context.Operation
        };

        using (_logger.BeginScope(state))
        {
            _logger.Log(logLevel, messageFormat, args);
        }
    }

    // ==================== 支持指定文件名的 ToFile 方法 ====================

    public void LogInfoToFile(string fileName, string message, params object?[] args)
    {
        var module = LogModuleDetector.DetectModule();
        WriteLog(LogLevel.Information, message, module, fileName, args);
    }

    public void LogErrorToFile(string fileName, string message, params object?[] args)
    {
        var module = LogModuleDetector.DetectModule();
        WriteLog(LogLevel.Error, message, module, fileName, args);
    }

    public void LogErrorToFile(string fileName, Exception exception, string message, params object?[] args)
    {
        var module = LogModuleDetector.DetectModule();
        var context = CreateLogContext(LogLevel.Error, module);
        context.Type = LogType.Exception;

        using (PushFileName(fileName))
        {
            if (args != null && args.Length > 0)
            {
                var messageTemplate = $"[{GetModuleCode(module)}] {message}";
                _logger.LogError(exception, messageTemplate, args);
            }
            else
            {
                _logger.LogError(exception, "[{ModuleCode}] {Message}", GetModuleCode(module), message);
            }
        }
    }

    public void LogOperationToFile(string fileName, string operation, string? userName = null, long? userId = null, object? data = null)
    {
        var module = LogModuleDetector.DetectModule();
        var context = CreateLogContext(LogLevel.Information, module);
        context.Type = LogType.Operation;
        context.Operation = operation;
        context.UserName = userName ?? context.UserName;
        context.UserId = userId ?? context.UserId;

        var dataJson = data != null ? JsonSerializer.Serialize(data) : null;

        using (PushFileName(fileName))
        {
            _logger.LogInformation("[{ModuleCode}] [OPERATION] {Operation} by {UserName} (UserId:{UserId}) {Data}",
                GetModuleCode(module), operation, context.UserName, context.UserId, dataJson ?? "");
        }
    }

    public void LogBusinessToFile(string fileName, string businessType, string message, object? data = null)
    {
        var module = LogModuleDetector.DetectModule();
        var context = CreateLogContext(LogLevel.Information, module);
        context.Type = LogType.Business;
        context.Operation = businessType;

        var dataJson = data != null ? JsonSerializer.Serialize(data) : null;

        using (PushFileName(fileName))
        {
            _logger.LogInformation("[{ModuleCode}] [BUSINESS:{Type}] {Message} {Data}",
                GetModuleCode(module), businessType, message, dataJson ?? "");
        }
    }

    // ==================== 私有方法 ====================

    private void WriteLog(LogLevel level, string message, LogModule module, string? fileName, object?[] args)
    {
        using (PushFileName(fileName))
        {
            if (args != null && args.Length > 0)
            {
                var messageTemplate = $"[{GetModuleCode(module)}] {message}";
                _logger.Log(level, messageTemplate, args);
            }
            else
            {
                _logger.Log(level, $"[{GetModuleCode(module)}] {message}");
            }
        }
    }

    private IDisposable? PushFileName(string? fileName)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            return null;
        }

        return SerilogLogContext.PushProperty("FileName", fileName);
    }

    private LogContext CreateLogContext(LogLevel level, LogModule module)
    {
        var context = new LogContext
        {
            Level = ConvertToLogLevelType(level),
            Module = module
        };

        if (_httpContextAccessor?.HttpContext != null)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var user = httpContext.User;

            var userIdClaim = user?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (long.TryParse(userIdClaim, out var userId))
            {
                context.UserId = userId;
            }

            context.UserName = user?.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;

            var tenantIdClaim = user?.FindFirst("TenantId")?.Value;
            if (long.TryParse(tenantIdClaim, out var tenantId))
            {
                context.TenantId = tenantId;
            }

            context.RequestId = httpContext.TraceIdentifier;
            context.CorrelationId = httpContext.Request.Headers[HeaderConstants.Tracing.X_CORRELATION_ID].FirstOrDefault();
            context.ClientIp = httpContext.Connection.RemoteIpAddress?.ToString();
        }

        return context;
    }

    private LogLevel ConvertLogLevel(LogLevelType level)
    {
        return level switch
        {
            LogLevelType.Verbose => LogLevel.Trace,
            LogLevelType.Debug => LogLevel.Debug,
            LogLevelType.Information => LogLevel.Information,
            LogLevelType.Warning => LogLevel.Warning,
            LogLevelType.Error => LogLevel.Error,
            LogLevelType.Fatal => LogLevel.Critical,
            LogLevelType.None => LogLevel.None,
            _ => LogLevel.Information
        };
    }

    private LogLevelType ConvertToLogLevelType(LogLevel level)
    {
        return level switch
        {
            Microsoft.Extensions.Logging.LogLevel.Trace => LogLevelType.Verbose,
            Microsoft.Extensions.Logging.LogLevel.Debug => LogLevelType.Debug,
            Microsoft.Extensions.Logging.LogLevel.Information => LogLevelType.Information,
            Microsoft.Extensions.Logging.LogLevel.Warning => LogLevelType.Warning,
            Microsoft.Extensions.Logging.LogLevel.Error => LogLevelType.Error,
            Microsoft.Extensions.Logging.LogLevel.Critical => LogLevelType.Fatal,
            Microsoft.Extensions.Logging.LogLevel.None => LogLevelType.None,
            _ => LogLevelType.Information
        };
    }

    private string GetModuleCode(LogModule module)
    {
        return ((int)module).ToString() + " " + module;
    }

    private string GetModuleName(LogModule module)
    {
        return module.ToString();
    }
}
