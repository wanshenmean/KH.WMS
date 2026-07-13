namespace KH.WMS.Core.Models;

/// <summary>
/// 服务层操作结果（替代 (bool Success, string? Message) 元组）
/// </summary>
public class ServiceResult
{
    public bool Success { get; init; }
    public string? Message { get; init; }

    public static ServiceResult Ok(string? message = null) => new() { Success = true, Message = message };
    public static ServiceResult Fail(string message) => new() { Success = false, Message = message };

    public static implicit operator ServiceResult((bool Success, string? Message) tuple)
        => new() { Success = tuple.Success, Message = tuple.Message };
}

/// <summary>
/// 带数据的操作结果（替代 (bool Success, string? Message, T? Data) 元组）
/// </summary>
public class ServiceResult<T> : ServiceResult
{
    public T? Data { get; init; }

    public static ServiceResult<T> Ok(T data, string? message = null)
        => new() { Success = true, Data = data, Message = message };

    public new static ServiceResult<T> Fail(string message)
        => new() { Success = false, Message = message };
}
