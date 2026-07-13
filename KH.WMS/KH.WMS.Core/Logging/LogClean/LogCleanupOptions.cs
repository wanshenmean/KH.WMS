using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KH.WMS.Core.Logging.LogClean
{
    /// <summary>
    /// 日志清理配置
    /// </summary>
    public class LogCleanupOptions
    {
        /// <summary>
        /// 日志保留天数（默认30天）
        /// </summary>
        public int RetentionDays { get; set; } = 30;

        /// <summary>
        /// 单个日志文件最大大小（MB，默认100MB）
        /// </summary>
        public int MaxFileSizeMB { get; set; } = 100;

        /// <summary>
        /// 日志目录路径（默认 Logs）
        /// </summary>
        public string LogPath { get; set; } = "Logs";

        /// <summary>
        /// 是否启用自动清理
        /// </summary>
        public bool EnableAutoCleanup { get; set; } = true;

        /// <summary>
        /// 自动清理时间（默认凌晨2点）
        /// </summary>
        public string CleanupTime { get; set; } = "02:00";

        /// <summary>
        /// 是否清理数据库日志
        /// </summary>
        public bool CleanupDatabaseLogs { get; set; } = true;

        /// <summary>
        /// 数据库日志表名
        /// </summary>
        public string DatabaseLogTable { get; set; } = "SysLog";

        /// <summary>
        /// 每次清理的数据库日志数量
        /// </summary>
        public int DatabaseCleanupBatchSize { get; set; } = 1000;
    }

}
