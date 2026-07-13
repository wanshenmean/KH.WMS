using System.Text;
using KH.WMS.Core.Constants;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Profiling;
using StackExchange.Profiling.Storage;

namespace KH.WMS.Core.Monitoring.MiniProfiler;

/// <summary>
/// MiniProfiler 配置
/// </summary>
public static class MiniProfilerSetup
{
    /// <summary>
    /// 添加 MiniProfiler 服务
    /// </summary>
    public static IServiceCollection AddMiniProfilerCustom(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        var miniProfilerSettings = configuration.GetSection(AppSettingsConstants.MiniProfiler).Get<MiniProfilerSettings>();
        miniProfilerSettings ??= new MiniProfilerSettings();

        // 创建存储实例（单例）
        var storage = new MiniProfilerMemoryStorage();

        // 注册存储服务
        services.AddSingleton<IAsyncStorage>(storage);

        // 调用官方 MiniProfiler 的扩展方法
        services.AddMiniProfiler(options =>
        {
            options.RouteBasePath = miniProfilerSettings.RouteBasePath;
            options.PopupRenderPosition = RenderPosition.BottomLeft;
            options.PopupShowTimeWithChildren = true;

            // 生产环境配置
            if (!miniProfilerSettings.EnableInProduction && !environment.IsDevelopment())
            {
                options.ShouldProfile = _ => false;
            }
            else
            {
                // 开发环境或配置为启用时，分析所有请求
                options.ShouldProfile = _ => true;
            }

            // 数据库优化
            options.TrackConnectionOpenClose = miniProfilerSettings.TrackConnectionOpenClose;

            // 配置存储（使用同一个实例）
            options.Storage = storage;
        });

        return services;
    }

    /// <summary>
    /// 使用 MiniProfiler 中间件
    /// </summary>
    public static IApplicationBuilder UseMiniProfilerCustom(this IApplicationBuilder app)
    {
        app.UseMiniProfiler();
        app.UseMiddleware<MiniProfilerInjectorMiddleware>();
        return app;
    }
}

/// <summary>
/// MiniProfiler 配置选项
/// </summary>
public class MiniProfilerSettings
{
    /// <summary>
    /// 路由基础路径
    /// </summary>
    public string RouteBasePath { get; set; } = "/profiler";

    /// <summary>
    /// 是否在生产环境启用
    /// </summary>
    public bool EnableInProduction { get; set; } = false;

    /// <summary>
    /// 是否跟踪数据库连接
    /// </summary>
    public bool TrackConnectionOpenClose { get; set; } = true;

    /// <summary>
    /// 堆栈跟踪长度
    /// </summary>
    public int StackTraceLength { get; set; } = 5;
}


/// <summary>
/// MiniProfiler 脚本注入中间件
/// 自动在 HTML 页面中注入 MiniProfiler 脚本
/// </summary>
public class MiniProfilerInjectorMiddleware
{
    private readonly RequestDelegate _next;

    public MiniProfilerInjectorMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // 使用原始响应流
        var originalBodyStream = context.Response.Body;

        using var memoryStream = new MemoryStream();
        context.Response.Body = memoryStream;

        try
        {
            await _next(context);

            // 只处理 HTML 响应且状态码为 200
            if (context.Response.StatusCode == 200 &&
                context.Response.ContentType != null &&
                context.Response.ContentType.Contains("text/html", StringComparison.OrdinalIgnoreCase))
            {
                memoryStream.Seek(0, SeekOrigin.Begin);
                using var reader = new StreamReader(memoryStream, leaveOpen: true);
                var html = await reader.ReadToEndAsync();

                // 在 </head> 前注入 MiniProfiler 脚本
                var miniProfilerScript = @"<script async=""async"" id=""mini-profiler"" src=""/profiler/includes.min.js"" data-path=""/profiler/"" data-position=""Left"" data-scheme=""Light"" data-authorized=""true"" data-children=""true"" data-max-traces=""15"" data-toggle-shortcut=""Alt+P"" data-trivial-milliseconds=""2.0""></script>";

                if (html.Contains("</head>"))
                {
                    html = html.Replace("</head>", $"{miniProfilerScript}</head>");
                }
                else if (html.Contains("<body>"))
                {
                    // 如果没有 </head>，在 <body> 前注入
                    html = html.Replace("<body>", $"{miniProfilerScript}<body>");
                }

                var outputBytes = Encoding.UTF8.GetBytes(html);
                context.Response.ContentLength = outputBytes.Length;
                context.Response.Body = originalBodyStream;
                await context.Response.Body.WriteAsync(outputBytes, context.RequestAborted);
            }
            else
            {
                // 非 HTML 响应，直接复制原始内容
                memoryStream.Seek(0, SeekOrigin.Begin);
                await memoryStream.CopyToAsync(originalBodyStream, context.RequestAborted);
            }
        }
        finally
        {
            // 确保恢复原始响应流
            context.Response.Body = originalBodyStream;
        }
    }
}