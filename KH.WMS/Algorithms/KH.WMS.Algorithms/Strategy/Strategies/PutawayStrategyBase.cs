using KH.WMS.Algorithms.Strategy.Constants;
using KH.WMS.Algorithms.Strategy.Enums;
using KH.WMS.Algorithms.Strategy.Interfaces;

namespace KH.WMS.Algorithms.Strategy.Strategies
{
    /// <summary>
    /// 上架策略基类
    /// 控制物料从入库口到目标货位的整个上架流程
    /// 职责：确定上架巷道 → 确定目标库区 → 调度货位分配策略 → 生成上架路径
    /// </summary>
    public abstract class PutawayStrategyBase : PolicyStrategyBase
    {
        public sealed override PolicyType PolicyType => PolicyType.Putaway;

        protected abstract Task<PutawayResult> ExecutePutawayAsync(IPolicyContext context, CancellationToken cancellationToken = default);

        public sealed override async Task<IPolicyResult> ExecuteAsync(IPolicyContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await ExecutePutawayAsync(context, cancellationToken);
                if (result == null)
                    return PolicyResult.Skipped("上架策略未产生结果");

                context.SetData(StrategyParams.PutawayOutput.RESULT, result);
                return PolicyResult.Success(result);
            }
            catch (Exception ex)
            {
                return PolicyResult.Failure($"上架策略执行失败: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// 上架策略执行结果
    /// </summary>
    public class PutawayResult
    {
        /// <summary>入库口ID（物料从哪个入口进入）</summary>
        public string? InboundStationId { get; set; }

        /// <summary>入库口编码</summary>
        public string? InboundStationCode { get; set; }

        /// <summary>分配的巷道ID</summary>
        public long? AisleId { get; set; }

        /// <summary>分配的巷道编码</summary>
        public string? AisleCode { get; set; }

        /// <summary>目标库区ID</summary>
        public long? TargetZoneId { get; set; }

        /// <summary>目标库区编码</summary>
        public string? TargetZoneCode { get; set; }

        /// <summary>上架路径（从入库口到目标货位的巷道-货位序列）</summary>
        public List<string> Route { get; set; } = new();

        /// <summary>路径优化方式（S_SHAPE/Z_SHAPE/U_SHAPE）</summary>
        public string? PathOptimization { get; set; }

        /// <summary>是否需要调用货位分配策略进一步计算具体货位</summary>
        public bool RequireLocationAllocation { get; set; } = true;

        /// <summary>传递给下游货位分配策略的参数</summary>
        public Dictionary<string, object?> AllocationParameters { get; set; } = new();
    }
}
