using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace KH.WMS.Core.Modularity;

/// <summary>
/// WMS模块接口
/// </summary>
public interface IModule
{
    /// <summary>
    /// 模块唯一标识
    /// </summary>
    Guid ModuleId { get; }

    /// <summary>
    /// 模块名称
    /// </summary>
    string Name { get; }

    /// <summary>
    /// 模块版本
    /// </summary>
    Version Version { get; }

    /// <summary>
    /// 模块描述
    /// </summary>
    string Description { get; }

    /// <summary>
    /// 模块作者
    /// </summary>
    string Author { get; }

    /// <summary>
    /// 依赖的其他模块
    /// </summary>
    IEnumerable<ModuleDependency> Dependencies { get; }

    /// <summary>
    /// 模块初始化
    /// </summary>
    Task InitializeAsync(IModuleContext context);

    /// <summary>
    /// 模块启动
    /// </summary>
    Task StartAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 模块停止
    /// </summary>
    Task StopAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 模块卸载
    /// </summary>
    Task ShutdownAsync();
}


