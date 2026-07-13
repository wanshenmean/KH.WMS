namespace KH.WMS.Core.Attributes;

/// <summary>
/// 指定启用/禁用管理的状态字段名（优先级高于默认的 Status / IsActive 约定）
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class StatusFieldNameAttribute : Attribute
{
    /// <summary>
    /// 状态属性名
    /// </summary>
    public string FieldName { get; }

    /// <summary>
    /// 指定状态字段名
    /// </summary>
    /// <param name="fieldName">属性名，如 "Status"、"IsActive"、"IsDisabled"</param>
    public StatusFieldNameAttribute(string fieldName)
    {
        FieldName = fieldName;
    }
}
