using System.ComponentModel.DataAnnotations;
using SqlSugar;
using KH.WMS.Core.Models.Entities;

namespace KH.WMS.Entities.BaseData;

/// <summary>
/// 物料主数据
/// </summary>
[SugarTable("md_material")]
[SugarIndex("uk_material_code", nameof(MaterialCode), OrderByType.Asc, true)]
[SugarIndex("idx_category", nameof(CategoryId), OrderByType.Asc)]
[SugarIndex("idx_status", nameof(Status), OrderByType.Asc)]
public class MdMaterial : BaseEntity<long>, IEnableDisableEntity
{

    /// <summary>
    /// 物料编码
    /// </summary>
    [SugarColumn(Length = 50, IsNullable = false, ColumnDescription = "物料编码")]
    public string MaterialCode { get; set; } = string.Empty;

    /// <summary>
    /// 物料名称
    /// </summary>
    [SugarColumn(Length = 200, IsNullable = false, ColumnDescription = "物料名称")]
    public string MaterialName { get; set; } = string.Empty;

    /// <summary>
    /// 物料分类ID
    /// </summary>
    [SugarColumn(IsNullable = true, ColumnDescription = "物料分类ID")]
    public long? CategoryId { get; set; }

    /// <summary>
    /// 基本单位ID
    /// </summary>
    [SugarColumn(IsNullable = false, ColumnDescription = "基本单位ID")]
    public long BaseUnitId { get; set; }

    /// <summary>
    /// 状态（1启用 0禁用）
    /// </summary>
    [SugarColumn(IsNullable = false, ColumnDescription = "状态（1启用 0禁用）", DefaultValue = "1")]
    public byte Status { get; set; } = 1;

    /// <summary>
    /// 是否批管理（1是 0否）
    /// </summary>
    [SugarColumn(IsNullable = false, ColumnDescription = "是否批管理（1是 0否）", DefaultValue = "0")]
    public int IsBatchManaged { get; set; } = 0;

    /// <summary>
    /// 是否序列号管理（1是 0否）
    /// </summary>
    [SugarColumn(IsNullable = false, ColumnDescription = "是否序列号管理（1是 0否）", DefaultValue = "0")]
    public int IsSerialManaged { get; set; } = 0;

    /// <summary>
    /// 是否有有效期（1是 0否）
    /// </summary>
    [SugarColumn(IsNullable = false, ColumnDescription = "是否有有效期（1是 0否）", DefaultValue = "0")]
    public int HasExpiry { get; set; } = 0;

    /// <summary>
    /// 保质期天数
    /// </summary>
    [SugarColumn(IsNullable = true, ColumnDescription = "保质期天数")]
    public int? ShelfLifeDays { get; set; }

    /// <summary>
    /// 周转分类
    /// </summary>
    [SugarColumn(Length = 1, IsNullable = true, ColumnDescription = "周转分类")]
    public string? TurnoverClass { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    [SugarColumn(Length = 500, IsNullable = true, ColumnDescription = "备注")]
    public string? Remark { get; set; }

    /// <summary>
    /// 扩展字段（JSON格式，CfgExtField配置驱动）
    /// </summary>
    [SugarColumn(IsNullable = true, ColumnDataType = "TEXT", ColumnDescription = "扩展字段")]
    public string? ExtData { get; set; }
}
