namespace KH.WMS.Core.Exceptions;

/// <summary>
/// 未找到异常
/// </summary>
public class NotFoundException : Exception
{
    /// <summary>
    /// 资源类型
    /// </summary>
    public string ResourceType { get; set; } = string.Empty;

    /// <summary>
    /// 资源ID
    /// </summary>
    public object? ResourceId { get; set; }

    public NotFoundException() : base()
    {
    }

    public NotFoundException(string message) : base(message)
    {
    }

    public NotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public NotFoundException(string resourceType, object resourceId)
        : base($"{resourceType} (ID: {resourceId}) 未找到")
    {
        ResourceType = resourceType;
        ResourceId = resourceId;
    }

    public NotFoundException(string resourceType, string fieldName, object fieldValue)
        : base($"{resourceType} ({fieldName}: {fieldValue}) 未找到")
    {
        ResourceType = resourceType;
        ResourceId = fieldValue;
    }
}
