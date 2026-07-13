using SqlSugar;
using System.ComponentModel.DataAnnotations;
using KH.WMS.Core.Models.Entities;
using KH.WMS.Entities.Constants;

namespace KH.WMS.Entities.Task
{
    /// <summary>
    /// 任务确认记录（整托确认，自动化立库场景）
    /// </summary>
    [SugarTable("bd_task_confirm")]
    [SugarIndex("idx_task_header", nameof(TaskHeaderId), OrderByType.Asc)]
    [SugarIndex("idx_confirm_no", nameof(ConfirmNo), OrderByType.Asc)]
    [SugarIndex("idx_confirmed_time", nameof(ConfirmedTime), OrderByType.Asc)]
    public class TaskConfirm : BaseEntity<long>
    {
        /// <summary>
        /// 任务头ID
        /// </summary>
        
        [SugarColumn(IsNullable = false, ColumnDescription = "任务头ID")]
        public long TaskHeaderId { get; set; }

        /// <summary>
        /// 确认编号
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnDescription = "确认编号")]
        public string? ConfirmNo { get; set; }

        /// <summary>
        /// 确认来源（WCS / MANUAL）
        /// </summary>
        [SugarColumn(Length = 20, IsNullable = false, ColumnDescription = "确认来源", DefaultValue = BizConstants.ConfirmSource.WCS)]
        public string ConfirmSource { get; set; } = BizConstants.ConfirmSource.WCS;

        /// <summary>
        /// 是否异常（0-正常 1-异常）
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "是否异常", DefaultValue = "0")]
        public byte IsException { get; set; }

        /// <summary>
        /// 异常类型
        /// </summary>
        [SugarColumn(Length = 30, IsNullable = true, ColumnDescription = "异常类型")]
        public string? ExceptionType { get; set; }

        /// <summary>
        /// 异常原因
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnDescription = "异常原因")]
        public string? ExceptionReason { get; set; }

        /// <summary>
        /// 异常处理
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnDescription = "异常处理")]
        public string? ExceptionHandle { get; set; }

        /// <summary>
        /// 执行设备编码（堆垛机/AGV/输送线）
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnDescription = "执行设备编码")]
        public string? EquipmentCode { get; set; }

        /// <summary>
        /// 确认人（人工确认时填写）
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "确认人")]
        public long? ConfirmedBy { get; set; }

        /// <summary>
        /// 确认人姓名
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnDescription = "确认人姓名")]
        public string? ConfirmedByName { get; set; }

        /// <summary>
        /// 确认时间
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "确认时间", DefaultValue = "CURRENT_TIMESTAMP")]
        public DateTime ConfirmedTime { get; set; }

        /// <summary>
        /// WCS任务编号
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnDescription = "WCS任务编号")]
        public string? WcsTaskNo { get; set; }

        /// <summary>
        /// WCS实际到达位置
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnDescription = "WCS实际到达位置")]
        public string? WcsActualPosition { get; set; }

        /// <summary>
        /// WCS行走时间(秒)
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "WCS行走时间(秒)")]
        public int? WcsTravelTime { get; set; }

        /// <summary>
        /// WCS称重
        /// </summary>
        [SugarColumn(DecimalDigits = 3, Length = 10, IsNullable = true, ColumnDescription = "WCS称重")]
        public decimal? WcsWeight { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnDescription = "备注")]
        public string? Remark { get; set; }
    }
}
