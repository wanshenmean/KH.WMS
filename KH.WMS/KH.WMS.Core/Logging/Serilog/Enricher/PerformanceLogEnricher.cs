using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog.Core;
using Serilog.Events;

namespace KH.WMS.Core.Logging.Serilog.Enricher
{

    /// <summary>
    /// 性能日志增强器
    /// </summary>
    public class PerformanceLogEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            // 如果日志类型是性能日志，添加额外的性能信息
            if (logEvent.Properties.TryGetValue("LogType", out var logTypeValue))
            {
                var logType = logTypeValue.ToString().Replace("\"", "");

                if (logType == "Performance")
                {
                    // 计算内存使用
                    var process = System.Diagnostics.Process.GetCurrentProcess();
                    var memoryUsed = process.WorkingSet64 / 1024 / 1024; // MB

                    logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("MemoryUsedMB", memoryUsed));
                }
            }
        }
    }

}
