using SqlSugar;
using System.ComponentModel.DataAnnotations;
using KH.WMS.Algorithms.Strategy.Constants;
using KH.WMS.Core.Models.Entities;

namespace KH.WMS.Algorithms.Strategy.Configuration
{
    /// <summary>
    /// 策略链步骤
    /// 定义策略链中每个步骤引用的具体策略及执行参数
    /// </summary>
    [SugarTable("cfg_strategy_chain_step")]
    [SugarIndex("uk_chain_step", nameof(ChainId), OrderByType.Asc, nameof(StepNo), OrderByType.Asc, true)]
    [SugarIndex("idx_chain", nameof(ChainId), OrderByType.Asc)]
    [SugarIndex("idx_strategy", nameof(StrategyConfigId), OrderByType.Asc)]
    public class CfgStrategyChainStep : BaseEntity<long>
    {
        /// <summary>
        /// 策略链ID
        /// </summary>
        
        [SugarColumn(IsNullable = false, ColumnDescription = "策略链ID")]
        public long ChainId { get; set; }

        /// <summary>
        /// 步骤号（从1开始，按升序执行）
        /// </summary>
        
        [SugarColumn(IsNullable = false, ColumnDescription = "步骤号")]
        public int StepNo { get; set; }

        /// <summary>
        /// 步骤名称
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true, ColumnDescription = "步骤名称")]
        public string? StepName { get; set; }

        /// <summary>
        /// 策略配置ID（关联 cfg_strategy_config）
        /// </summary>
        
        [SugarColumn(IsNullable = false, ColumnDescription = "策略配置ID")]
        public long StrategyConfigId { get; set; }

        /// <summary>
        /// 策略规则编码（冗余存储，方便查询）
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnDescription = "策略规则编码")]
        public string? RuleCode { get; set; }

        /// <summary>
        /// 步骤执行模式（CHAIN-链式 / PARALLEL-并行 / STOP_ON_SUCCESS-成功即停）
        /// </summary>
        [SugarColumn(Length = 20, IsNullable = false, ColumnDescription = "步骤执行模式", DefaultValue = "CHAIN")]
        public string StepMode { get; set; } = AlgoConstants.ExecutionMode.CHAIN;

        /// <summary>
        /// 是否启用（1启用 0跳过）
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "是否启用", DefaultValue = "1")]
        public byte IsEnabled { get; set; } = 1;

        /// <summary>
        /// 步骤参数（JSON格式，可覆盖策略配置中的默认参数）
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDataType = "TEXT", ColumnDescription = "步骤参数")]
        public string? StepParams { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnDescription = "备注")]
        public string? Remark { get; set; }
    }
}
