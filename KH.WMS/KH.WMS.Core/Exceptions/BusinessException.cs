namespace KH.WMS.Core.Exceptions;

/// <summary>
/// 业务异常
/// </summary>
public class BusinessException : Exception
{
    /// <summary>
    /// 错误码
    /// </summary>
    public int ErrorCode { get; set; } = ErrorCodes.BUSINESS_ERROR;

    /// <summary>
    /// 错误详情
    /// </summary>
    public Dictionary<string, object> Details { get; set; } = new();

    public BusinessException() : base()
    {
    }

    public BusinessException(string message) : base(message)
    {
    }

    public BusinessException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public BusinessException(int errorCode, string message) : base(message)
    {
        ErrorCode = errorCode;
    }

    public BusinessException(int errorCode, string message, Dictionary<string, object> details) : base(message)
    {
        ErrorCode = errorCode;
        Details = details;
    }
}
