namespace KH.WMS.Contracts.Tasks;

/// <summary>
/// 创建上架任务请求
/// </summary>
public class PutawayTaskRequest
{
    public long WarehouseId { get; set; }
    public long? DocId { get; set; }
    public string? DocNo { get; set; }
    public string? DocType { get; set; }
    public string? ContainerNo { get; set; }
    public string? FromLocationCode { get; set; }
    public long? ToLocationId { get; set; }
    public string? ToLocationCode { get; set; }
    public long? ToZoneId { get; set; }
    public List<PutawayTaskLineRequest> Lines { get; set; } = [];
}

/// <summary>
/// 上架任务行请求
/// </summary>
public class PutawayTaskLineRequest
{
    public long MaterialId { get; set; }
    public string? MaterialCode { get; set; }
    public string? MaterialName { get; set; }
    public string? BatchNo { get; set; }
}
