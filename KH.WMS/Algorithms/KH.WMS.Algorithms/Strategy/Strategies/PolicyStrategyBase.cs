using KH.WMS.Algorithms.Strategy.Enums;
using KH.WMS.Algorithms.Strategy.Interfaces;

namespace KH.WMS.Algorithms.Strategy.Strategies
{
    /// <summary>
    /// 策略基类
    /// </summary>
    public abstract class PolicyStrategyBase : IPolicyStrategy
    {
        public abstract string Name { get; }
        public abstract string Code { get; }
        public virtual int Priority => 0;
        public abstract PolicyType PolicyType { get; }
        public virtual bool IsEnabled => true;
        public virtual IEnumerable<long>? ApplicableWarehouseIds => null;
        public virtual IEnumerable<long>? ApplicableZoneIds => null;
        public virtual IEnumerable<long>? ApplicableMaterialIds => null;
        public virtual IEnumerable<string>? ApplicableDocTypes => null;

        public abstract string Author { get; }
        public abstract string Description { get; }

        /// <summary>
        /// 判断策略是否适用
        /// 检查维度：仓库、库区、物料、单据类型，任一配置了限制则必须匹配
        /// </summary>
        public virtual Task<bool> IsApplicableAsync(IPolicyContext context, CancellationToken cancellationToken = default)
        {
            if (ApplicableWarehouseIds != null && ApplicableWarehouseIds.Any())
            {
                if (!context.WarehouseId.HasValue || !ApplicableWarehouseIds.Contains(context.WarehouseId.Value))
                    return Task.FromResult(false);
            }

            if (ApplicableZoneIds != null && ApplicableZoneIds.Any())
            {
                if (!context.ZoneId.HasValue || !ApplicableZoneIds.Contains(context.ZoneId.Value))
                    return Task.FromResult(false);
            }

            if (ApplicableMaterialIds != null && ApplicableMaterialIds.Any())
            {
                if (!context.MaterialId.HasValue || !ApplicableMaterialIds.Contains(context.MaterialId.Value))
                    return Task.FromResult(false);
            }

            if (ApplicableDocTypes != null && ApplicableDocTypes.Any())
            {
                if (string.IsNullOrWhiteSpace(context.DocType) || !ApplicableDocTypes.Contains(context.DocType))
                    return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }

        public abstract Task<IPolicyResult> ExecuteAsync(IPolicyContext context, CancellationToken cancellationToken = default);
    }
}
