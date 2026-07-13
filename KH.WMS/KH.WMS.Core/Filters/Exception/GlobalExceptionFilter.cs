using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using KH.WMS.Core.Api.Responses;
using KH.WMS.Core.Exceptions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KH.WMS.Core.Filters.Exception;

/// <summary>
/// 全局异常过滤器 — 使用 ApiResponse 统一响应格式
/// </summary>
public class GlobalExceptionFilter : IAsyncExceptionFilter
{
    private readonly ILogger<GlobalExceptionFilter> _logger;
    private readonly IWebHostEnvironment _env;

    public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger, IWebHostEnvironment env)
    {
        _logger = logger;
        _env = env;
    }

    public async Task OnExceptionAsync(ExceptionContext context)
    {
        var exception = context.Exception;
        var request = context.HttpContext.Request;
        var path = request.Path.HasValue ? request.Path.Value : "";
        var method = request.Method;
        var query = request.QueryString.HasValue ? request.QueryString.Value : "";
        var traceId = Activity.Current?.Id ?? context.HttpContext.TraceIdentifier;
        var userId = context.HttpContext.User?.FindFirst(JwtRegisteredClaimNames.Jti)?.Value
                     ?? context.HttpContext.User?.Identity?.Name ?? "";

        // 读取请求体（EnableBuffering 已启用，可重读；仅 JSON 且 ≤100KB 时记录，避免大上传撑爆日志）
        var body = "";
        if (request.ContentLength is > 0 and <= 1024 * 100
            && request.ContentType != null
            && request.ContentType.Contains("application/json", System.StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                request.Body.Position = 0;
                using var reader = new System.IO.StreamReader(request.Body, System.Text.Encoding.UTF8, leaveOpen: true);
                body = await reader.ReadToEndAsync();
                request.Body.Position = 0;
                body = MaskSensitiveFields(body);
            }
            catch { /* 读 body 失败不影响异常处理 */ }
        }

        // 详细异常日志：方法/路径/TraceId/用户/查询参数/请求体(脱敏) + 异常类型与消息 + 完整堆栈(exception 参数由 Serilog {Exception} 自动附加)
        _logger.LogError(exception,
            "[全局异常] {Method} {Path} | TraceId: {TraceId} | User: {UserId} | Query: {Query} | Body: {Body} | 异常: {ExceptionType}: {Message}",
            method, path, traceId, userId, query, body, exception.GetType().Name, exception.Message);

        // 从请求级缓冲区取出完整调用链（方法参数/SQL），强制写入 error 日志（不管 MinimumLevel）
        var callChain = KH.WMS.Core.Logging.ErrorLogScope.Flush();
        if (callChain is { Count: > 0 })
        {
            _logger.LogError("[异常调用链] TraceId: {TraceId} | 调用链条数: {Count}\n{Detail}",
                traceId, callChain.Count, string.Join("\n", callChain));
        }

        context.Result = exception switch
        {
            BusinessException businessEx => BuildResult(
                ResponseCode.BAD_REQUEST,
                businessEx.Message,
                _env.IsDevelopment() ? new { errorCode = businessEx.ErrorCode, stackTrace = businessEx.StackTrace, details = businessEx.Details } : null,
                traceId),

            NotFoundException notFoundEx => BuildResult(
                ResponseCode.NOT_FOUND,
                notFoundEx.Message,
                new { resourceType = notFoundEx.ResourceType, resourceId = notFoundEx.ResourceId },
                traceId),

            ValidationException validationEx => BuildResult(
                ResponseCode.VALIDATION_ERROR,
                "数据验证失败",
                validationEx.Errors,
                traceId),

            UnauthorizedAccessException => BuildResult(
                ResponseCode.UNAUTHORIZED,
                "无权访问",
                null,
                traceId),

            ArgumentException argEx => BuildResult(
                ResponseCode.BAD_REQUEST,
                argEx.Message,
                _env.IsDevelopment() ? new { paramName = argEx.ParamName } : null,
                traceId),

            _ => BuildResult(
                ResponseCode.INTERNAL_SERVER_ERROR,
                _env.IsDevelopment() ? exception.Message : "服务器内部错误",
                _env.IsDevelopment()
                    ? new { exception = exception.GetType().Name, message = exception.Message, stackTrace = exception.StackTrace }
                    : null,
                traceId)
        };

        context.ExceptionHandled = true;
    }

    /// <summary>
    /// 构建 ApiResponse → ObjectResult，HTTP 状态码由 ResponseCode 映射
    /// </summary>
    private static Microsoft.AspNetCore.Mvc.ObjectResult BuildResult(int code, string message, object? data, string traceId)
    {
        var response = new ApiResponse
        {
            Code = code,
            Message = message,
            Data = data,
            TraceId = traceId
        };

        var httpStatus = ResponseCode.GetHttpStatusCode(code);
        return new Microsoft.AspNetCore.Mvc.ObjectResult(response) { StatusCode = httpStatus };
    }

    /// <summary>
    /// 脱敏 JSON 请求体中的敏感字段（Password/Token/Secret 等），避免密码泄露到日志
    /// </summary>
    private static string MaskSensitiveFields(string json)
    {
        if (string.IsNullOrEmpty(json)) return json;
        return System.Text.RegularExpressions.Regex.Replace(json,
            @"(""(?:password|token|secret|refreshtoken|oldpassword|newpassword|authorization)""\s*:\s*)""[^""]*""",
            "$1\"***\"", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
    }
}
