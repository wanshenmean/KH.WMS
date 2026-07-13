namespace KH.WMS.Contracts.Inbound;

/// <summary>
/// 组盘明细数据（跨模块传输用）
/// </summary>
public class BindDetailData
{
    public long MaterialId { get; set; }
    public string? MaterialCode { get; set; }
    public decimal Qty { get; set; }
    public string? BatchNo { get; set; }
    public DateOnly? ProductionDate { get; set; }
    public DateOnly? ExpiryDate { get; set; }
}
