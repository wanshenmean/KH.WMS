using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;
using KH.WMS.Core.Constants;
using KH.WMS.Core.Exceptions;
using Microsoft.AspNetCore.Http;
using KH.WMS.Core.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Builder;
using KH.WMS.Core.Api.Responses;
using Microsoft.Data.SqlClient;

namespace KH.WMS.Core.Middlewares;

/// <summary>
/// 全局异常处理中间件
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILoggerService _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILoggerService logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = new ApiResponse { TraceId = context.TraceIdentifier };
        var statusCode = HttpStatusCode.InternalServerError;

        switch (exception)
        {
            case BusinessException businessException:
                statusCode = HttpStatusCode.BadRequest;
                response.Code = businessException.ErrorCode;
                response.Message = businessException.Message;
                response.Data = businessException.Details;
                _logger.LogError(businessException, "业务异常: {Message}", businessException.Message);
                break;

            case ValidationException validationException:
                statusCode = HttpStatusCode.BadRequest;
                response.Code = ErrorCodes.VALIDATION_ERROR;
                response.Message = "数据验证失败";
                response.Data = validationException.Errors;
                _logger.LogError(validationException, "验证异常: {Message}", validationException.Message);
                break;

            case NotFoundException notFoundException:
                statusCode = HttpStatusCode.NotFound;
                response.Code = ErrorCodes.NOT_FOUND;
                response.Message = notFoundException.Message;
                _logger.LogError(notFoundException, "未找到异常: {Message}", notFoundException.Message);
                break;

            case UnauthorizedAccessException:
                statusCode = HttpStatusCode.Unauthorized;
                response.Code = ErrorCodes.UNAUTHORIZED;
                response.Message = "未授权访问";
                _logger.LogError(exception, "未授权访问");
                break;
            default:
                statusCode = HttpStatusCode.InternalServerError;
                response.Code = ErrorCodes.SYSTEM_ERROR;
                response.Message = _environment.IsDevelopment()
                    ? exception.Message
                    : "系统发生错误，请稍后重试";

                if (_environment.IsDevelopment())
                {
                    response.Data = new
                    {
                        exception.GetType().Name,
                        exception.Message,
                        exception.StackTrace
                    };
                }

                _logger.LogError(exception, "系统异常: {Message}", exception.Message);
                break;
        }

        context.Response.ContentType = HeaderConstants.ContentTypes.APPLICATION_JSON;
        context.Response.StatusCode = (int)statusCode;

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = _environment.IsDevelopment()
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }
}

/// <summary>
/// 异常处理中间件扩展
/// </summary>
public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}
