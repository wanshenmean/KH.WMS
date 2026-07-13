using SqlSugar;
using System.ComponentModel.DataAnnotations;
using KH.WMS.Core.Models.Entities;
using KH.WMS.Core.Database.SqlSugar;

namespace KH.WMS.Config.Abstractions
{
    /// <summary>
    /// 编码序列
    /// 管理各编码规则的序列号计数
    /// </summary>
    [ConfigDb]
    [SugarTable("cfg_code_sequence")]
    [SugarIndex("uk_sequence_key", nameof(RuleId), OrderByType.Asc, nameof(SequenceKey), OrderByType.Asc, true)]
    [SugarIndex("idx_rule", nameof(RuleId), OrderByType.Asc)]
    [SugarIndex("idx_period", nameof(PeriodStart), OrderByType.Asc, nameof(PeriodEnd), OrderByType.Asc)]
    public class CfgCodeSequence : BaseEntity<long>
    {
        /// <summary>
        /// 规则ID
        /// </summary>
        
        [SugarColumn(IsNullable = false, ColumnDescription = "规则ID")]
        public long RuleId { get; set; }

        /// <summary>
        /// 序列键
        /// </summary>
        
        [SugarColumn(Length = 100, IsNullable = false, ColumnDescription = "序列键")]
        public string SequenceKey { get; set; } = string.Empty;

        /// <summary>
        /// 当前值
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "当前值", DefaultValue = "0")]
        public long CurrentValue { get; set; } = 0;

        /// <summary>
        /// 周期开始日期
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "周期开始日期")]
        public DateOnly? PeriodStart { get; set; }

        /// <summary>
        /// 周期结束日期
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "周期结束日期")]
        public DateOnly? PeriodEnd { get; set; }

        /// <summary>
        /// 已生成数量
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "已生成数量", DefaultValue = "0")]
        public int GeneratedCount { get; set; } = 0;

        /// <summary>
        /// 最后生成时间
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "最后生成时间")]
        public DateTime? LastGeneratedTime { get; set; }
    }
}
