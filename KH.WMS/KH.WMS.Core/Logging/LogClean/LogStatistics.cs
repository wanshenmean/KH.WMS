using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KH.WMS.Core.Logging.LogClean
{

    /// <summary>
    /// 日志统计信息
    /// </summary>
    public class LogStatistics
    {
        /// <summary>
        /// 文件日志总数
        /// </summary>
        public int FileCount { get; set; }

        /// <summary>
        /// 文件日志总大小（MB）
        /// </summary>
        public decimal FileSizeMB { get; set; }

        /// <summary>
        /// 数据库日志总数
        /// </summary>
        public long DatabaseLogCount { get; set; }

        /// <summary>
        /// 最旧日志日期
        /// </summary>
        public DateTime? OldestLogDate { get; set; }

        /// <summary>
        /// 最新日志日期
        /// </summary>
        public DateTime? NewestLogDate { get; set; }
    }

}
