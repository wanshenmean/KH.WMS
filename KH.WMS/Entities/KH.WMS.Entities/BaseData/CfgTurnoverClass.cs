using System.ComponentModel.DataAnnotations;
using SqlSugar;
using KH.WMS.Core.Models.Entities;
using KH.WMS.Core.Database.SqlSugar;

namespace KH.WMS.Entities.BaseData;

/// <summary>
/// 周转分类配置
/// 定义ABC分类的规则参数和阈值
/// </summary>
[ConfigDb]
[SugarTable("cfg_turnover_class")]
[SugarIndex("uk_class_code", nameof(ClassCode), OrderByType.Asc, true)]
public class CfgTurnoverClass : BaseEntity<long>, IEnableDisableEntity
{
    /// <summary>
    /// 分类编码（A/B/C）
    /// </summary>
    [SugarColumn(Length = 10, IsNullable = false, ColumnDescription = "分类编码")]
    public string ClassCode { get; set; } = string.Empty;

    /// <summary>
    /// 分类名称（如高频物料/中频物料/低频物料）
    /// </summary>
    [SugarColumn(Length = 50, IsNullable = false, ColumnDescription = "分类名称")]
    public string ClassName { get; set; } = string.Empty;

    /// <summary>
    /// 累计占比下限（%），如 A类: 0, B类: 70, C类: 90
    /// </summary>
    [SugarColumn(DecimalDigits = 2, Length = 5, IsNullable = false, ColumnDescription = "累计占比下限(%)", DefaultValue = "0")]
    public decimal CumulativeRatioMin { get; set; }

    /// <summary>
    /// 累计占比上限（%），如 A类: 70, B类: 90, C类: 100
    /// </summary>
    [SugarColumn(DecimalDigits = 2, Length = 5, IsNullable = false, ColumnDescription = "累计占比上限(%)", DefaultValue = "100")]
    public decimal CumulativeRatioMax { get; set; } = 100;

    /// <summary>
    /// 分析维度（OUTBOUND_QTY-出库数量 / OUTBOUND_FREQ-出库频次）
    /// </summary>
    [SugarColumn(Length = 30, IsNullable = false, ColumnDescription = "分析维度", DefaultValue = "OUTBOUND_QTY")]
    public string AnalysisDimension { get; set; } = "OUTBOUND_QTY";

    /// <summary>
    /// 颜色标识（前端展示用）
    /// </summary>
    [SugarColumn(Length = 20, IsNullable = true, ColumnDescription = "颜色标识")]
    public string? Color { get; set; }

    /// <summary>
    /// 排序号
    /// </summary>
    [SugarColumn(IsNullable = false, ColumnDescription = "排序号", DefaultValue = "0")]
    public int SortNo { get; set; }

    /// <summary>
    /// 状态（1启用 0禁用）
    /// </summary>
    [SugarColumn(IsNullable = false, ColumnDescription = "状态", DefaultValue = "1")]
    public byte Status { get; set; } = 1;
}
