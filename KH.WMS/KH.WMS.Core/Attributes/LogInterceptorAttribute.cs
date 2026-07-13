using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace KH.WMS.Core.Attributes
{
    /// <summary>
    /// 日志拦截器特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class LogInterceptorAttribute : Attribute
    {
        /// <summary>
        /// 是否记录参数
        /// </summary>
        public bool LogParameters { get; set; } = true;

        /// <summary>
        /// 是否记录返回值
        /// </summary>
        public bool LogReturnValue { get; set; } = false;

        /// <summary>
        /// 是否记录执行时间
        /// </summary>
        public bool LogExecutionTime { get; set; } = true;

        /// <summary>
        /// 日志级别
        /// </summary>
        public LogLevel LogLevel { get; set; } = LogLevel.Information;
    }

}
