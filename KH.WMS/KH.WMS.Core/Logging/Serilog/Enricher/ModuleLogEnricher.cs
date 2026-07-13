using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KH.WMS.Core.Logging.LogEnums;
using Serilog.Core;
using Serilog.Events;

namespace KH.WMS.Core.Logging.Serilog.Enricher
{

    /// <summary>
    /// 模块日志增强器
    /// </summary>
    public class ModuleLogEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            // 从日志上下文获取模块信息
            if (logEvent.Properties.TryGetValue("Module", out var moduleValue))
            {
                var moduleCode = moduleValue.ToString().Replace("\"", "");
                logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("ModuleCode", moduleCode));

                // 尝试获取模块名称
                if (Enum.TryParse<LogModule>(moduleValue.ToString(), out var module))
                {
                    logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("ModuleName", module.ToString()));
                }
            }
        }
    }

}
