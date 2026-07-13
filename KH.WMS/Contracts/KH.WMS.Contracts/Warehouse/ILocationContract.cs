namespace KH.WMS.Contracts.Warehouse;

/// <summary>
/// 库位跨模块接口契约
/// 供 Inbound/Task 等模块更新库位状态
/// WarehouseModule 负责实现
/// </summary>
public interface ILocationContract
{
    /// <summary>
    /// 更新库位物理状态（EMPTY / OCCUPIED）
    /// </summary>
    Task UpdateLocationStatusAsync(long locationId, string status);

    /// <summary>
    /// 更新库位锁定状态（NONE / INBOUND_RESERVED / OUTBOUND_LOCKED / STOCKTAKE_FREEZE）
    /// </summary>
    Task UpdateLocationLockStatusAsync(long locationId, byte lockStatus);
}
