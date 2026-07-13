using KH.WMS.Core.Models.Entities;
using SqlSugar;
using System.ComponentModel.DataAnnotations;

namespace KH.WMS.Engines.DataMap
{
    /// <summary>
    /// 同步日志明细
    /// </summary>
    [SugarTable("int_sync_log_detail")]
    [SugarIndex("idx_sync", nameof(SyncId), OrderByType.Asc)]
    [SugarIndex("idx_result", nameof(SyncResult), OrderByType.Asc)]
    public class IntSyncLogDetail : BaseEntity<long>
    {

        /// <summary>
        /// 同步ID
        /// </summary>
        
        [SugarColumn(IsNullable = false, ColumnDescription = "同步ID")]
        public long SyncId { get; set; }

        /// <summary>
        /// 数据ID
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "数据ID")]
        public long? DataId { get; set; }

        /// <summary>
        /// 数据编码
        /// </summary>
        
        [SugarColumn(IsNullable = false, Length = 50, ColumnDescription = "数据编码")]
        public string DataCode { get; set; }

        /// <summary>
        /// 同步结果
        /// </summary>
        [SugarColumn(IsNullable = true, Length = 20, ColumnDescription = "同步结果")]
        public string SyncResult { get; set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        [SugarColumn(IsNullable = true, Length = 500, ColumnDescription = "错误信息")]
        public string ErrorMessage { get; set; }
    }
}
