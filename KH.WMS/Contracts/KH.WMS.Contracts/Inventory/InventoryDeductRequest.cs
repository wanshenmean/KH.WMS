using KH.WMS.Entities.Constants;

namespace KH.WMS.Contracts.Inventory;

/// <summary>
/// 库存扣减请求
/// </summary>
public class InventoryDeductRequest
{
    /// <summary>
    /// 库存明细ID
    /// </summary>
    public long InventoryDetailId { get; init; }

    /// <summary>
    /// 扣减数量
    /// </summary>
    public decimal DeductQty { get; init; }

    /// <summary>
    /// 仓库ID
    /// </summary>
    public long WarehouseId { get; init; }

    /// <summary>
    /// 单据类型
    /// </summary>
    public string DocType { get; init; } = string.Empty;

    /// <summary>
    /// 单据编号
    /// </summary>
    public string DocNo { get; init; } = string.Empty;

    /// <summary>
    /// 移动类型（OUTBOUND/ADJUST 等）
    /// </summary>
    public string MovementType { get; init; } = BizConstants.MovementTypes.OUTBOUND;
}
