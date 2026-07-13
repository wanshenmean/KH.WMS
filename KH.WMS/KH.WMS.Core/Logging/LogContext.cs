using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KH.WMS.Core.Logging.LogEnums;

namespace KH.WMS.Core.Logging
{

    /// <summary>
    /// 日志上下文
    /// </summary>
    public class LogContext
    {
        /// <summary>
        /// 日志级别
        /// </summary>
        public LogLevelType Level { get; set; } = LogLevelType.Information;

        /// <summary>
        /// 日志模块
        /// </summary>
        public LogModule Module { get; set; }

        /// <summary>
        /// 日志类型
        /// </summary>
        public LogType Type { get; set; } = LogType.System;

        /// <summary>
        /// 用户ID
        /// </summary>
        public long? UserId { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string? UserName { get; set; }

        /// <summary>
        /// 租户ID
        /// </summary>
        public long? TenantId { get; set; }

        /// <summary>
        /// 请求ID
        /// </summary>
        public string? RequestId { get; set; }

        /// <summary>
        /// 关联ID
        /// </summary>
        public string? CorrelationId { get; set; }

        /// <summary>
        /// 客户端IP
        /// </summary>
        public string? ClientIp { get; set; }

        /// <summary>
        /// 操作类型
        /// </summary>
        public string? Operation { get; set; }

        /// <summary>
        /// 业务数据
        /// </summary>
        public Dictionary<string, object?> Data { get; set; } = new();

        /// <summary>
        /// 扩展属性
        /// </summary>
        public Dictionary<string, object?> Properties { get; set; } = new();
    }

}
