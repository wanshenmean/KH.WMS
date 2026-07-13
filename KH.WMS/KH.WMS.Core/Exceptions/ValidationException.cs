namespace KH.WMS.Core.Exceptions;

/// <summary>
/// 验证异常
/// </summary>
public class ValidationException : Exception
{
    /// <summary>
    /// 验证错误列表
    /// </summary>
    public List<ValidationError> Errors { get; set; } = new();

    public ValidationException() : base()
    {
    }

    public ValidationException(string message) : base(message)
    {
    }

    public ValidationException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public ValidationException(List<ValidationError> errors) : base("数据验证失败")
    {
        Errors = errors;
    }

    public ValidationException(string field, string message) : base("数据验证失败")
    {
        Errors.Add(new ValidationError { Field = field, Message = message });
    }
}

/// <summary>
/// 验证错误详情
/// </summary>
public class ValidationError
{
    /// <summary>
    /// 字段名
    /// </summary>
    public string Field { get; set; } = string.Empty;

    /// <summary>
    /// 错误消息
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// 错误值
    /// </summary>
    public object? Value { get; set; }
}
