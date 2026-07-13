namespace KH.WMS.Contracts.Inventory;

/// <summary>
/// 库存跨模块接口契约
/// 供 Inbound/Outbound 等模块查询和操作库存
/// InventoryModule 负责实现
/// </summary>
public interface IInventoryContract
{
    /// <summary>
    /// 检查容器是否已有库存记录
    /// </summary>
    Task<bool> ContainerHasInventoryAsync(string containerCode);

    /// <summary>
    /// 获取容器当前库存数量
    /// </summary>
    Task<decimal> GetContainerQtyAsync(string containerCode);

    /// <summary>
    /// 检查货位是否可用（状态允许上架）
    /// </summary>
    Task<bool> IsLocationAvailableAsync(long locationId);

    /// <summary>
    /// 根据上架任务完成信息生成库存（库存头 + 库存明细）
    /// </summary>
    /// <returns>库存头ID</returns>
    Task<long> GenerateInventoryFromPutawayAsync(InventoryGenerationRequest request);

    /// <summary>
    /// 扣减库存明细的 Qty 和 LockedQty，并记录库存流水
    /// </summary>
    /// <returns>扣减后的库存数量，失败返回 null</returns>
    Task<decimal?> DeductInventoryAsync(InventoryDeductRequest request);

    /// <summary>
    /// 锁定库存（增加 LockedQty），用于出库分配、盘点冻结等
    /// </summary>
    /// <returns>锁定后的 LockedQty，失败返回 null</returns>
    Task<decimal?> LockInventoryAsync(long inventoryDetailId, decimal lockQty);

    /// <summary>
    /// 解锁库存（减少 LockedQty），用于出库取消、分配回滚、盘点解冻等。
    /// 解锁量不会超过当前 LockedQty（下限为 0）。
    /// </summary>
    /// <returns>解锁后的 LockedQty，失败返回 null</returns>
    Task<decimal?> UnlockInventoryAsync(long inventoryDetailId, decimal unlockQty);

    /// <summary>
    /// 移动库存头到新货位（下架完成时使用：将库存从存储货位移到拣货/暂存货位）
    /// 同时记录库存流水
    /// </summary>
    Task MoveInventoryLocationAsync(InventoryMoveLocationRequest request);
}
