using KH.WMS.Core.License.Interfaces;
using KH.WMS.Core.License.Middleware;
using KH.WMS.Core.Middlewares;
using KH.WMS.Core.Monitoring.MiniProfiler;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace KH.WMS.Core.Setup;

/// <summary>
/// 中间件配置 - 统一入口
/// </summary>
public static class MiddlewareSetup
{
    /// <summary>
    /// 使用所有自定义中间件（按推荐顺序）
    /// </summary>
    /// <summary>
    /// 使用所有自定义中间件（按推荐顺序）
    /// </summary>
    public static IApplicationBuilder UseCustomMiddleware(this IApplicationBuilder app, IWebHostEnvironment env)
    {
        // 1. 异常处理（最外层）
        app.UseExceptionHandling();

        // 2. License 验证（在异常处理中间件之后，认证授权之前）
        app.UseLicenseValidation();
        using (var scope = app.ApplicationServices.CreateScope())
        {
            // 首次启动时自动初始化 License（生成密钥对 + 默认180天授权）
            var licenseService = scope.ServiceProvider.GetRequiredService<ILicenseService>();
            licenseService.EnsureDefaultLicense();
        }

        // 3. HTTPS 重定向（生产环境）
        if (!env.IsDevelopment())
        {
            app.UseHttpsRedirection();
        }

        // 4. 静态文件
        app.UseCustomStaticFiles(env);

        // 5. CORS
        app.UseCustomCors();

        // 6. 请求日志
        app.UseRequestLogging();

        // 7. 限流
        //app.UseRateLimiting();

        // 8. 路由
        app.UseRouting();

        // 9. 性能监控（在 UseRouting 之后）
        app.UseMiniProfilerCustom();

        // 10. 认证
        app.UseAuthentication();

        // 11. 授权
        app.UseAuthorization();

        // 12. 端点映射
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapRazorPages();  // MiniProfiler UI 需要
        });

        return app;
    }

}
