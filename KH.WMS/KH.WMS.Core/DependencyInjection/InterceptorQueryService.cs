using System.Reflection;
using Autofac;
using Autofac.Core;
using Autofac.Core.Activators.Reflection;
using Autofac.Core.Registration;
using Microsoft.Extensions.DependencyInjection;

namespace KH.WMS.Core.DependencyInjection;

/// <summary>
/// 拦截器查询服务实现 - 通过 Autofac API 查询
/// </summary>
public class InterceptorQueryService : IInterceptorQueryService
{
    private readonly ILifetimeScope _lifetimeScope;

    public InterceptorQueryService(ILifetimeScope lifetimeScope)
    {
        _lifetimeScope = lifetimeScope;
    }

    public List<InterceptorServiceInfo> GetRegisteredServices()
    {
        var services = new List<InterceptorServiceInfo>();
        var registrations = _lifetimeScope.ComponentRegistry.Registrations;

        foreach (var registration in registrations)
        {
            var info = TryGetInterceptorInfo(registration);
            if (info != null && (info.HasInterfaceInterceptors || info.HasClassInterceptors || info.Interceptors.Count > 0))
            {
                // 只返回项目自定义的服务（过滤掉 Autofac 自己注册的服务）
                if (IsProjectService(info))
                {
                    services.Add(info);
                }
            }
        }

        return services.OrderBy(s => s.ServiceName).ToList();
    }

    public InterceptorServiceInfo? GetServiceInfo(string serviceName)
    {
        var registrations = _lifetimeScope.ComponentRegistry.Registrations;

        foreach (var registration in registrations)
        {
            var info = TryGetInterceptorInfo(registration);
            if (info != null && info.ServiceName.Equals(serviceName, StringComparison.OrdinalIgnoreCase))
            {
                return info;
            }
        }

        return null;
    }

    public List<InterceptorServiceInfo> GetServicesByInterceptor(string interceptorName)
    {
        var allServices = GetRegisteredServices();
        return allServices
            .Where(s => s.Interceptors.Any(i => i.Contains(interceptorName, StringComparison.OrdinalIgnoreCase)))
            .ToList();
    }

    public List<string> GetAllInterceptorTypes()
    {
        var allServices = GetRegisteredServices();
        return allServices
            .SelectMany(s => s.Interceptors)
            .Distinct()
            .OrderBy(i => i)
            .ToList();
    }

    public InterceptorStats GetStats()
    {
        var services = GetRegisteredServices();

        return new InterceptorStats
        {
            TotalServices = services.Count,
            ByLifetime = services.GroupBy(s => s.Lifetime).ToDictionary(g => g.Key, g => g.Count()),
            ByInterceptor = services
                .SelectMany(s => s.Interceptors)
                .GroupBy(i => i)
                .ToDictionary(g => g.Key, g => g.Count()),
            Services = services
        };
    }

    /// <summary>
    /// 尝试从注册信息中获取拦截器信息
    /// </summary>
    private InterceptorServiceInfo? TryGetInterceptorInfo(IComponentRegistration registration)
    {
        // 跳过没有服务的注册（比如中间件注册）
        if (registration.Services == null || !registration.Services.Any())
        {
            return null;
        }

        var service = registration.Services.FirstOrDefault() as TypedService;
        if (service == null)
        {
            return null;
        }

        var serviceType = service.ServiceType;
        var implementationType = GetImplementationType(registration);

        // 检查是否有拦截器
        var hasInterfaceInterceptors = HasInterfaceInterceptors(registration);
        var hasClassInterceptors = HasClassInterceptors(registration);
        var interceptors = GetInterceptors(registration);

        // 只返回启用了拦截器的服务
        if (!hasInterfaceInterceptors && !hasClassInterceptors && interceptors.Count == 0)
        {
            return null;
        }

        return new InterceptorServiceInfo
        {
            ServiceName = serviceType.Name,
            ServiceType = serviceType.FullName ?? serviceType.Name,
            ImplementationType = implementationType?.FullName ?? "Unknown",
            Lifetime = GetLifetime(registration),
            HasInterfaceInterceptors = hasInterfaceInterceptors,
            HasClassInterceptors = hasClassInterceptors,
            Interceptors = interceptors
        };
    }

    /// <summary>
    /// 获取实现类型
    /// </summary>
    private Type? GetImplementationType(IComponentRegistration registration)
    {
        // 如果有 Target，说明是装饰器或中间服务，获取 Target 的实现类型
        if (registration.Target != null)
        {
            return GetImplementationTypeFromActivator(registration.Target)
                ?? GetImplementationTypeFromActivator(registration);
        }

        return GetImplementationTypeFromActivator(registration);
    }

    private Type? GetImplementationTypeFromActivator(IComponentRegistration registration)
    {
        if (registration.Activator is ReflectionActivator reflectorActivator)
        {
            // 通过反射获取 LimitType 属性
            var limitTypeProperty = reflectorActivator.GetType().GetProperty("LimitType");
            if (limitTypeProperty != null)
            {
                var limitType = limitTypeProperty.GetValue(reflectorActivator) as Type;
                return limitType;
            }
        }

        return null;
    }

    /// <summary>
    /// 检查是否启用了接口拦截器
    /// </summary>
    private bool HasInterfaceInterceptors(IComponentRegistration registration)
    {
        // 方法1: 检查元数据中是否有拦截器
        if (registration.Metadata.ContainsKey("Autofac.Extras.DynamicProxy.Interceptors"))
        {
            var service = registration.Services.FirstOrDefault() as TypedService;
            // 如果服务类型是接口，说明是接口拦截器
            return service?.ServiceType.IsInterface == true;
        }

        return false;
    }

    /// <summary>
    /// 检查是否启用了类拦截器
    /// </summary>
    private bool HasClassInterceptors(IComponentRegistration registration)
    {
        // 方法1: 检查元数据中是否有拦截器
        if (registration.Metadata.ContainsKey("Autofac.Extras.DynamicProxy.Interceptors"))
        {
            var service = registration.Services.FirstOrDefault() as TypedService;
            // 如果服务类型是类（非接口），说明是类拦截器
            return service?.ServiceType.IsClass == true && service.ServiceType.IsClass && !service.ServiceType.IsInterface;
        }

        return false;
    }

    /// <summary>
    /// 获取拦截器列表
    /// </summary>
    private List<string> GetInterceptors(IComponentRegistration registration)
    {
        var interceptors = new List<string>();

        // Autofac.Extras.DynamicProxy 将拦截器信息存储在元数据中
        // 键为 "Autofac.Extras.DynamicProxy.RegistrationExtensions.InterceptorsPropertyName"
        const string interceptorMetadataKey = "Autofac.Extras.DynamicProxy.RegistrationExtensions.InterceptorsPropertyName";

        if (registration.Metadata.TryGetValue(interceptorMetadataKey, out var interceptorMetadata))
        {
            // 拦截器信息存储为 TypedService[] 数组
            if (interceptorMetadata is TypedService[] typedServices)
            {
                interceptors.AddRange(typedServices.Select(ts => ts.ServiceType.FullName ?? ts.ServiceType.Name));
            }
            else if (interceptorMetadata is IEnumerable<object> objects)
            {
                foreach (var obj in objects)
                {
                    if (obj is TypedService ts)
                    {
                        interceptors.Add(ts.ServiceType.FullName ?? ts.ServiceType.Name);
                    }
                }
            }
        }

        return interceptors.Distinct().ToList();
    }

    /// <summary>
    /// 获取生命周期名称
    /// </summary>
    private string GetLifetime(IComponentRegistration registration)
    {
        // 检查实例生命周期
        // Autofac 9.x 中的生命周期类型
        var lifetimeType = registration.Lifetime.GetType();
        var lifetimeName = lifetimeType.Name ?? lifetimeType.FullName ?? "";

        if (lifetimeName.Contains("Singleton") || lifetimeName.Contains("RootScope"))
        {
            return "Singleton";
        }
        else if (lifetimeName.Contains("LifetimeScope") || lifetimeName.Contains("CurrentScope"))
        {
            return "Scoped";
        }
        else
        {
            return "Transient";
        }
    }

    /// <summary>
    /// 判断是否是项目自定义的服务（过滤掉 Autofac 自己注册的服务）
    /// </summary>
    private bool IsProjectService(InterceptorServiceInfo info)
    {
        // 定义项目的命名空间前缀
        var projectNamespacePrefixes = new[] { "KH.WMS." };

        // 检查服务类型是否在项目命名空间下
        if (projectNamespacePrefixes.Any(prefix => info.ServiceType.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
        {
            // 排除一些特殊的框架服务
            if (IsFrameworkService(info.ServiceType) || IsFrameworkService(info.ImplementationType))
            {
                return false;
            }
            return true;
        }

        return false;
    }

    /// <summary>
    /// 判断是否是框架服务（需要排除的服务）
    /// </summary>
    private bool IsFrameworkService(string serviceTypeName)
    {
        // 排除一些框架级别的服务
        var excludePatterns = new[]
        {
            "Microsoft.",
            "Autofac.",
            "System.",
            "Castle.",
            "NetCore.",
            "KH.WMS.Core.DependencyInjection.IInterceptorQueryService",  // 排除查询服务自己
            "KH.WMS.Core.DependencyInjection.InterceptorQueryService"
        };

        return excludePatterns.Any(pattern => serviceTypeName.StartsWith(pattern, StringComparison.OrdinalIgnoreCase));
    }
}
