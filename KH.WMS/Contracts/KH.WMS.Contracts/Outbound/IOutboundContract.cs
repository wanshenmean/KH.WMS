using KH.WMS.Core.Models;

namespace KH.WMS.Contracts.Outbound;

/// <summary>
/// 出库跨模块接口契约
/// 供 Task/Inventory 等模块在拣货任务完成时回调更新出库状态
/// OutboundModule 负责实现
/// </summary>
public interface IOutboundContract
{
    /// <summary>
    /// 拣货任务完成时回调：扣减库存、更新分配明细/分配头/出库单状态
    /// </summary>
    Task<ServiceResult> OnPickingTaskCompletedAsync(PickingCompletedEvent evt);

    /// <summary>
    /// 更新分配明细状态
    /// </summary>
    Task UpdateAllocationDetailStatusAsync(long detailId, string status, long? taskHeaderId = null);

    /// <summary>
    /// 更新分配头状态
    /// </summary>
    Task UpdateAllocationHeaderStatusAsync(long headerId, string status);

    /// <summary>
    /// 更新出库单行状态
    /// </summary>
    Task UpdateOrderLineStatusAsync(long lineId, string status);

    /// <summary>
    /// 更新出库单状态
    /// </summary>
    Task UpdateOrderStatusAsync(long orderId, string status);

    /// <summary>
    /// 检查分配头下所有明细是否全部已拣，如果是则更新分配头状态为 PICKED
    /// </summary>
    Task<bool> TryCompleteAllocationHeaderAsync(long allocationHeaderId);

    /// <summary>
    /// 检查出库单行下所有分配明细是否全部已拣，如果是则更新行状态为 PICKED
    /// </summary>
    Task<bool> TryCompleteOrderLineAsync(long orderLineId);

    /// <summary>
    /// 检查出库单所有行是否全部已拣，如果是则更新出库单状态为 COMPLETED
    /// </summary>
    Task<bool> TryCompleteOrderAsync(long outboundOrderId);

    /// <summary>
    /// PICKING任务取消时回退分配明细和分配头状态
    /// 分配明细：PICKING → ALLOCATED，清空 TaskHeaderId
    /// 分配头：如果无其他 PICKING 明细，PICKING → ALLOCATED
    /// </summary>
    Task ResetAllocationByTaskAsync(long taskHeaderId);
}
