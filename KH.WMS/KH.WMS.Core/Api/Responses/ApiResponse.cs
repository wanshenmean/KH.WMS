using System.ComponentModel.DataAnnotations;

namespace KH.WMS.Core.Api.Responses;

/// <summary>
/// API 统一响应格式
/// </summary>
public class ApiResponse
{
    /// <summary>
    /// 响应码
    /// </summary>
    public int Code { get; set; } = ResponseCode.SUCCESS;

    /// <summary>
    /// 响应消息
    /// </summary>
    public string Message { get; set; } = "操作成功";

    /// <summary>
    /// 时间戳
    /// </summary>
    public long Timestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    /// <summary>
    /// 响应数据
    /// </summary>
    public object? Data { get; set; }

    /// <summary>
    /// 请求跟踪ID
    /// </summary>
    public string? TraceId { get; set; }

    /// <summary>
    /// 创建成功响应
    /// </summary>
    public static ApiResponse Ok(object? data = null, string message = "操作成功")
    {
        return new ApiResponse
        {
            Code = ResponseCode.SUCCESS,
            Message = message,
            Data = data
        };
    }

    /// <summary>
    /// 创建失败响应
    /// </summary>
    public static ApiResponse Fail(int code, string message)
    {
        return new ApiResponse
        {
            Code = code,
            Message = message
        };
    }

    /// <summary>
    /// 创建未找到响应
    /// </summary>
    public static ApiResponse NotFound(string message = "资源未找到")
    {
        return new ApiResponse
        {
            Code = ResponseCode.NOT_FOUND,
            Message = message
        };
    }

    /// <summary>
    /// 创建验证错误响应
    /// </summary>
    public static ApiResponse ValidationError(string message, object? errors = null)
    {
        return new ApiResponse
        {
            Code = ResponseCode.VALIDATION_ERROR,
            Message = message,
            Data = errors
        };
    }

    /// <summary>
    /// 创建未授权响应
    /// </summary>
    public static ApiResponse Unauthorized(string message = "未授权访问")
    {
        return new ApiResponse
        {
            Code = ResponseCode.UNAUTHORIZED,
            Message = message
        };
    }

    /// <summary>
    /// 创建服务器错误响应
    /// </summary>
    public static ApiResponse Error(string message = "服务器内部错误")
    {
        return new ApiResponse
        {
            Code = ResponseCode.INTERNAL_SERVER_ERROR,
            Message = message
        };
    }
}

/// <summary>
/// API 泛型响应格式
/// </summary>
public class ApiResponse<T> : ApiResponse
{
    /// <summary>
    /// 响应数据
    /// </summary>
    public new T? Data { get; set; }

    /// <summary>
    /// 创建成功响应
    /// </summary>
    public static ApiResponse<T> Ok(T? data, string message = "操作成功")
    {
        return new ApiResponse<T>
        {
            Code = ResponseCode.SUCCESS,
            Message = message,
            Data = data
        };
    }

    /// <summary>
    /// 创建失败响应
    /// </summary>
    public static new ApiResponse<T> Fail(int code, string message)
    {
        return new ApiResponse<T>
        {
            Code = code,
            Message = message
        };
    }
}

/// <summary>
/// API 响应扩展
/// </summary>
public static class ApiResponseExtensions
{
    /// <summary>
    /// 转换为 MVC ActionResult
    /// </summary>
    public static Microsoft.AspNetCore.Mvc.ActionResult ToActionResult(this ApiResponse response)
    {
        var statusCode = ResponseCode.GetHttpStatusCode(response.Code);
        return new Microsoft.AspNetCore.Mvc.ObjectResult(response)
        {
            StatusCode = statusCode
        };
    }

    /// <summary>
    /// 转换为 MVC ActionResult（泛型）
    /// </summary>
    public static Microsoft.AspNetCore.Mvc.ActionResult<T> ToActionResult<T>(this ApiResponse<T> response)
    {
        var statusCode = ResponseCode.GetHttpStatusCode(response.Code);
        return new Microsoft.AspNetCore.Mvc.ObjectResult(response)
        {
            StatusCode = statusCode
        };
    }
}
