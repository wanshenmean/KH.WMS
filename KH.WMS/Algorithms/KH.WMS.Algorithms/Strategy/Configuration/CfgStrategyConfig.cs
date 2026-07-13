using SqlSugar;
using System.ComponentModel.DataAnnotations;
using KH.WMS.Algorithms.Strategy.Constants;
using KH.WMS.Core.Models.Entities;

namespace KH.WMS.Algorithms.Strategy.Configuration
{
    /// <summary>
    /// 策略配置
    /// 统一管理所有策略类型的配置，通过 StrategyType 区分不同策略
    /// 策略类型：PUTAWAY-上架策略 / LOCATION_ALLOCATION-货位分配策略 / PICKING-下架策略 / INVENTORY_ALLOCATION-库存分配策略
    /// 匹配优先级（从高到低）：物料级 > 单据类型+仓库级 > 区域级 > 仓库级 > 全局
    /// </summary>
    [SugarTable("cfg_strategy_config")]
    [SugarIndex("uk_strategy_code", nameof(StrategyCode), OrderByType.Asc, true)]
    [SugarIndex("idx_type_status", nameof(StrategyType), OrderByType.Asc, nameof(Status), OrderByType.Asc)]
    [SugarIndex("idx_match_warehouse", nameof(WarehouseId), OrderByType.Asc)]
    [SugarIndex("idx_match_zone", nameof(ZoneId), OrderByType.Asc)]
    [SugarIndex("idx_match_material", nameof(MaterialId), OrderByType.Asc)]
    [SugarIndex("idx_match_doc_type", nameof(DocType), OrderByType.Asc)]
    [SugarIndex("idx_match_priority", nameof(WarehouseId), OrderByType.Asc, nameof(ZoneId), OrderByType.Asc, nameof(MaterialId), OrderByType.Asc, nameof(DocType), OrderByType.Asc, nameof(IsDefault), OrderByType.Asc)]
    [SugarIndex("idx_status", nameof(Status), OrderByType.Asc)]
    public class CfgStrategyConfig : BaseEntity<long>
    {
        /// <summary>
        /// 策略编码
        /// </summary>
        
        [SugarColumn(Length = 50, IsNullable = false, ColumnDescription = "策略编码")]
        public string StrategyCode { get; set; } = string.Empty;

        /// <summary>
        /// 策略名称
        /// </summary>
        
        [SugarColumn(Length = 100, IsNullable = false, ColumnDescription = "策略名称")]
        public string StrategyName { get; set; } = string.Empty;

        /// <summary>
        /// 策略类型（PUTAWAY / LOCATION_ALLOCATION / PICKING / INVENTORY_ALLOCATION）
        /// </summary>
        
        [SugarColumn(Length = 30, IsNullable = false, ColumnDescription = "策略类型")]
        public string StrategyType { get; set; } = string.Empty;

        /// <summary>
        /// 策略规则编码（对应算法引擎中注册的策略实现编码，如 DEFAULT_PUTAWAY/ABC_CLASS/FIFO/FEFO 等）
        /// </summary>
        
        [SugarColumn(Length = 50, IsNullable = false, ColumnDescription = "策略规则编码")]
        public string RuleCode { get; set; } = string.Empty;

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
        /// 物料ID（为空表示不限物料，有值时为物料级策略）
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "物料ID")]
        public long? MaterialId { get; set; }

        /// <summary>
        /// 物料分类ID（为空表示不限物料分类）
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "物料分类ID")]
        public long? MaterialCategoryId { get; set; }

        /// <summary>
        /// 单据类型（为空表示不限单据类型）
        /// </summary>
        [SugarColumn(Length = 30, IsNullable = true, ColumnDescription = "单据类型")]
        public string? DocType { get; set; }

        /// <summary>
        /// 优先级（数字越大优先级越高）
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "优先级", DefaultValue = "100")]
        public int Priority { get; set; } = 100;

        /// <summary>
        /// 是否默认策略（1是 0否）
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "是否默认策略", DefaultValue = "0")]
        public byte IsDefault { get; set; } = 0;

        /// <summary>
        /// 状态（1启用 0禁用）
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "状态", DefaultValue = "1")]
        public byte Status { get; set; } = 1;

        /// <summary>
        /// 执行顺序（在同一策略链中的执行顺序，数字越小越先执行）
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "执行顺序", DefaultValue = "100")]
        public int SortOrder { get; set; } = 100;

        /// <summary>
        /// 执行模式（CHAIN-链式执行 / PARALLEL-并行执行）
        /// </summary>
        [SugarColumn(Length = 20, IsNullable = false, ColumnDescription = "执行模式", DefaultValue = "CHAIN")]
        public string ExecutionMode { get; set; } = AlgoConstants.ExecutionMode.CHAIN;

        /// <summary>
        /// 策略参数（JSON格式，存储该策略特有的配置参数）
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDataType = "TEXT", ColumnDescription = "策略参数")]
        public string? StrategyParams { get; set; }

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
