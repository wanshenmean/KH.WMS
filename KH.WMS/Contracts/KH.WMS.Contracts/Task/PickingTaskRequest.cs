namespace KH.WMS.Contracts.Tasks;

/// <summary>
/// 创建拣货任务请求
/// </summary>
public class PickingTaskRequest
{
    public long WarehouseId { get; set; }
    public long? DocId { get; set; }
    public string? DocNo { get; set; }
    public string? DocType { get; set; }
    public long? ContainerId { get; set; }
    public string? ContainerNo { get; set; }
    public long? FromLocationId { get; set; }
    public string? FromLocationCode { get; set; }
    public long? FromZoneId { get; set; }
    public long? ToLocationId { get; set; }
    public string? ToLocationCode { get; set; }
    public long? ToZoneId { get; set; }
    public List<PickingTaskLineRequest> Lines { get; set; } = [];
}

/// <summary>
/// 拣货任务行请求
/// </summary>
public class PickingTaskLineRequest
{
    public long MaterialId { get; set; }
    public string? MaterialCode { get; set; }
    public string? MaterialName { get; set; }
    public string? BatchNo { get; set; }
    public long? InventoryHeaderId { get; set; }
}
