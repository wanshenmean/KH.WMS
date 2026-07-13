using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KH.WMS.Core.Modularity
{
    /// <summary>
    /// WMS模块基类
    /// </summary>
    public abstract class ModuleBase : IModule
    {
        public abstract Guid ModuleId { get; }
        public abstract string Name { get; }
        public abstract Version Version { get; }
        public abstract string Description { get; }
        public abstract string Author { get; }


        public virtual IEnumerable<ModuleDependency> Dependencies => Array.Empty<ModuleDependency>();

        public abstract Task InitializeAsync(IModuleContext context);

        public virtual Task StartAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public virtual Task StopAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public virtual Task ShutdownAsync()
        {
            return Task.CompletedTask;
        }
    }
}
