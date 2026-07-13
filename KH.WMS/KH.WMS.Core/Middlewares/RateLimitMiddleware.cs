using KH.WMS.Core.Caching;
using KH.WMS.Core.Constants;
using KH.WMS.Core.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Text.Json;

namespace KH.WMS.Core.Middlewares;

/// <summary>
/// 限流中间件
/// </summary>
public class RateLimitMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ICacheService _cache;
    private readonly ILoggerService _logger;
    private readonly RateLimitOptions _options;

    public RateLimitMiddleware(
        RequestDelegate next,
        ICacheService cache,
        ILoggerService logger,
        RateLimitOptions options)
    {
        _next = next;
        _cache = cache;
        _logger = logger;
        _options = options;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var clientId = GetClientId(context);
        var key = $"{_options.KeyPrefix}:{clientId}";

        var counter = _cache.GetOrCreate(key, () =>
        {
            //entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(_options.WindowSeconds);
            return new RateLimitCounter();
        });

        counter.Count++;

        if (counter.Count > _options.RequestLimit)
        {
            _logger.LogWarning("限流触发: 客户端 {ClientId}, 请求数: {Count}, 限制: {Limit}",
                clientId, counter.Count, _options.RequestLimit);

            context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
            context.Response.ContentType = "application/json";

            var response = new
            {
                Code = "RATE_LIMIT_EXCEEDED",
                Message = $"请求过于频繁，请在 {_options.WindowSeconds} 秒后重试",
                RetryAfter = _options.WindowSeconds
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            return;
        }

        await _next(context);
    }

    private string GetClientId(HttpContext context)
    {
        // 1. 优先使用用户ID
        var userId = context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            return $"user:{userId}";
        }

        // 2. 使用API Key
        var apiKey = context.Request.Headers[HeaderConstants.Api.X_API_KEY].FirstOrDefault();
        if (!string.IsNullOrEmpty(apiKey))
        {
            return $"apikey:{apiKey}";
        }

        // 3. 使用IP地址
        var ip = context.Connection.RemoteIpAddress?.ToString();
        return $"ip:{ip ?? "unknown"}";
    }
}

/// <summary>
/// 限流计数器
/// </summary>
public class RateLimitCounter
{
    public int Count { get; set; }
}

/// <summary>
/// 限流选项
/// </summary>
public class RateLimitOptions
{
    /// <summary>
    /// 时间窗口内允许的请求数
    /// </summary>
    public int RequestLimit { get; set; } = 500;

    /// <summary>
    /// 时间窗口（秒）
    /// </summary>
    public int WindowSeconds { get; set; } = 60;

    /// <summary>
    /// 缓存键前缀
    /// </summary>
    public string KeyPrefix { get; set; } = "ratelimit";
}

/// <summary>
/// 限流中间件扩展
/// </summary>
public static class RateLimitMiddlewareExtensions
{
    public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder app, RateLimitOptions? options = null)
    {
        options ??= new RateLimitOptions();
        return app.UseMiddleware<RateLimitMiddleware>(options);
    }

    public static IServiceCollection AddRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RateLimitOptions>(configuration.GetSection("RateLimit"));
        return services;
    }
}
