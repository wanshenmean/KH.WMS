using KH.WMS.Core.Constants;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KH.WMS.Core.Middlewares;

/// <summary>
/// CORS 中间件配置
/// </summary>
public static class CorsMiddlewareExtensions
{
    /// <summary>
    /// 配置 CORS 策略
    /// </summary>
    public static IServiceCollection AddCustomCors(this IServiceCollection services, IConfiguration configuration)
    {
        var corsOptions = configuration.GetSection("Cors").Get<CorsOptions>();

        services.AddCors(options =>
        {
            options.AddPolicy("DefaultPolicy", builder =>
            {
                // 配置允许的来源
                if (corsOptions?.AllowAnyOrigin ?? false)
                {
                    builder.AllowAnyOrigin();
                    // AllowAnyOrigin 不能与 AllowCredentials 同时使用
                }
                else
                {
                    builder.WithOrigins(corsOptions?.AllowedOrigins ?? Array.Empty<string>())
                           .AllowCredentials();
                }

                // 配置允许的方法
                if (corsOptions?.AllowAnyMethod ?? true)
                {
                    builder.AllowAnyMethod();
                }
                else
                {
                    builder.WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS", "PATCH");
                }

                // 配置允许的请求头
                if (corsOptions?.AllowAnyHeader ?? true)
                {
                    builder.AllowAnyHeader();
                }
                else
                {
                    builder.WithHeaders(HeaderConstants.Content.CONTENT_TYPE, HeaderConstants.Authentication.AUTHORIZATION, HeaderConstants.Security.X_REQUESTED_WITH);
                }

                // 暴露的响应头
                if (corsOptions?.ExposedHeaders != null && corsOptions.ExposedHeaders.Length > 0)
                {
                    builder.WithExposedHeaders(corsOptions.ExposedHeaders);
                }

                // 预检请求缓存时间
                if (corsOptions?.SetPreflightMaxAge ?? false)
                {
                    builder.SetPreflightMaxAge(TimeSpan.FromSeconds(corsOptions.PreflightMaxAgeSeconds));
                }
            });

            // 开发环境策略（允许所有来源，不支持凭证）
            options.AddPolicy("DevelopmentPolicy", builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader()
                       .SetPreflightMaxAge(TimeSpan.FromSeconds(3600));
            });

            // 生产环境策略（指定来源，支持凭证）
            options.AddPolicy("ProductionPolicy", builder =>
            {
                builder.WithOrigins(corsOptions?.AllowedOrigins ?? Array.Empty<string>())
                       .AllowAnyMethod()
                       .AllowAnyHeader()
                       .AllowCredentials();
            });
        });

        return services;
    }

    /// <summary>
    /// 使用 CORS
    /// </summary>
    public static IApplicationBuilder UseCustomCors(this IApplicationBuilder app, string policyName = "DefaultPolicy")
    {
        app.UseCors(policyName);
        return app;
    }
}

/// <summary>
/// CORS 配置选项
/// </summary>
public class CorsOptions
{
    /// <summary>
    /// 是否允许任何来源（注意：与 AllowCredentials 冲突，二选一）
    /// </summary>
    public bool AllowAnyOrigin { get; set; } = false;

    /// <summary>
    /// 允许的来源列表（当 AllowAnyOrigin 为 false 时使用）
    /// </summary>
    public string[] AllowedOrigins { get; set; } = Array.Empty<string>();

    /// <summary>
    /// 是否允许任何 HTTP 方法
    /// </summary>
    public bool AllowAnyMethod { get; set; } = true;

    /// <summary>
    /// 是否允许任何请求头
    /// </summary>
    public bool AllowAnyHeader { get; set; } = true;

    /// <summary>
    /// 暴露的响应头
    /// </summary>
    public string[]? ExposedHeaders { get; set; }

    /// <summary>
    /// 是否设置预检请求的最大缓存时间
    /// </summary>
    public bool SetPreflightMaxAge { get; set; } = false;

    /// <summary>
    /// 预检请求的最大缓存时间（秒）
    /// </summary>
    public int PreflightMaxAgeSeconds { get; set; } = 600;
}
