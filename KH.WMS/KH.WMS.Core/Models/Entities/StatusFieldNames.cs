namespace KH.WMS.Core.Models.Entities;

/// <summary>
/// 启用/禁用状态字段名常量
/// CrudService 按以下优先级查找状态字段：
/// 1. 实体上的 [StatusFieldName] 特性指定的字段名
/// 2. StatusFieldNames.Status（默认）
/// 3. StatusFieldNames.IsActive（备选）
/// </summary>
public static class StatusFieldNames
{
    /// <summary>
    /// 默认状态字段名
    /// </summary>
    public const string Status = nameof(Status);

    /// <summary>
    /// 配置类实体常用状态字段名
    /// </summary>
    public const string IsActive = nameof(IsActive);
}
