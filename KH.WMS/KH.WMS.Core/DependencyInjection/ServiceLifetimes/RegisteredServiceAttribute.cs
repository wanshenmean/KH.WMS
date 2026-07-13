using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace KH.WMS.Core.DependencyInjection.ServiceLifetimes
{
    /// <summary>
    /// 带接口层自注册服务标记 - 用于标记需要自动注册到依赖注入容器的服务
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class RegisteredServiceAttribute : Attribute
    {
        /// <summary>
        /// 服务生命周期
        /// </summary>
        public ServiceLifetime Lifetime { get; set; } = ServiceLifetime.Scoped;

        public bool WithoutInterceptor { get; set; } = false;

        /// <summary>
        /// 
        /// </summary>
        public Type? ServiceType { get; set; } = null;
    }
}
