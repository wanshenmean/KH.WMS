using SqlSugar;
using System.ComponentModel.DataAnnotations;
using KH.WMS.Core.Models.Entities;
using KH.WMS.Core.Database.SqlSugar;

namespace KH.WMS.Config.Abstractions
{
    /// <summary>
    /// 编码生成日志
    /// 记录所有编码的生成历史
    /// </summary>
    [ConfigDb]
    [SugarTable("log_code_record")]
    [SugarIndex("idx_rule", nameof(RuleId), OrderByType.Asc)]
    [SugarIndex("idx_rule_type", nameof(RuleType), OrderByType.Asc)]
    [SugarIndex("idx_generated_no", nameof(GeneratedNo), OrderByType.Asc)]
    [SugarIndex("idx_generated_time", nameof(GeneratedTime), OrderByType.Asc)]
    [SugarIndex("idx_doc_id", nameof(DocId), OrderByType.Asc)]
    public class LogCodeRecord : BaseEntity<long>
    {
        /// <summary>
        /// 规则ID
        /// </summary>
        
        [SugarColumn(IsNullable = false, ColumnDescription = "规则ID")]
        public long RuleId { get; set; }

        /// <summary>
        /// 规则类型
        /// </summary>
        [SugarColumn(Length = 30, IsNullable = true, ColumnDescription = "规则类型")]
        public string? RuleType { get; set; }

        /// <summary>
        /// 生成编码
        /// </summary>
        
        [SugarColumn(Length = 50, IsNullable = false, ColumnDescription = "生成编码")]
        public string GeneratedNo { get; set; } = string.Empty;

        /// <summary>
        /// 序列值
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "序列值")]
        public long? SequenceValue { get; set; }

        /// <summary>
        /// 关联单据ID
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "关联单据ID")]
        public long? DocId { get; set; }

        /// <summary>
        /// 关联单据编号
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnDescription = "关联单据编号")]
        public string? DocNo { get; set; }

        /// <summary>
        /// 生成人
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "生成人")]
        public long? GeneratedBy { get; set; }

        /// <summary>
        /// 生成时间
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "生成时间", DefaultValue = "CURRENT_TIMESTAMP")]
        public DateTime GeneratedTime { get; set; } = DateTime.Now;

        /// <summary>
        /// IP地址
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnDescription = "IP地址")]
        public string? IpAddress { get; set; }
    }
}
