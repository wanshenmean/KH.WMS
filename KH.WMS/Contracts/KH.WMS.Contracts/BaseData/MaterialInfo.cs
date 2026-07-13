namespace KH.WMS.Contracts.BaseData;

/// <summary>
/// 物料信息（跨模块传输用）
/// </summary>
public class MaterialInfo
{
    public long Id { get; set; }
    public string MaterialCode { get; set; } = string.Empty;
    public string MaterialName { get; set; } = string.Empty;
    public long BaseUnitId { get; set; }
}
