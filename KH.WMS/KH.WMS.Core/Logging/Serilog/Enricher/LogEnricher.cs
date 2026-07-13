using Microsoft.AspNetCore.Http;
using Serilog.Core;
using Serilog.Events;

namespace KH.WMS.Core.Logging.Serilog.Enricher;

/// <summary>
/// 日志增强器 - 添加自定义属性
/// </summary>
public class LogEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        // 添加机器信息
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("MachineName", Environment.MachineName));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("ProcessId", Environment.ProcessId));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("ThreadId", Environment.CurrentManagedThreadId));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("AppDomain", AppDomain.CurrentDomain.FriendlyName));

        // 添加应用信息
        var appName = AppContext.BaseDirectory.Split('\\', '/').LastOrDefault(s => !string.IsNullOrEmpty(s)) ?? "Application";
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("AppName", appName));
    }
}
