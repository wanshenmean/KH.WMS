using KH.WMS.Algorithms.Strategy.Interfaces;

namespace KH.WMS.Algorithms.Strategy.Strategies
{
    /// <summary>
    /// 同步策略基类（简化开发）
    /// </summary>
    public abstract class SyncPolicyStrategyBase : PolicyStrategyBase
    {
        public sealed override async Task<IPolicyResult> ExecuteAsync(IPolicyContext context, CancellationToken cancellationToken = default)
        {
            return await Task.FromResult(ExecuteSync(context));
        }

        protected abstract IPolicyResult ExecuteSync(IPolicyContext context);
    }
}
