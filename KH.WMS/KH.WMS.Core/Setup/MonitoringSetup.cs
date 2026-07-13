using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KH.WMS.Core.Setup;

/// <summary>
/// 性能监控配置
/// </summary>
public static class MonitoringSetup
{
    /// <summary>
    /// 配置 MiniProfiler
    /// </summary>
    public static IServiceCollection AddMonitoringSetup(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        return KH.WMS.Core.Monitoring.MiniProfiler.MiniProfilerSetup.AddMiniProfilerCustom(services, configuration, environment);
    }

    /// <summary>
    /// 使用 MiniProfiler
    /// </summary>
    public static IApplicationBuilder UseMiniProfiler(this IApplicationBuilder app)
    {
        return KH.WMS.Core.Monitoring.MiniProfiler.MiniProfilerSetup.UseMiniProfilerCustom(app);
    }
}
