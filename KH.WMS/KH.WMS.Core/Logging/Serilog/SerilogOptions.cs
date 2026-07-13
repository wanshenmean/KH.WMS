using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KH.WMS.Core.Logging.Serilog
{
    /// <summary>
    /// 日志配置选项
    /// </summary>
    public class SerilogOptions
    {
        /// <summary>
        /// 最低日志级别
        /// </summary>
        public string MinimumLevel { get; set; } = "Information";

        /// <summary>
        /// 日志目录
        /// </summary>
        public string LogDirectory { get; set; } = "Logs";

        /// <summary>
        /// 日志保留天数
        /// </summary>
        public int RetentionDays { get; set; } = 30;

        /// <summary>
        /// 单个文件最大大小（MB）
        /// </summary>
        public int MaxFileSizeMB { get; set; } = 100;

        /// <summary>
        /// 是否按模块分离日志
        /// </summary>
        public bool SplitByModule { get; set; } = false;

        /// <summary>
        /// 是否输出到控制台
        /// </summary>
        public bool WriteToConsole { get; set; } = true;

        /// <summary>
        /// 是否输出到文件
        /// </summary>
        public bool WriteToFile { get; set; } = true;

        /// <summary>
        /// 模块级别覆盖
        /// </summary>
        public Dictionary<string, string> ModuleOverrides { get; set; } = new()
        {
            ["Microsoft"] = "Warning",
            ["Microsoft.EntityFrameworkCore"] = "Warning",
            ["System"] = "Warning"
        };

        /// <summary>
        /// 自定义日志文件名（不含路径和扩展名，例如: "myapp" 会生成 "myapp-.txt"）
        /// 为 null 时使用默认文件名
        /// </summary>
        public string? CustomLogFileName { get; set; }

        /// <summary>
        /// 自定义错误日志文件名（不含路径和扩展名）
        /// 为 null 时使用默认文件名 "error"
        /// </summary>
        public string? CustomErrorFileName { get; set; }

        /// <summary>
        /// 自定义警告日志文件名（不含路径和扩展名）
        /// 为 null 时使用默认文件名 "warning"
        /// </summary>
        public string? CustomWarningFileName { get; set; }
    }
}
