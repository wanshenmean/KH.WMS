using System.ComponentModel.DataAnnotations;
using SqlSugar;
using KH.WMS.Core.Models.Entities;

namespace KH.WMS.Entities.BaseData;

/// <summary>
/// 物料分类
/// </summary>
[SugarTable("md_material_category")]
[SugarIndex("uk_category_code", nameof(CategoryCode), OrderByType.Asc, true)]
[SugarIndex("idx_parent", nameof(ParentId), OrderByType.Asc)]
[SugarIndex("idx_level", nameof(Level), OrderByType.Asc)]
public class MdMaterialCategory : BaseEntity<long>, IEnableDisableEntity
{

    /// <summary>
    /// 分类编码
    /// </summary>
    [SugarColumn(Length = 50, IsNullable = false, ColumnDescription = "分类编码")]
    public string CategoryCode { get; set; } = string.Empty;

    /// <summary>
    /// 分类名称
    /// </summary>
    [SugarColumn(Length = 100, IsNullable = false, ColumnDescription = "分类名称")]
    public string CategoryName { get; set; } = string.Empty;

    /// <summary>
    /// 父级ID
    /// </summary>
    [SugarColumn(IsNullable = false, ColumnDescription = "父级ID", DefaultValue = "0")]
    public long ParentId { get; set; } = 0;

    /// <summary>
    /// 层级
    /// </summary>
    [SugarColumn(IsNullable = false, ColumnDescription = "层级", DefaultValue = "1")]
    public int Level { get; set; } = 1;

    /// <summary>
    /// 层级路径
    /// </summary>
    [SugarColumn(Length = 500, IsNullable = true, ColumnDescription = "层级路径")]
    public string? Path { get; set; }

    /// <summary>
    /// 排序号
    /// </summary>
    [SugarColumn(IsNullable = false, ColumnDescription = "排序号", DefaultValue = "0")]
    public int SortNo { get; set; } = 0;

    /// <summary>
    /// 状态（1启用 0禁用）
    /// </summary>
    [SugarColumn(IsNullable = false, ColumnDescription = "状态（1启用 0禁用）", DefaultValue = "1")]
    public byte Status { get; set; } = 1;
}
