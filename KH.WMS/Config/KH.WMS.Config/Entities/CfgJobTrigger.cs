using SqlSugar;
using System.ComponentModel.DataAnnotations;
using KH.WMS.Core.Models.Entities;
using KH.WMS.Core.Database.SqlSugar;

namespace KH.WMS.Config.Abstractions
{
    /// <summary>
    /// 作业触发器
    /// </summary>
    [ConfigDb]
    [SugarTable("cfg_job_trigger")]
    [SugarIndex("uk_trigger_code", nameof(TriggerCode), OrderByType.Asc, true)]
    [SugarIndex("idx_job", nameof(JobId), OrderByType.Asc)]
    public class CfgJobTrigger : BaseEntity<long>, IEnableDisableEntity
    {

        /// <summary>
        /// 触发器编码
        /// </summary>
        
        [SugarColumn(Length = 50, IsNullable = false, ColumnDescription = "触发器编码")]
        public string TriggerCode { get; set; } = string.Empty;

        /// <summary>
        /// 触发器名称
        /// </summary>
        
        [SugarColumn(Length = 100, IsNullable = false, ColumnDescription = "触发器名称")]
        public string TriggerName { get; set; } = string.Empty;

        /// <summary>
        /// 作业ID
        /// </summary>
        
        [SugarColumn(IsNullable = false, ColumnDescription = "作业ID")]
        public long JobId { get; set; }

        /// <summary>
        /// 触发器类型
        /// </summary>
        
        [SugarColumn(Length = 20, IsNullable = false, ColumnDescription = "触发器类型")]
        public string TriggerType { get; set; } = string.Empty;

        /// <summary>
        /// Cron表达式
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true, ColumnDescription = "Cron表达式")]
        public string? CronExpression { get; set; }

        /// <summary>
        /// Cron描述
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true, ColumnDescription = "Cron描述")]
        public string? CronDescription { get; set; }

        /// <summary>
        /// 重复次数
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "重复次数")]
        public int? RepeatCount { get; set; }

        /// <summary>
        /// 重复间隔
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "重复间隔")]
        public int? RepeatInterval { get; set; }

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
        /// 优先级
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "优先级", DefaultValue = "100")]
        public int Priority { get; set; } = 100;

        /// <summary>
        /// 触发器状态
        /// </summary>
        [SugarColumn(Length = 20, IsNullable = true, ColumnDescription = "触发器状态")]
        public string? TriggerState { get; set; }

        /// <summary>
        /// 已触发次数
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "已触发次数", DefaultValue = "0")]
        public int TimesTriggered { get; set; } = 0;

        /// <summary>
        /// 上次触发时间
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "上次触发时间")]
        public DateTime? LastFireTime { get; set; }

        /// <summary>
        /// 下次触发时间
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "下次触发时间")]
        public DateTime? NextFireTime { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "是否启用", DefaultValue = "1")]
        public byte IsActive { get; set; } = 1;
    }
}
