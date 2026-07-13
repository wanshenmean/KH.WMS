using System.Reflection;
using Autofac;
using Microsoft.Extensions.Logging;

namespace KH.WMS.Core.Modularity;

/// <summary>
/// 模块加载器 - 使用 Autofac 容器
/// </summary>
public class ModuleLoader
{
    private readonly List<IModule> _modules = new();
    private readonly ILogger<ModuleLoader> _logger;

    public ModuleLoader(ILogger<ModuleLoader> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 从程序集加载模块
    /// </summary>
    public void LoadModulesFromAssembly(Assembly assembly, IModuleContext? context = null)
    {
        var moduleTypes = assembly.GetTypes()
            .Where(t => typeof(IModule).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
            .ToList();

        foreach (var moduleType in moduleTypes)
        {
            try
            {
                var module = (IModule)Activator.CreateInstance(moduleType)!;
                _modules.Add(module);
                _logger.LogInformation("Loaded module: {ModuleName} v{ModuleVersion}", module.Name, module.Version);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load module: {ModuleTypeName}", moduleType.Name);
            }
        }
    }

    /// <summary>
    /// 从多个程序集加载模块
    /// </summary>
    public void LoadModulesFromAssemblies(IEnumerable<Assembly> assemblies, IModuleContext? context = null)
    {
        foreach (var assembly in assemblies)
        {
            LoadModulesFromAssembly(assembly, context);
        }
    }

    /// <summary>
    /// 初始化所有模块
    /// </summary>
    public async Task InitializeModulesAsync(IModuleContext context)
    {
        var sortedModules = TopologicalSort(_modules);

        foreach (var module in sortedModules)
        {
            try
            {
                await module.InitializeAsync(context);
                _logger.LogInformation("Initialized module: {ModuleName}", module.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize module: {ModuleName}", module.Name);
                throw;
            }
        }
    }

    /// <summary>
    /// 启动所有模块
    /// </summary>
    public async Task StartModulesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var module in _modules)
        {
            try
            {
                await module.StartAsync(cancellationToken);
                _logger.LogInformation("Started module: {ModuleName}", module.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start module: {ModuleName}", module.Name);
                throw;
            }
        }
    }

    /// <summary>
    /// 停止所有模块
    /// </summary>
    public async Task StopModulesAsync(CancellationToken cancellationToken = default)
    {
        // 按相反顺序停止模块
        var sortedModules = TopologicalSort(_modules);
        var reversedModules = sortedModules.AsEnumerable().Reverse();

        foreach (var module in reversedModules)
        {
            try
            {
                await module.StopAsync(cancellationToken);
                _logger.LogInformation("Stopped module: {ModuleName}", module.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to stop module: {ModuleName}", module.Name);
            }
        }
    }

    /// <summary>
    /// 卸载所有模块
    /// </summary>
    public async Task ShutdownModulesAsync()
    {
        // 按相反顺序卸载模块
        var sortedModules = TopologicalSort(_modules);
        var reversedModules = sortedModules.AsEnumerable().Reverse();

        foreach (var module in reversedModules)
        {
            try
            {
                await module.ShutdownAsync();
                _logger.LogInformation("Shutdown module: {ModuleName}", module.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to shutdown module: {ModuleName}", module.Name);
            }
        }
    }

    /// <summary>
    /// 拓扑排序 - 根据依赖关系排序模块
    /// </summary>
    private List<IModule> TopologicalSort(List<IModule> modules)
    {
        var sorted = new List<IModule>();
        var visited = new HashSet<Guid>();
        var visiting = new HashSet<Guid>();

        void Visit(IModule module)
        {
            if (visited.Contains(module.ModuleId))
                return;

            if (visiting.Contains(module.ModuleId))
                throw new InvalidOperationException($"Circular dependency detected: {module.Name}");

            visiting.Add(module.ModuleId);

            foreach (var dependency in module.Dependencies)
            {
                var depModule = modules.FirstOrDefault(m => m.Name == dependency.ModuleName);
                if (depModule != null)
                {
                    Visit(depModule);
                }
            }

            visiting.Remove(module.ModuleId);
            visited.Add(module.ModuleId);
            sorted.Add(module);
        }

        foreach (var module in modules)
        {
            Visit(module);
        }

        return sorted;
    }

    /// <summary>
    /// 获取所有已加载的模块
    /// </summary>
    public IReadOnlyList<IModule> Modules => _modules.AsReadOnly();
}
