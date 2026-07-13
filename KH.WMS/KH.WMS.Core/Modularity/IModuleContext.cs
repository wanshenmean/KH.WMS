using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace KH.WMS.Core.Modularity
{

    /// <summary>
    /// 模块上下文
    /// </summary>
    public interface IModuleContext
    {
        IServiceProvider ServiceProvider { get; }
        IConfiguration Configuration { get; }
        ILoggerFactory LoggerFactory { get; }
        IServiceCollection Services { get; }

        void RegisterService<TService, TImplementation>(ServiceLifetime lifetime = ServiceLifetime.Transient)
            where TService : class
            where TImplementation : class, TService;

        void RegisterService<TService>(TService instance, ServiceLifetime lifetime = ServiceLifetime.Singleton);

        void RegisterSingleton<TService, TImplementation>()
            where TService : class
            where TImplementation : class, TService;

        void RegisterScoped<TService, TImplementation>()
            where TService : class
            where TImplementation : class, TService;

        void RegisterTransient<TService, TImplementation>()
            where TService : class
            where TImplementation : class, TService;
    }

}
