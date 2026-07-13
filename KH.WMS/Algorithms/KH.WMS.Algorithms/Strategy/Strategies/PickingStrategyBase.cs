using KH.WMS.Algorithms.Strategy.Constants;
using KH.WMS.Algorithms.Strategy.Enums;
using KH.WMS.Algorithms.Strategy.Interfaces;

namespace KH.WMS.Algorithms.Strategy.Strategies
{
    /// <summary>
    /// 下架策略基类（代码中 Picking 即"下架"）
    /// 控制物料从货位移出到目标出口/站台的整个下架流程
    /// 职责：调度库存分配策略（选哪些库存）→ 确定取货路径 → 确定出到哪个出口/站台
    /// 注意：本策略仅负责"堆垛机/设备从货位取货到出库口"的路径与出口决策；
    ///       人工拣货确认、数量录入、异常上报等由 PDA 业务层处理，不在策略引擎范围内。
    /// </summary>
    public abstract class PickingStrategyBase : PolicyStrategyBase
    {
        public sealed override PolicyType PolicyType => PolicyType.Picking;

        protected abstract Task<PickingResult> ExecutePickingAsync(IPolicyContext context, CancellationToken cancellationToken = default);

        public sealed override async Task<IPolicyResult> ExecuteAsync(IPolicyContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await ExecutePickingAsync(context, cancellationToken);
                if (result == null)
                    return PolicyResult.Skipped("下架策略未产生结果");

                context.SetData(StrategyParams.PickingOutput.RESULT, result);
                return PolicyResult.Success(result);
            }
            catch (Exception ex)
            {
                return PolicyResult.Failure($"下架策略执行失败: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// 下架策略执行结果
    /// </summary>
    public class PickingResult
    {
        /// <summary>下架任务列表（按执行优先级排序）</summary>
        public List<PickingTaskItem> Tasks { get; set; } = new();

        /// <summary>目标出口ID</summary>
        public long? DestinationStationId { get; set; }

        /// <summary>目标出口编码</summary>
        public string? DestinationStationCode { get; set; }

        /// <summary>目标区域ID（出库暂存区、出库站台等）</summary>
        public long? DestinationZoneId { get; set; }

        /// <summary>目标区域编码</summary>
        public string? DestinationZoneCode { get; set; }

        /// <summary>取货路径（库位编码列表，按堆垛机行走顺序排列）</summary>
        public List<string> PickRoute { get; set; } = new();

        /// <summary>送达路径（从最后一个取货位到目标出口的路径）</summary>
        public List<string> DeliveryRoute { get; set; } = new();

        /// <summary>路径优化方式（S_SHAPE/Z_SHAPE/U_SHAPE）</summary>
        public string? PathOptimization { get; set; }

        /// <summary>是否需要调用库存分配策略</summary>
        public bool RequireInventoryAllocation { get; set; } = true;

        /// <summary>传递给下游库存分配策略的参数</summary>
        public Dictionary<string, object?> AllocationParameters { get; set; } = new();
    }

    /// <summary>
    /// 下架任务项
    /// </summary>
    public class PickingTaskItem
    {
        /// <summary>取货货位ID</summary>
        public string FromLocationId { get; set; } = string.Empty;

        /// <summary>取货货位编码</summary>
        public string FromLocationCode { get; set; } = string.Empty;

        /// <summary>所在巷道ID</summary>
        public long? AisleId { get; set; }

        /// <summary>容器编号</summary>
        public string? ContainerCode { get; set; }

        /// <summary>物料ID</summary>
        public long MaterialId { get; set; }

        /// <summary>批次号</summary>
        public string? BatchNo { get; set; }

        /// <summary>序列号</summary>
        public string? SerialNo { get; set; }

        /// <summary>下架数量</summary>
        public decimal Qty { get; set; }

        /// <summary>执行优先级（数字越小越先执行）</summary>
        public int Priority { get; set; }

        /// <summary>是否整托出库</summary>
        public bool IsFullPallet { get; set; }

        /// <summary>库存明细ID（关联库存记录）</summary>
        public long? InventoryDetailId { get; set; }

        /// <summary>库存头ID（关联库存头记录）</summary>
        public long? InventoryHeaderId { get; set; }
    }
}
