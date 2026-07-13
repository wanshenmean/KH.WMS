using Microsoft.Extensions.DependencyInjection;

namespace KH.WMS.Core.DependencyInjection;

/// <summary>
/// 拦截器查询服务接口
/// </summary>
public interface IInterceptorQueryService
{
    /// <summary>
    /// 获取所有已注册拦截器的服务列表
    /// </summary>
    List<InterceptorServiceInfo> GetRegisteredServices();

    /// <summary>
    /// 根据服务名称获取拦截器信息
    /// </summary>
    InterceptorServiceInfo? GetServiceInfo(string serviceName);

    /// <summary>
    /// 获取指定拦截器应用的所有服务
    /// </summary>
    List<InterceptorServiceInfo> GetServicesByInterceptor(string interceptorName);

    /// <summary>
    /// 获取所有拦截器类型
    /// </summary>
    List<string> GetAllInterceptorTypes();

    /// <summary>
    /// 获取统计信息
    /// </summary>
    InterceptorStats GetStats();
}

/// <summary>
/// 拦截器服务信息
/// </summary>
public class InterceptorServiceInfo
{
    /// <summary>
    /// 服务名称
    /// </summary>
    public string ServiceName { get; set; } = string.Empty;

    /// <summary>
    /// 服务类型（接口或类）
    /// </summary>
    public string ServiceType { get; set; } = string.Empty;

    /// <summary>
    /// 实现类型
    /// </summary>
    public string ImplementationType { get; set; } = string.Empty;

    /// <summary>
    /// 生命周期
    /// </summary>
    public string Lifetime { get; set; } = string.Empty;

    /// <summary>
    /// 是否启用了接口拦截器
    /// </summary>
    public bool HasInterfaceInterceptors { get; set; }

    /// <summary>
    /// 是否启用了类拦截器
    /// </summary>
    public bool HasClassInterceptors { get; set; }

    /// <summary>
    /// 应用的拦截器列表
    /// </summary>
    public List<string> Interceptors { get; set; } = new();
}

/// <summary>
/// 拦截器统计信息
/// </summary>
public class InterceptorStats
{
    public int TotalServices { get; set; }
    public Dictionary<string, int> ByLifetime { get; set; } = new();
    public Dictionary<string, int> ByInterceptor { get; set; } = new();
    public List<InterceptorServiceInfo> Services { get; set; } = new();
}
