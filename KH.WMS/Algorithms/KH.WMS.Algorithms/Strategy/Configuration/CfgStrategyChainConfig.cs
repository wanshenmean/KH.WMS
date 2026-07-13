using SqlSugar;
using System.ComponentModel.DataAnnotations;
using KH.WMS.Core.Models.Entities;

namespace KH.WMS.Algorithms.Strategy.Configuration
{
    /// <summary>
    /// 策略链配置
    /// 定义一条完整的策略链，包含多个策略步骤，按顺序执行
    /// </summary>
    [SugarTable("cfg_strategy_chain_config")]
    [SugarIndex("uk_chain_code", nameof(ChainCode), OrderByType.Asc, true)]
    [SugarIndex("idx_type_status", nameof(ChainType), OrderByType.Asc, nameof(Status), OrderByType.Asc)]
    [SugarIndex("idx_match", nameof(WarehouseId), OrderByType.Asc, nameof(ZoneId), OrderByType.Asc, nameof(DocType), OrderByType.Asc, nameof(IsDefault), OrderByType.Asc)]
    [SugarIndex("idx_status", nameof(Status), OrderByType.Asc)]
    public class CfgStrategyChainConfig : BaseEntity<long>
    {
        /// <summary>
        /// 策略链编码
        /// </summary>
        
        [SugarColumn(Length = 50, IsNullable = false, ColumnDescription = "策略链编码")]
        public string ChainCode { get; set; } = string.Empty;

        /// <summary>
        /// 策略链名称
        /// </summary>
        
        [SugarColumn(Length = 100, IsNullable = false, ColumnDescription = "策略链名称")]
        public string ChainName { get; set; } = string.Empty;

        /// <summary>
        /// 策略链类型（PUTAWAY / LOCATION_ALLOCATION / PICKING / INVENTORY_ALLOCATION）
        /// </summary>
        
        [SugarColumn(Length = 30, IsNullable = false, ColumnDescription = "策略链类型")]
        public string ChainType { get; set; } = string.Empty;

        /// <summary>
        /// 仓库ID（为空表示不限仓库）
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "仓库ID")]
        public long? WarehouseId { get; set; }

        /// <summary>
        /// 库区ID（为空表示不限库区）
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "库区ID")]
        public long? ZoneId { get; set; }

        /// <summary>
        /// 单据类型（为空表示不限单据类型）
        /// </summary>
        [SugarColumn(Length = 30, IsNullable = true, ColumnDescription = "单据类型")]
        public string? DocType { get; set; }

        /// <summary>
        /// 策略链优先级（数字越大优先级越高）
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "策略链优先级", DefaultValue = "100")]
        public int Priority { get; set; } = 100;

        /// <summary>
        /// 是否默认链（1是 0否）
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "是否默认链", DefaultValue = "0")]
        public byte IsDefault { get; set; } = 0;

        /// <summary>
        /// 状态（1启用 0禁用）
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "状态", DefaultValue = "1")]
        public byte Status { get; set; } = 1;

        /// <summary>
        /// 步骤数量
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "步骤数量", DefaultValue = "0")]
        public int StepCount { get; set; } = 0;

        /// <summary>
        /// 描述
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnDescription = "描述")]
        public string? Description { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnDescription = "备注")]
        public string? Remark { get; set; }
    }
}
