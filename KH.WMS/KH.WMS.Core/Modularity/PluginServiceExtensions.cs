//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Logging;

//namespace KH.WMS.Core.Modularity;

///// <summary>
///// 插件系统服务注册扩展
///// </summary>
//public static class PluginServiceExtensions
//{
//    /// <summary>
//    /// 注册插件系统服务
//    /// </summary>
//    /// <param name="services">服务集合</param>
//    /// <param name="pluginDirectory">插件目录，默认为 ./plugins</param>
//    /// <returns>服务集合</returns>
//    public static IServiceCollection AddPluginSystem(this IServiceCollection services, string? pluginDirectory = null)
//    {
//        // 注册插件管理器为单例
//        services.AddSingleton<IPluginManager>(sp =>
//        {
//            // 使用 Framework 提供的日志工厂创建日志记录器
//            var loggerFactory = Logging.LoggerFactoryExtensions.CreateSerilogLoggerFactory();
//            var logger = loggerFactory.CreateLogger<PluginManager>();
//            return new PluginManager(logger, pluginDirectory);
//        });

//        return services;
//    }

//    /// <summary>
//    /// 注册插件系统服务并自动加载插件
//    /// </summary>
//    /// <param name="services">服务集合</param>
//    /// <param name="pluginDirectory">插件目录，默认为 ./plugins</param>
//    /// <param name="autoLoadPlugins">是否自动加载所有插件</param>
//    /// <returns>服务集合</returns>
//    public static async Task<IServiceCollection> AddPluginSystemAsync(
//        this IServiceCollection services,
//        string? pluginDirectory = null,
//        bool autoLoadPlugins = true)
//    {
//        // 注册插件管理器
//        services.AddPluginSystem(pluginDirectory);

//        if (autoLoadPlugins)
//        {
//            // 构建服务提供器
//            var serviceProvider = services.BuildServiceProvider();
//            var pluginManager = serviceProvider.GetRequiredService<IPluginManager>();

//            // 自动加载所有插件
//            await pluginManager.LoadAllPluginsAsync();

//            // 初始化并启动所有插件
//            foreach (var plugin in pluginManager.Plugins.Values)
//            {
//                try
//                {
//                    await pluginManager.InitializePluginAsync(plugin.PluginId);
//                    await pluginManager.StartPluginAsync(plugin.PluginId);
//                }
//                catch (Exception ex)
//                {
//                    // 插件启动失败，记录到日志
//                    var logger = serviceProvider.GetRequiredService<ILogger<PluginManager>>();
//                    logger.LogError(ex, "启动插件失败: {PluginName}", plugin.Name);
//                }
//            }
//        }

//        return services;
//    }
//}
