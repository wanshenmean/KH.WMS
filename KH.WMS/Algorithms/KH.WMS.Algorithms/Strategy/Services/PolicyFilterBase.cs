using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KH.WMS.Algorithms.Strategy.Interfaces;

namespace KH.WMS.Algorithms.Strategy.Services
{
    /// <summary>
    /// 策略过滤器基类
    /// </summary>
    public abstract class PolicyFilterBase : IPolicyFilter
    {
        public abstract int Order { get; }
        public abstract string Name { get; }

        public virtual Task<IPolicyContext> OnBeforeExecutionAsync(IPolicyContext context, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(context);
        }

        public virtual Task<IPolicyResult> OnAfterExecutionAsync(IPolicyContext context, IPolicyResult result, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(result);
        }
    }
}
