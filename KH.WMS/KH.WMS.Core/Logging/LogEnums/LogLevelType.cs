using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KH.WMS.Core.Logging.LogEnums
{
    /// <summary>
    /// 日志级别枚举
    /// </summary>
    public enum LogLevelType
    {
        /// <summary>
        /// 详细跟踪 - 每一行SQL执行、详细流程跟踪
        /// </summary>
        Verbose = 0,

        /// <summary>
        /// 调试信息 - 方法参数、中间变量
        /// </summary>
        Debug = 1,

        /// <summary>
        /// 一般信息 - 用户登录、单据创建
        /// </summary>
        Information = 2,

        /// <summary>
        /// 警告 - API调用慢、缓存未命中，需关注
        /// </summary>
        Warning = 3,

        /// <summary>
        /// 错误 - HTTP 500、业务异常，但系统可继续
        /// </summary>
        Error = 4,

        /// <summary>
        /// 致命错误 - 数据库连接失败、磁盘满，系统无法继续
        /// </summary>
        Fatal = 5,

        /// <summary>
        /// 无
        /// </summary>
        None = 6
    }

}
