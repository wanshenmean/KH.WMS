using System.Reflection;
using Autofac;
using Autofac.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;

namespace KH.WMS.Core.DependencyInjection;

/// <summary>
/// 服务集合扩展方法
/// </summary>
public class ServiceExtensions : Autofac.Module
{
    protected override void Load(ContainerBuilder builder)
    {
        List<Assembly> assemblies = AssemblyService.GetReferencedAssemblies();

        IServiceRegistrar registrar = new ServiceRegistrar();
        registrar.Register(builder, assemblies.ToArray());
    }

   
}
