using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using KH.WMS.Core.Constants;

namespace KH.WMS.Core.Middlewares;

/// <summary>
/// 静态文件中间件扩展
/// </summary>
public static class StaticFileMiddlewareExtensions
{
    /// <summary>
    /// 配置静态文件服务
    /// </summary>
    public static IApplicationBuilder UseCustomStaticFiles(this IApplicationBuilder app, IWebHostEnvironment webHostEnvironment, string wwwRoot = "wwwroot")
    {
        app.UseStaticFiles();

        // 使用默认静态文件
        app.UseStaticFiles(new StaticFileOptions
        {
            OnPrepareResponse = ctx =>
            {
                // 缓存控制
                const int cacheDuration = 60 * 60 * 24; // 1天
                ctx.Context.Response.Headers.Add(HeaderConstants.Cache.CACHE_CONTROL, $"public, max-age={cacheDuration}");
            }
        });

        // 自定义静态文件目录
        var fileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(webHostEnvironment.WebRootPath);

        var contentTypeProvider = new FileExtensionContentTypeProvider();

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = fileProvider,
            ContentTypeProvider = contentTypeProvider,
            OnPrepareResponse = ctx =>
            {
                var headers = ctx.Context.Response.Headers;
                headers.Add("X-Content-Type-Options", "nosniff");
                headers.Add("X-Frame-Options", "DENY");
            }
        });

        return app;
    }

    /// <summary>
    /// 配允许可浏览目录
    /// </summary>
    public static IApplicationBuilder UseDirectoryBrowsing(this IApplicationBuilder app, string directoryPath)
    {
        app.UseDirectoryBrowser(new DirectoryBrowserOptions
        {
            FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(directoryPath),
            RequestPath = new PathString("/files")
        });

        return app;
    }
}
