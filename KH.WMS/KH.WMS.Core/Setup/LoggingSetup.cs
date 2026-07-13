using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace KH.WMS.Core.Setup;

/// <summary>
/// 日志配置
/// </summary>
public static class LoggingSetup
{
    /// <summary>
    /// 配置 Serilog
    /// </summary>
    public static IHostBuilder AddSerilog(this IHostBuilder hostBuilder)
    {
        return KH.WMS.Core.Logging.Serilog.SerilogSetup.AddSerilog(hostBuilder, "KH.WMS", "Logs");
    }

    /// <summary>
    /// 配置服务日志
    /// </summary>
    public static IServiceCollection AddLoggingSetup(this IServiceCollection services, IConfiguration configuration)
    {
        // 添加日志服务
        services.AddSingleton<KH.WMS.Core.Logging.ILoggerService, KH.WMS.Core.Logging.Serilog.LoggerService>();

        return services;
    }
}
