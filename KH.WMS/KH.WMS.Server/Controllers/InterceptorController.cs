using Autofac;
using Autofac.Core;
using KH.WMS.Core.DependencyInjection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KH.WMS.Server.Controllers;

[Route("api/[controller]")]
[ApiController]
[AllowAnonymous]
public class InterceptorController : ControllerBase
{
    private readonly IInterceptorQueryService _interceptorQueryService;
    private readonly ILifetimeScope _lifetimeScope;

    public InterceptorController(IInterceptorQueryService interceptorQueryService, ILifetimeScope lifetimeScope)
    {
        _interceptorQueryService = interceptorQueryService;
        _lifetimeScope = lifetimeScope;
    }

    /// <summary>
    /// 调试接口：获取所有注册的原始数据
    /// </summary>
    [HttpGet("debug")]
    public IActionResult GetDebugInfo()
    {
        var registrations = _lifetimeScope.ComponentRegistry.Registrations.ToList();
        var debugInfo = new List<object>();

        foreach (var registration in registrations)
        {
            var service = registration.Services.FirstOrDefault() as TypedService;
            if (service == null) continue;

            var info = new
            {
                ServiceType = service.ServiceType.FullName ?? service.ServiceType.Name,
                ImplementationType = GetImplementationTypeName(registration),
                MetadataKeys = registration.Metadata.Keys.ToList(),
                Metadata = registration.Metadata.ToDictionary(kvp => kvp.Key, kvp => kvp.Value?.ToString() ?? "null"),
                Lifetime = registration.Lifetime.GetType().Name
            };

            debugInfo.Add(info);
        }

        return Ok(new
        {
            total = debugInfo.Count,
            data = debugInfo
        });
    }

    private string? GetImplementationTypeName(IComponentRegistration registration)
    {
        if (registration.Target != null)
        {
            var targetService = registration.Target.Services.FirstOrDefault() as TypedService;
            if (targetService != null)
            {
                return targetService.ServiceType.FullName ?? targetService.ServiceType.Name;
            }
        }

        var currentService = registration.Services.FirstOrDefault() as TypedService;
        return currentService?.ServiceType.FullName ?? currentService?.ServiceType.Name;
    }

    /// <summary>
    /// 获取所有已注册拦截器的服务列表
    /// </summary>
    /// <returns></returns>
    [HttpGet("services")]
    public IActionResult GetRegisteredServices()
    {
        var services = _interceptorQueryService.GetRegisteredServices();
        return Ok(new
        {
            total = services.Count,
            data = services
        });
    }

    /// <summary>
    /// 根据服务名称获取拦截器信息
    /// </summary>
    /// <param name="serviceName"></param>
    /// <returns></returns>
    [HttpGet("service/{serviceName}")]
    public IActionResult GetServiceInfo(string serviceName)
    {
        var info = _interceptorQueryService.GetServiceInfo(serviceName);
        if (info == null)
        {
            return NotFound(new { message = $"Service '{serviceName}' not found or has no interceptors." });
        }
        return Ok(info);
    }

    /// <summary>
    /// 获取指定拦截器应用的所有服务
    /// </summary>
    /// <param name="interceptorName"></param>
    /// <returns></returns>
    [HttpGet("by-interceptor/{interceptorName}")]
    public IActionResult GetServicesByInterceptor(string interceptorName)
    {
        var services = _interceptorQueryService.GetServicesByInterceptor(interceptorName);
        return Ok(new
        {
            interceptor = interceptorName,
            total = services.Count,
            data = services
        });
    }

    /// <summary>
    /// 获取所有拦截器类型
    /// </summary>
    /// <returns></returns>
    [HttpGet("interceptors")]
    public IActionResult GetAllInterceptors()
    {
        var interceptors = _interceptorQueryService.GetAllInterceptorTypes();
        return Ok(new
        {
            total = interceptors.Count,
            data = interceptors
        });
    }

    /// <summary>
    /// 获取拦截器统计信息
    /// </summary>
    /// <returns></returns>
    [HttpGet("stats")]
    public IActionResult GetStats()
    {
        var stats = _interceptorQueryService.GetStats();
        return Ok(stats);
    }
}
