namespace KH.WMS.Contracts.Inventory;

/// <summary>
/// 上架完成生成库存请求
/// </summary>
public class InventoryGenerationRequest
{
    public string ContainerCode { get; set; } = string.Empty;
    public long WarehouseId { get; set; }
    public string? WarehouseCode { get; set; }
    public long? LocationId { get; set; }
    public string? LocationCode { get; set; }
    public string? SourceDocNo { get; set; }
    public List<InventoryLineRequest> Lines { get; set; } = [];
}

/// <summary>
/// 库存明细行请求
/// </summary>
public class InventoryLineRequest
{
    public long MaterialId { get; set; }
    public string MaterialCode { get; set; } = string.Empty;
    public string? BatchNo { get; set; }
    public decimal Qty { get; set; }
    public DateOnly? ProductionDate { get; set; }
    public DateOnly? ExpiryDate { get; set; }
    public string? InboundDocNo { get; set; }
}
