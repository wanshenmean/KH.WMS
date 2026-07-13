namespace KH.WMS.Contracts.Inventory;

/// <summary>
/// 库存货位移动请求（下架完成时使用）
/// </summary>
public class InventoryMoveLocationRequest
{
    /// <summary>
    /// 容器编号
    /// </summary>
    public string ContainerCode { get; init; } = string.Empty;

    /// <summary>
    /// 仓库ID
    /// </summary>
    public long WarehouseId { get; init; }

    /// <summary>
    /// 原货位ID
    /// </summary>
    public long FromLocationId { get; init; }

    /// <summary>
    /// 原货位编码
    /// </summary>
    public string FromLocationCode { get; init; } = string.Empty;

    /// <summary>
    /// 目标货位ID
    /// </summary>
    public long ToLocationId { get; init; }

    /// <summary>
    /// 目标货位编码
    /// </summary>
    public string ToLocationCode { get; init; } = string.Empty;

    /// <summary>
    /// 关联单据类型（如 PICKING、REPLENISHMENT）
    /// </summary>
    public string DocType { get; init; } = string.Empty;

    /// <summary>
    /// 关联单据编号（如任务编号）
    /// </summary>
    public string DocNo { get; init; } = string.Empty;
}
