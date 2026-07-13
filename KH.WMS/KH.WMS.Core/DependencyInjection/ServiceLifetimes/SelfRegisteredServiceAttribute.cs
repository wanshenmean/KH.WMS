using Microsoft.Extensions.DependencyInjection;

namespace KH.WMS.Core.DependencyInjection.ServiceLifetimes;

/// <summary>
/// 不带接口层自注册服务标记 - 服务自身作为实现类型
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class SelfRegisteredServiceAttribute : Attribute
{
    /// <summary>
    /// 服务生命周期
    /// </summary>
    public ServiceLifetime Lifetime { get; set; } = ServiceLifetime.Scoped;

    public bool WithoutInterceptor { get; set; } = false;

}


