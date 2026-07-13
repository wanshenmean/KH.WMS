namespace KH.WMS.Contracts.Outbound;

/// <summary>
/// 拣货任务完成事件
/// </summary>
public class PickingCompletedEvent
{
    /// <summary>
    /// 出库单ID
    /// </summary>
    public long OutboundOrderId { get; init; }

    /// <summary>
    /// 任务号
    /// </summary>
    public string TaskNo { get; init; } = string.Empty;

    /// <summary>
    /// 容器编号
    /// </summary>
    public string ContainerCode { get; init; } = string.Empty;

    /// <summary>
    /// 源库位ID
    /// </summary>
    public long FromLocationId { get; init; }
}
