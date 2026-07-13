using System.ComponentModel.DataAnnotations;
using SqlSugar;
using KH.WMS.Core.Models.Entities;

namespace KH.WMS.Entities.BaseData;

/// <summary>
/// 物料周转分类结果
/// 存储ABC分析的计算结果，每次计算覆盖
/// </summary>
[SugarTable("md_material_turnover")]
[SugarIndex("uk_material_period", nameof(MaterialId), OrderByType.Asc, nameof(Period), OrderByType.Asc, true)]
[SugarIndex("idx_class", nameof(ClassCode), OrderByType.Asc)]
public class MdMaterialTurnover : BaseEntity<long>
{
    /// <summary>
    /// 物料ID
    /// </summary>
    [SugarColumn(IsNullable = false, ColumnDescription = "物料ID")]
    public long MaterialId { get; set; }

    /// <summary>
    /// 物料编码（冗余，便于查询）
    /// </summary>
    [SugarColumn(Length = 50, IsNullable = true, ColumnDescription = "物料编码")]
    public string? MaterialCode { get; set; }

    /// <summary>
    /// 物料名称（冗余，便于查询）
    /// </summary>
    [SugarColumn(Length = 200, IsNullable = true, ColumnDescription = "物料名称")]
    public string? MaterialName { get; set; }

    /// <summary>
    /// 分类编码（A/B/C）
    /// </summary>
    [SugarColumn(Length = 10, IsNullable = false, ColumnDescription = "分类编码")]
    public string ClassCode { get; set; } = string.Empty;

    /// <summary>
    /// 分析周期（如 2026-Q1、2026-04）
    /// </summary>
    
    [SugarColumn(Length = 20, IsNullable = false, ColumnDescription = "分析周期")]
    public string Period { get; set; } = string.Empty;

    /// <summary>
    /// 出库次数
    /// </summary>
    [SugarColumn(IsNullable = false, ColumnDescription = "出库次数", DefaultValue = "0")]
    public int OutboundCount { get; set; }

    /// <summary>
    /// 出库数量
    /// </summary>
    [SugarColumn(DecimalDigits = 2, Length = 18, IsNullable = false, ColumnDescription = "出库数量", DefaultValue = "0")]
    public decimal OutboundQty { get; set; }

    /// <summary>
    /// 累计占比（%）
    /// </summary>
    [SugarColumn(DecimalDigits = 2, Length = 5, IsNullable = false, ColumnDescription = "累计占比(%)", DefaultValue = "0")]
    public decimal CumulativeRatio { get; set; }

    /// <summary>
    /// 计算时间
    /// </summary>
    [SugarColumn(IsNullable = false, ColumnDescription = "计算时间")]
    public DateTime CalculatedAt { get; set; } = DateTime.Now;
}
