using System.ComponentModel.DataAnnotations;
using SqlSugar;
using KH.WMS.Core.Models.Entities;

namespace KH.WMS.Entities.BaseData;

/// <summary>
/// 物料单位
/// </summary>
[SugarTable("md_material_unit")]
[SugarIndex("uk_unit_code", nameof(UnitCode), OrderByType.Asc, true)]
public class MdMaterialUnit : BaseEntity<long>, IEnableDisableEntity
{

    /// <summary>
    /// 单位编码
    /// </summary>
    
    [SugarColumn(Length = 10, IsNullable = false, ColumnDescription = "单位编码")]
    public string UnitCode { get; set; } = string.Empty;

    /// <summary>
    /// 单位名称
    /// </summary>
    
    [SugarColumn(Length = 50, IsNullable = false, ColumnDescription = "单位名称")]
    public string UnitName { get; set; } = string.Empty;

    /// <summary>
    /// 基本单位ID
    /// </summary>
    [SugarColumn(IsNullable = true, ColumnDescription = "基本单位ID")]
    public long? BaseUnitId { get; set; }

    /// <summary>
    /// 换算率
    /// </summary>
    [SugarColumn(DecimalDigits = 4, Length = 10, IsNullable = true, ColumnDescription = "换算率")]
    public decimal? ConversionRate { get; set; }

    /// <summary>
    /// 状态（1启用 0禁用）
    /// </summary>
    [SugarColumn(IsNullable = false, ColumnDescription = "状态（1启用 0禁用）", DefaultValue = "1")]
    public byte Status { get; set; } = 1;
}
