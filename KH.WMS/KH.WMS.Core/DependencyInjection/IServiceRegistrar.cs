using Autofac;
using Microsoft.Extensions.DependencyInjection;

namespace KH.WMS.Core.DependencyInjection;

/// <summary>
/// 服务注册器接口
/// </summary>
public interface IServiceRegistrar
{
    /// <summary>
    /// 注册服务
    /// </summary>
    void Register(ContainerBuilder builder, params System.Reflection.Assembly[] assemblies);
}
