using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using KH.WMS.Core.Middlewares;
using KH.WMS.Core.Setup;
using KH.WMS.Core.Database;
using KH.WMS.Core.Filters.Authorization;

// 此命名空间是为了向后兼容
namespace KH.WMS.Core.Setup;

/// <summary>
/// 服务集合配置 - 统一入口
/// </summary>
public static class ServiceCollectionSetup
{
    /// <summary>
    /// 添加所有基础设施服务
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        // 1. 配置选项
        //services.AddConfigurationOptions(configuration);

        // 2. 数据库
        services.AddSqlSugarSetup(configuration);

        // 3. 缓存
        services.AddCacheSetup(configuration);

        // 4. 认证
        services.AddAuthenticationSetup(configuration);

        // 5. 日志
        services.AddLoggingSetup(configuration);

        // 7. 性能监控
        services.AddMonitoringSetup(configuration, environment);

        // 8. 后台服务
        //services.AddBackgroundServiceSetup(configuration);

        // 9. API 文档
        services.AddApiDocumentationSetup(configuration);

        // 11. CORS
        services.AddCustomCors(configuration);

        // 12. 限流
        services.AddRateLimiting(configuration);

        // 13. HTTP 客户端
        services.AddHttpClient();

        // 14. 数据库初始化已移至 Program.cs 中同步执行，确保中间件配置前表已创建

        services.AddMvc(options =>
        {
            options.Filters.Add<ApiAuthorizeFilter>();
            options.Filters.Add<Filters.Result.TraceIdResultFilter>();
        });

        return services;
    }

}
