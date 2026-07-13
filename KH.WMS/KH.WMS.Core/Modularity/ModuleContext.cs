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
    /// 模块上下文实现
    /// </summary>
    public class ModuleContext : IModuleContext
    {
        public IServiceProvider ServiceProvider { get; }
        public IConfiguration Configuration { get; }
        public ILoggerFactory LoggerFactory { get; }
        public IServiceCollection Services { get; }

        public ModuleContext(
            IServiceProvider serviceProvider,
            IConfiguration configuration,
            ILoggerFactory loggerFactory,
            IServiceCollection services)
        {
            ServiceProvider = serviceProvider;
            Configuration = configuration;
            LoggerFactory = loggerFactory;
            Services = services;
        }

        public void RegisterService<TService, TImplementation>(ServiceLifetime lifetime = ServiceLifetime.Transient)
            where TService : class
            where TImplementation : class, TService
        {
            Services.Add(new ServiceDescriptor(typeof(TService), typeof(TImplementation), lifetime));
        }

        public void RegisterService<TService>(TService instance, ServiceLifetime lifetime = ServiceLifetime.Singleton)
        {
            Services.Add(new ServiceDescriptor(typeof(TService), instance));
        }

        public void RegisterSingleton<TService, TImplementation>()
            where TService : class
            where TImplementation : class, TService
        {
            Services.AddSingleton<TService, TImplementation>();
        }

        public void RegisterScoped<TService, TImplementation>()
            where TService : class
            where TImplementation : class, TService
        {
            Services.AddScoped<TService, TImplementation>();
        }

        public void RegisterTransient<TService, TImplementation>()
            where TService : class
            where TImplementation : class, TService
        {
            Services.AddTransient<TService, TImplementation>();
        }
    }

}
