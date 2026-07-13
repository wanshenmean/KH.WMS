using KH.WMS.Core.Models.Entities;
using SqlSugar;
using System.ComponentModel.DataAnnotations;

namespace KH.WMS.Engines.DataMap
{
    /// <summary>
    /// 同步日志
    /// </summary>
    [SugarTable("int_sync_log")]
    [SugarIndex("idx_type", nameof(SyncType), OrderByType.Asc)]
    [SugarIndex("idx_time", nameof(StartTime), OrderByType.Asc)]
    public class IntSyncLog : BaseEntity<long>
    {

        /// <summary>
        /// 同步类型
        /// </summary>
        
        [SugarColumn(IsNullable = false, Length = 30, ColumnDescription = "同步类型")]
        public string SyncType { get; set; }

        /// <summary>
        /// 来源系统
        /// </summary>
        [SugarColumn(IsNullable = true, Length = 20, ColumnDescription = "来源系统")]
        public string SourceSystem { get; set; }

        /// <summary>
        /// 目标系统
        /// </summary>
        [SugarColumn(IsNullable = true, Length = 20, ColumnDescription = "目标系统")]
        public string TargetSystem { get; set; }

        /// <summary>
        /// 数据类型
        /// </summary>
        [SugarColumn(IsNullable = true, Length = 30, ColumnDescription = "数据类型")]
        public string DataType { get; set; }

        /// <summary>
        /// 总数
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "总数")]
        public int? TotalCount { get; set; }

        /// <summary>
        /// 成功数
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "成功数")]
        public int? SuccessCount { get; set; }

        /// <summary>
        /// 失败数
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "失败数")]
        public int? FailedCount { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "开始时间")]
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "结束时间")]
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        [SugarColumn(IsNullable = true, Length = 20, ColumnDescription = "状态", DefaultValue = DataMapConstants.CallLogStatus.SUCCESS)]
        public string Status { get; set; }
    }
}
