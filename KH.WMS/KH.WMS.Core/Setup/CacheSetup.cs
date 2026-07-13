using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using KH.WMS.Core.Caching;
using KH.WMS.Core.Caching.Memory;

namespace KH.WMS.Core.Setup;

/// <summary>
/// 缓存配置
/// </summary>
public static class CacheSetup
{
    /// <summary>
    /// 配置缓存服务
    /// </summary>
    public static IServiceCollection AddCacheSetup(this IServiceCollection services, IConfiguration configuration)
    {
        // 注册内存缓存
        services.AddMemoryCache();

        // 注册缓存服务（统一接口）
        services.AddSingleton<ICacheService, MemoryCacheService>();
        services.AddSingleton<IMemoryCacheService, MemoryCacheService>();

        return services;
    }
}
