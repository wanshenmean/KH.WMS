using Serilog.Core;
using Serilog.Events;
using KH.WMS.Core.Logging.LogEnums;

namespace KH.WMS.Core.Logging.Serilog.Enricher;

/// <summary>
/// 日志类型增强器
/// </summary>
public class LogTypeEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        // 从日志上下文获取 LogType
        if (logEvent.Properties.TryGetValue("LogType", out var logTypeValue))
        {
            var logType = logTypeValue.ToString().Replace("\"", "");
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("LogType", logType));
        }
        else
        {
            // 默认值
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("LogType", LogType.System.ToString()));
        }
    }
}
