namespace KH.WMS.Contracts.Events;

/// <summary>
/// 入库收货完成事件
/// 收货完成后发布，供库存模块、任务模块等订阅处理
/// </summary>
public class InboundCompletedEvent
{
    /// <summary>
    /// 入库单ID
    /// </summary>
    public long InboundOrderId { get; set; }

    /// <summary>
    /// 入库单号
    /// </summary>
    public string OrderNo { get; set; } = string.Empty;

    /// <summary>
    /// 仓库ID
    /// </summary>
    public long? WarehouseId { get; set; }

    /// <summary>
    /// 事件时间
    /// </summary>
    public DateTime EventTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 完成的明细行ID列表
    /// </summary>
    public List<long> CompletedLineIds { get; set; } = new();
}
