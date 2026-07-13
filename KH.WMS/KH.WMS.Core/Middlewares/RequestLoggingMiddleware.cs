using System.Diagnostics;
using KH.WMS.Core.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace KH.WMS.Core.Middlewares;

/// <summary>
/// 请求日志中间件
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILoggerService _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILoggerService logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.Value?.Contains("/api") ?? false)
        {
            var stopwatch = Stopwatch.StartNew();
            var request = context.Request;

            // 记录请求信息
            //_logger.LogInfo("请求开始: {Method} {Path} from {RemoteIp}",
            //    request.Method,
            //    request.Path,
            //    context.Connection.RemoteIpAddress);

            try
            {
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();

                //_logger.LogInfo("请求完成: {Method} {Path} - {StatusCode} - 耗时: {Elapsed}ms",
                //    request.Method,
                //    request.Path,
                //    context.Response.StatusCode,
                //    stopwatch.ElapsedMilliseconds);
            }
        }
        else
        {
            await _next(context);
        }
    }
}

/// <summary>
/// 请求日志中间件扩展
/// </summary>
public static class RequestLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app)
    {
        return app.UseMiddleware<RequestLoggingMiddleware>();
    }
}
