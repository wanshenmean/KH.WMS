using KH.WMS.Entities.Constants;

namespace KH.WMS.Contracts.Events;

/// <summary>
/// 组盘完成事件
/// 组盘完成后发布，供上架模块订阅处理
/// </summary>
public class ContainerBindEvent
{
    /// <summary>
    /// 入库单ID
    /// </summary>
    public long? InboundOrderId { get; set; }

    /// <summary>
    /// 入库单号
    /// </summary>
    public string? OrderNo { get; set; }

    /// <summary>
    /// 涉及的容器编号列表
    /// </summary>
    public List<string> ContainerCodes { get; set; } = new();

    /// <summary>
    /// 仓库ID
    /// </summary>
    public long? WarehouseId { get; set; }

    /// <summary>
    /// 事件时间
    /// </summary>
    public DateTime EventTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 来源类型
    /// </summary>
    public string SourceType { get; set; } = BizConstants.SourceTypes.INBOUND;
}
