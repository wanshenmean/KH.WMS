using System.Reflection;
using Autofac;
using Autofac.Extras.DynamicProxy;
using Castle.DynamicProxy;
using KH.WMS.Core.AOP.Interceptors;
using KH.WMS.Core.DependencyInjection.ServiceLifetimes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
// TransactionInterceptor 已替换为 TransactionActionFilter (ASP.NET Core Filter 方案)

namespace KH.WMS.Core.DependencyInjection;

/// <summary>
/// 自动服务注册器
/// </summary>
public class ServiceRegistrar : IServiceRegistrar
{
    public void Register(ContainerBuilder builder, params Assembly[] assemblies)
    {
        // 如果没有传入程序集，使用调用程序集
        if (assemblies == null || assemblies.Length == 0)
        {
            assemblies = new[] { Assembly.GetCallingAssembly() };
        }

        // 注册所有带生命周期标记的服务
        RegisterServicesWithLifetime(builder, assemblies);

        Console.WriteLine("服务注册完成");
    }

    private Type[] interceptors = new Type[]
    {
        typeof(LoggingInterceptor),
        typeof(CachingInterceptor),
        typeof(ConfigValidationInterceptor),
        typeof(ExceptionInterceptor),
        typeof(PerformanceInterceptor)
    };

    private void RegisterServicesWithLifetime(ContainerBuilder builder, Assembly[] assemblies)
    {
        var serviceTypes = new[]
        {
            typeof(RegisteredServiceAttribute),
            typeof(SelfRegisteredServiceAttribute)
        };

        builder.RegisterType<LoggingInterceptor>();
        builder.RegisterType<CachingInterceptor>();
        builder.RegisterType<ConfigValidationInterceptor>();
        builder.RegisterType<ExceptionInterceptor>();
        builder.RegisterType<PerformanceInterceptor>();

        // 注册拦截器查询服务（不启用拦截器，避免无限循环）
        builder.RegisterType<InterceptorQueryService>()
            .As<IInterceptorQueryService>()
            .InstancePerLifetimeScope();

        Console.WriteLine($"[拦截器注册] 已注册 {interceptors.Length} 个拦截器: {string.Join(", ", interceptors.Select(i => i.Name))}");

        foreach (var assembly in assemblies)
        {
            // 扫描所有带特性的类（包括抽象泛型类）
            var types = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsInterface && (!t.IsAbstract || (t.IsAbstract && t.IsGenericTypeDefinition)))
                .SelectMany(t => t.GetCustomAttributes(), (t, attr) => new { Type = t, Attribute = attr })
                .Where(x => serviceTypes.Contains(x.Attribute.GetType())).ToList();

            foreach (var item in types)
            {
                var interfaces = item.Type.GetInterfaces();
                Type serviceType = interfaces.FirstOrDefault() ?? item.Type;

                // 对于泛型类型，优先选择泛型接口
                //if (item.Type.IsGenericType || item.Type.IsGenericTypeDefinition)
                //{
                //    // 查找泛型参数数量匹配的泛型接口
                //    var genericInterfaces = interfaces.Where(i =>
                //        i.IsGenericType &&
                //        i.GetGenericArguments().Length == item.Type.GetGenericArguments().Length).ToList();
                //    serviceType = genericInterfaces.FirstOrDefault() ?? interfaces.FirstOrDefault() ?? item.Type;
                //}

                if (item.Attribute is RegisteredServiceAttribute registeredAttr)
                {
                    if (serviceType == item.Type)
                    {
                        Console.WriteLine($"类型 {item.Type.Name} 标记为 RegisteredServiceAttribute，但未实现任何接口，建议使用 SelfRegisteredServiceAttribute 进行自注册。");
                        Console.WriteLine($"继续注册 {item.Type.Name} 为自注册服务。");

                        if (registeredAttr.WithoutInterceptor)
                        {
                            RegisterServiceWithoutInterceptor(builder, item.Type, item.Attribute, assemblies);
                            continue;
                        }
                        RegisterSelfService(builder, item.Type, registeredAttr.Lifetime);
                        continue;
                    }
                    if (interfaces.Length > 0 && serviceType == null)
                    {
                        Console.WriteLine($"类型 {item.Type.Name} 有多重实现接口，未指定接口类型。");
                        continue;
                    }

                    if (interfaces.Length > 0 && registeredAttr.ServiceType != null)
                    {
                        serviceType = registeredAttr.ServiceType;
                    }

                    // 只有开放泛型定义才使用 RegisterGeneric
                    if (item.Type.IsGenericTypeDefinition)
                    {
                        //if (registeredAttr.WithoutInterceptor)
                        //{
                        //    RegisterServiceWithoutInterceptor(builder, item.Type, item.Attribute, assemblies);
                        //    continue;
                        //}
                        RegisterGenericService(builder, item.Type, serviceType, registeredAttr.Lifetime);
                    }
                    else
                    {
                        if (registeredAttr.WithoutInterceptor)
                        {
                            RegisterServiceWithoutInterceptor(builder, item.Type, item.Attribute, assemblies);
                            continue;
                        }
                        RegisterService(builder, serviceType, item.Type, registeredAttr.Lifetime);
                    }

                }
                else if (item.Attribute is SelfRegisteredServiceAttribute selfRegisteredAttr)
                {
                    if (selfRegisteredAttr.WithoutInterceptor)
                    {
                        RegisterServiceWithoutInterceptor(builder, item.Type, item.Attribute, assemblies);
                        continue;
                    }
                    RegisterSelfService(builder, item.Type, selfRegisteredAttr.Lifetime);
                }
            }
        }
    }

    /// <summary>
    /// 注册服务但不添加拦截器（用于 LoggerService 等基础服务）
    /// </summary>
    private void RegisterServiceWithoutInterceptor(ContainerBuilder builder, Type implementationType, Attribute attribute, Assembly[] assemblies)
    {
        if (attribute is RegisteredServiceAttribute registeredAttr)
        {
            var interfaces = implementationType.GetInterfaces();
            var serviceType = interfaces.FirstOrDefault() ?? implementationType;

            switch (registeredAttr.Lifetime)
            {
                case ServiceLifetime.Singleton:
                    builder.RegisterType(implementationType).As(serviceType).SingleInstance();
                    Console.WriteLine($"注册 Singleton 服务（无拦截器）: {serviceType.Name}");
                    break;
                case ServiceLifetime.Scoped:
                    builder.RegisterType(implementationType).As(serviceType).InstancePerLifetimeScope();
                    Console.WriteLine($"注册 Scoped 服务（无拦截器）: {serviceType.Name}");
                    break;
                case ServiceLifetime.Transient:
                    builder.RegisterType(implementationType).As(serviceType).InstancePerDependency();
                    Console.WriteLine($"注册 Transient 服务（无拦截器）: {serviceType.Name}");
                    break;
            }
        }
        else if (attribute is SelfRegisteredServiceAttribute selfRegisteredAttr)
        {
            switch (selfRegisteredAttr.Lifetime)
            {
                case ServiceLifetime.Singleton:
                    builder.RegisterType(implementationType).SingleInstance();
                    Console.WriteLine($"注册自注册 Singleton 服务（无拦截器）: {implementationType.Name}");
                    break;
                case ServiceLifetime.Scoped:
                    builder.RegisterType(implementationType).InstancePerLifetimeScope();
                    Console.WriteLine($"注册自注册 Scoped 服务（无拦截器）: {implementationType.Name}");
                    break;
                case ServiceLifetime.Transient:
                    builder.RegisterType(implementationType).InstancePerDependency();
                    Console.WriteLine($"注册自注册 Transient 服务（无拦截器）: {implementationType.Name}");
                    break;
            }
        }
    }

    private void RegisterService(ContainerBuilder builder, Type serviceType, Type implementationType, ServiceLifetime lifetime)
    {
        Console.WriteLine($"[RegisterService] 准备注册: ServiceType={serviceType.Name}, ImplementationType={implementationType.Name}, Lifetime={lifetime}");

        switch (lifetime)
        {
            case ServiceLifetime.Singleton:
                builder.RegisterType(implementationType).As(serviceType).SingleInstance().EnableInterfaceInterceptors().InterceptedBy(interceptors);
                Console.WriteLine($"注册 Singleton 服务: {serviceType.Name}");
                break;
            case ServiceLifetime.Scoped:
                builder.RegisterType(implementationType).As(serviceType).InstancePerLifetimeScope().EnableInterfaceInterceptors().InterceptedBy(interceptors);
                Console.WriteLine($"注册 Scoped 服务: {serviceType.Name}");
                break;
            case ServiceLifetime.Transient:
                builder.RegisterType(implementationType).As(serviceType).InstancePerDependency().EnableInterfaceInterceptors().InterceptedBy(interceptors);
                Console.WriteLine($"注册 Transient 服务: {serviceType.Name}");
                break;
        }
    }

    private void RegisterSelfService(ContainerBuilder builder, Type implementationType, ServiceLifetime lifetime)
    {
        switch (lifetime)
        {
            case ServiceLifetime.Singleton:
                builder.RegisterType(implementationType).SingleInstance().EnableClassInterceptors().InterceptedBy(interceptors);
                Console.WriteLine($"注册自注册 Singleton 服务: {implementationType.Name}");
                break;
            case ServiceLifetime.Scoped:
                builder.RegisterType(implementationType).InstancePerLifetimeScope().EnableClassInterceptors().InterceptedBy(interceptors);
                Console.WriteLine($"注册自注册 Scoped 服务: {implementationType.Name}");
                break;
            case ServiceLifetime.Transient:
                builder.RegisterType(implementationType).InstancePerDependency().EnableClassInterceptors().InterceptedBy(interceptors);
                Console.WriteLine($"注册自注册 Transient 服务: {implementationType.Name}");
                break;
        }
    }

    /// <summary>
    /// 注册泛型服务
    /// </summary>
    private void RegisterGenericService(ContainerBuilder builder, Type genericType, Type genericServiceType, ServiceLifetime lifetime)
    {
        switch (lifetime)
        {
            case ServiceLifetime.Singleton:
                builder.RegisterGeneric(genericType).As(genericServiceType).SingleInstance().EnableInterfaceInterceptors().InterceptedBy(interceptors);
                Console.WriteLine($"注册泛型 Singleton 服务: {genericType.Name} -> {genericServiceType.Name}");
                break;
            case ServiceLifetime.Scoped:
                builder.RegisterGeneric(genericType).As(genericServiceType).InstancePerLifetimeScope().EnableInterfaceInterceptors().InterceptedBy(interceptors);
                Console.WriteLine($"注册泛型 Scoped 服务: {genericType.Name} -> {genericServiceType.Name}");
                break;
            case ServiceLifetime.Transient:
                builder.RegisterGeneric(genericType).As(genericServiceType).InstancePerDependency().EnableInterfaceInterceptors().InterceptedBy(interceptors);
                Console.WriteLine($"注册泛型 Transient 服务: {genericType.Name} -> {genericServiceType.Name}");
                break;
        }
    }

    /// <summary>
    /// 注册泛型自注册服务
    /// </summary>
    private void RegisterGenericSelfService(ContainerBuilder builder, Type genericType, ServiceLifetime lifetime)
    {
        switch (lifetime)
        {
            case ServiceLifetime.Singleton:
                builder.RegisterGeneric(genericType).SingleInstance().EnableInterfaceInterceptors().InterceptedBy(interceptors);
                Console.WriteLine($"注册泛型自注册 Singleton 服务: {genericType.Name}");
                break;
            case ServiceLifetime.Scoped:
                builder.RegisterGeneric(genericType).InstancePerLifetimeScope().EnableInterfaceInterceptors().InterceptedBy(interceptors);
                Console.WriteLine($"注册泛型自注册 Scoped 服务: {genericType.Name}");
                break;
            case ServiceLifetime.Transient:
                builder.RegisterGeneric(genericType).InstancePerDependency().EnableInterfaceInterceptors().InterceptedBy(interceptors);
                Console.WriteLine($"注册泛型自注册 Transient 服务: {genericType.Name}");
                break;
        }
    }
}
