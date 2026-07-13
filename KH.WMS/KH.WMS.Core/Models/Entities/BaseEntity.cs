using SqlSugar;

namespace KH.WMS.Core.Models.Entities;

/// <summary>
/// 实体基类
/// </summary>
public abstract class BaseEntity<T> : RootEntity where T : struct
{
    /// <summary>
    /// 主键ID
    /// </summary>
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true, ColumnDescription = "主键ID")]
    public T Id { get; set; }

}
