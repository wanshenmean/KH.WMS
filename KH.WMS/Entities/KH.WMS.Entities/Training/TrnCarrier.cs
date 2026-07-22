using KH.WMS.Core.Models.Entities;
using SqlSugar;

namespace KH.WMS.Entities.Training;

[SugarTable("trn_carrier")]
[SugarIndex("uk_trn_carrier_code", nameof(CarrierCode), OrderByType.Asc, true)]
public class TrnCarrier : BaseEntity<long>, IEnableDisableEntity
{
    [SugarColumn(Length = 30, IsNullable = false)]
    public string CarrierCode { get; set; } = string.Empty;

    [SugarColumn(Length = 100, IsNullable = false)]
    public string CarrierName { get; set; } = string.Empty;

    [SugarColumn(Length = 50, IsNullable = true)]
    public string? ContactName { get; set; }

    [SugarColumn(Length = 20, IsNullable = true)]
    public string? ContactPhone { get; set; }

    [SugarColumn(Length = 20, IsNullable = true)]
    public string? TransportMode { get; set; }

    [SugarColumn(IsNullable = false, DefaultValue = "1")]
    public byte Status { get; set; } = 1;

    [SugarColumn(Length = 500, IsNullable = true)]
    public string? Remark { get; set; }
}
