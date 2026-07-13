namespace KH.WMS.Contracts.Inbound;

/// <summary>
/// 入库单跨模块接口契约
/// 供 Inventory/Task 等模块查询入库单状态和数据
/// 各模块按需引用此接口，InboundModule 负责实现
/// </summary>
public interface IInboundOrderContract
{
    /// <summary>
    /// 根据入库单ID获取单据状态
    /// </summary>
    Task<string?> GetOrderStatusAsync(long orderId);

    /// <summary>
    /// 根据入库单号获取单据ID
    /// </summary>
    Task<long?> GetOrderIdByOrderNoAsync(string orderNo);

    /// <summary>
    /// 检查入库单是否已完成收货
    /// </summary>
    Task<bool> IsReceivedAsync(long orderId);

    /// <summary>
    /// 获取入库单关联的仓库ID
    /// </summary>
    Task<long?> GetWarehouseIdAsync(long orderId);

    /// <summary>
    /// 将组盘头状态标记为上架中（PUTTING_AWAY）
    /// </summary>
    Task<bool> MarkBindAsPuttingAwayAsync(long bindHeaderId);

    /// <summary>
    /// 将组盘头状态标记为已上架（PUT_AWAY）
    /// </summary>
    Task<bool> MarkBindAsPutAwayAsync(long bindHeaderId);

    /// <summary>
    /// 获取组盘头的来源单据号
    /// </summary>
    Task<string?> GetBindSourceDocNoAsync(long bindHeaderId);

    /// <summary>
    /// 获取组盘明细列表
    /// </summary>
    Task<List<BindDetailData>?> GetBindDetailsAsync(long bindHeaderId);

    /// <summary>
    /// 将组盘头状态从上架中（PUTTING_AWAY）回退为已组盘（BOUND）
    /// 用于上架任务取消时回滚组盘状态
    /// </summary>
    Task<bool> RevertBindToBoundAsync(long bindHeaderId);
}
