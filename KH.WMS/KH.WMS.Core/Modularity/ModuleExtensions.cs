using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace KH.WMS.Core.Modularity;

/// <summary>
/// 模块自动发现和注册扩展
/// </summary>
public static class ModuleExtensions
{
    /// <summary>
    /// 自动发现并注册所有模块
    /// </summary>
    public static ContainerBuilder RegisterModules(this ContainerBuilder builder, IServiceCollection services)
    {
        var loggerFactory = services.BuildServiceProvider().GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger("WMS.Framework.Modularity.ModuleExtensions");

        logger.LogInformation("开始自动发现和注册模块...");

        // 获取所有引用的程序集（排除系统程序集）
        var assemblies = GetReferencedAssemblies();

        // 查找所有模块类型
        var moduleTypes = FindModuleTypes(assemblies, logger);

        // 按依赖关系排序
        var sortedModules = SortModulesByDependencies(moduleTypes, logger);

        // 注册共享基础设施
        RegisterSharedInfrastructure(builder, services);

        // 注册模块服务
        RegisterModules(builder, sortedModules, logger);

        return builder;
    }

    /// <summary>
    /// 自动初始化所有模块
    /// </summary>
    public static async Task InitializeModulesAsync(this IServiceProvider serviceProvider)
    {
        var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("WMS.Framework.Modularity.ModuleExtensions");
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

        logger.LogInformation("开始初始化所有模块...");

        // 获取所有引用的程序集
        var assemblies = GetReferencedAssemblies();

        // 查找所有模块类型
        var moduleTypes = FindModuleTypes(assemblies, logger);

        // 按依赖关系排序
        var sortedModules = SortModulesByDependencies(moduleTypes, logger);

        // 创建模块上下文
        var moduleContext = new ModuleContext(serviceProvider, configuration, loggerFactory, serviceProvider.GetService<IServiceCollection>());

        // 初始化所有模块
        var initializedModules = new List<ModuleBase>();
        foreach (var moduleType in sortedModules)
        {
            try
            {
                var module = (ModuleBase?)Activator.CreateInstance(moduleType);
                if (module != null)
                {
                    await module.InitializeAsync(moduleContext);
                    initializedModules.Add(module);
                    logger.LogInformation("✓ 模块 {ModuleName} ({ModuleVersion}) 初始化成功", module.Name, module.Version);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "✗ 模块 {ModuleTypeName} 初始化失败: {ErrorMessage}", moduleType.Name, ex.Message);
            }
        }

        // 启动所有模块
        foreach (var module in initializedModules)
        {
            try
            {
                await module.StartAsync();
                logger.LogInformation("✓ 模块 {ModuleName} 启动成功", module.Name);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "✗ 模块 {ModuleName} 启动失败: {ErrorMessage}", module.Name, ex.Message);
            }
        }

        logger.LogInformation("所有模块初始化完成，成功初始化 {ModuleCount} 个模块", initializedModules.Count);
    }

    /// <summary>
    /// 获取所有引用的程序集（排除系统程序集）
    /// </summary>
    private static List<Assembly> GetReferencedAssemblies()
    {
        var assemblies = new List<Assembly>();
        Assembly? entryAssembly = null;

        try
        {
            var dependencyContext = DependencyContext.Default;
            entryAssembly = Assembly.GetEntryAssembly();

            if (dependencyContext == null || entryAssembly == null)
            {
                // 如果无法获取 DependencyContext 或入口程序集，返回空列表
                return entryAssembly != null ? new List<Assembly> { entryAssembly } : assemblies;
            }

            var loadedAssemblies = new HashSet<string>();

            // 遍历所有运行时库
            foreach (var runtimeLibrary in dependencyContext.RuntimeLibraries)
            {
                if (IsSystemLibrary(runtimeLibrary.Name))
                {
                    continue;
                }

                try
                {
                    var assembly = Assembly.Load(new AssemblyName(runtimeLibrary.Name));
                    if (assembly != null && loadedAssemblies.Add(assembly.FullName))
                    {
                        assemblies.Add(assembly);
                    }
                }
                catch
                {
                    // 忽略加载失败
                }
            }

            // 添加入口程序集
            if (loadedAssemblies.Add(entryAssembly.FullName))
            {
                assemblies.Add(entryAssembly);
            }
        }
        catch
        {
            // 如果出现任何错误，至少返回入口程序集
            if (entryAssembly != null && !assemblies.Contains(entryAssembly))
            {
                assemblies.Add(entryAssembly);
            }
        }

        return assemblies;
    }

    /// <summary>
    /// 判断是否为系统库
    /// </summary>
    private static bool IsSystemLibrary(string? libraryName)
    {
        if (string.IsNullOrWhiteSpace(libraryName))
        {
            return true;
        }

        return libraryName.StartsWith("System.") ||
               libraryName.StartsWith("Microsoft.") ||
               libraryName.StartsWith("Net.") ||
               libraryName.StartsWith("Autofac") ||
               libraryName.StartsWith("Serilog") ||
               libraryName.StartsWith("SqlSugar") ||
               libraryName.StartsWith("StackExchange") ||
               libraryName.StartsWith("Newtonsoft") ||
               libraryName.StartsWith("MediatR") ||
               libraryName.Equals("mscorlib") ||
               libraryName.Equals("netstandard") ||
               libraryName.StartsWith("runtime.");
    }

    /// <summary>
    /// 查找所有模块类型
    /// </summary>
    private static List<Type> FindModuleTypes(List<Assembly> assemblies, ILogger logger)
    {
        var moduleTypes = new List<Type>();

        foreach (var assembly in assemblies)
        {
            try
            {
                var types = assembly.GetTypes();
                foreach (var type in types)
                {
                    if (typeof(ModuleBase).IsAssignableFrom(type) &&
                        !type.IsAbstract &&
                        !type.IsInterface &&
                        type.GetConstructor(Type.EmptyTypes) != null)
                    {
                        moduleTypes.Add(type);
                        logger.LogDebug("发现模块: {ModuleTypeName}", type.FullName);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "扫描程序集 {AssemblyName} 时出错", assembly.FullName);
            }
        }

        logger.LogInformation("共发现 {ModuleCount} 个模块", moduleTypes.Count);
        return moduleTypes;
    }

    /// <summary>
    /// 按依赖关系排序模块
    /// </summary>
    private static List<Type> SortModulesByDependencies(List<Type> moduleTypes, ILogger logger)
    {
        var sorted = new List<Type>();
        var visited = new HashSet<Type>();

        foreach (var moduleType in moduleTypes)
        {
            VisitModule(moduleType, moduleTypes, sorted, visited, logger);
        }

        return sorted;
    }

    /// <summary>
    /// 递归访问模块（用于拓扑排序）
    /// </summary>
    private static void VisitModule(Type moduleType, List<Type> allModules, List<Type> sorted, HashSet<Type> visited, ILogger logger)
    {
        if (visited.Contains(moduleType))
        {
            return;
        }

        visited.Add(moduleType);

        // 获取模块实例以检查依赖
        try
        {
            var module = (ModuleBase?)Activator.CreateInstance(moduleType);
            if (module?.Dependencies != null)
            {
                foreach (var dependency in module.Dependencies)
                {
                    var dependencyModule = allModules.FirstOrDefault(m =>
                    {
                        try
                        {
                            var depModule = (ModuleBase?)Activator.CreateInstance(m);
                            return depModule?.Name == dependency.ModuleName;
                        }
                        catch
                        {
                            return false;
                        }
                    });

                    if (dependencyModule != null)
                    {
                        VisitModule(dependencyModule, allModules, sorted, visited, logger);
                    }
                }
            }
        }
        catch
        {
            // 忽略实例化失败
        }

        sorted.Add(moduleType);
    }

    /// <summary>
    /// 注册共享基础设施
    /// </summary>
    private static void RegisterSharedInfrastructure(ContainerBuilder builder, IServiceCollection services)
    {
        // 领域事件分发器和 Saga 协调器在各自的模块中注册
    }

    /// <summary>
    /// 注册模块服务到 Autofac 容器
    /// </summary>
    private static void RegisterModules(ContainerBuilder builder, List<Type> moduleTypes, ILogger logger)
    {
        foreach (var moduleType in moduleTypes)
        {
            try
            {
                var module = (ModuleBase?)Activator.CreateInstance(moduleType);
                if (module != null)
                {
                    // 调用模块的 RegisterServices 方法
                    var registerMethod = moduleType.GetMethod("RegisterServices", new[] { typeof(ContainerBuilder) });
                    if (registerMethod != null)
                    {
                        registerMethod.Invoke(module, new object[] { builder });
                        logger.LogInformation("✓ 注册模块服务: {ModuleName} ({ModuleVersion})", module.Name, module.Version);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "✗ 注册模块 {ModuleTypeName} 失败: {ErrorMessage}", moduleType.Name, ex.Message);
            }
        }
    }
}
