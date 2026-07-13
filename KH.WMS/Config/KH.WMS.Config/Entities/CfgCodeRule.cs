using SqlSugar;
using System.ComponentModel.DataAnnotations;
using KH.WMS.Core.Models.Entities;
using KH.WMS.Core.Database.SqlSugar;

namespace KH.WMS.Config.Abstractions
{
    /// <summary>
    /// 编码生成规则
    /// 统一管理物料编码、容器编码、单据编号等各类编码的生成规则
    /// </summary>
    [ConfigDb]
    [SugarTable("cfg_code_rule")]
    [SugarIndex("uk_rule_code", nameof(RuleCode), OrderByType.Asc, true)]
    [SugarIndex("idx_rule_type", nameof(RuleType), OrderByType.Asc)]
    [SugarIndex("idx_is_default", nameof(IsDefault), OrderByType.Asc)]
    [SugarIndex("idx_is_active", nameof(IsActive), OrderByType.Asc)]
    [SugarIndex("idx_effective_date", nameof(EffectiveDate), OrderByType.Asc, nameof(ExpiryDate), OrderByType.Asc)]
    public class CfgCodeRule : BaseEntity<long>, IEnableDisableEntity
    {
        /// <summary>
        /// 规则编码
        /// </summary>
        
        [SugarColumn(Length = 50, IsNullable = false, ColumnDescription = "规则编码")]
        public string RuleCode { get; set; } = string.Empty;

        /// <summary>
        /// 规则名称
        /// </summary>
        
        [SugarColumn(Length = 100, IsNullable = false, ColumnDescription = "规则名称")]
        public string RuleName { get; set; } = string.Empty;

        /// <summary>
        /// 规则类型（MATERIAL-物料编码 / CONTAINER-容器编码 / INBOUND_DOC-入库单号 / OUTBOUND_DOC-出库单号 / STOCKTAKE_DOC-盘点单号 / ADJUST_DOC-调整单号 / OTHER-其他）
        /// </summary>
        
        [SugarColumn(Length = 30, IsNullable = false, ColumnDescription = "规则类型")]
        public string RuleType { get; set; } = string.Empty;

        /// <summary>
        /// 前缀
        /// </summary>
        [SugarColumn(Length = 20, IsNullable = true, ColumnDescription = "前缀")]
        public string? Prefix { get; set; }

        /// <summary>
        /// 日期格式
        /// </summary>
        [SugarColumn(Length = 20, IsNullable = true, ColumnDescription = "日期格式")]
        public string? DateFormat { get; set; }

        /// <summary>
        /// 序列号长度
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "序列号长度", DefaultValue = "4")]
        public int SequenceLength { get; set; } = 4;

        /// <summary>
        /// 序列类型（DAILY-每日 / MONTHLY-每月 / YEARLY-每年 / NONE-不重置）
        /// </summary>
        [SugarColumn(Length = 20, IsNullable = false, ColumnDescription = "序列类型", DefaultValue = "DAILY")]
        public string SequenceType { get; set; } = "DAILY";

        /// <summary>
        /// 分隔符
        /// </summary>
        [SugarColumn(Length = 10, IsNullable = true, ColumnDescription = "分隔符")]
        public string? Separator { get; set; }

        /// <summary>
        /// 示例
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnDescription = "示例")]
        public string? Example { get; set; }

        /// <summary>
        /// 缓存大小
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "缓存大小", DefaultValue = "10")]
        public int CacheSize { get; set; } = 10;

        /// <summary>
        /// 是否默认规则（1是 0否）
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "是否默认规则", DefaultValue = "0")]
        public byte IsDefault { get; set; } = 0;

        /// <summary>
        /// 是否启用（1启用 0禁用）
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "是否启用", DefaultValue = "1")]
        public byte IsActive { get; set; } = 1;

        /// <summary>
        /// 生效日期
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "生效日期")]
        public DateOnly? EffectiveDate { get; set; }

        /// <summary>
        /// 失效日期
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "失效日期")]
        public DateOnly? ExpiryDate { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnDescription = "描述")]
        public string? Description { get; set; }
    }
}
