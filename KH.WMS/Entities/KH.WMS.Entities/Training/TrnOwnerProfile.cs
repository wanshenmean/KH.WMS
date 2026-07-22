using KH.WMS.Core.Models.Entities;
using SqlSugar;

namespace KH.WMS.Entities.Training;

[SugarTable("trn_owner_profile")]
[SugarIndex("uk_trn_owner_profile_code", nameof(OwnerCode), OrderByType.Asc, true)]
public class TrnOwnerProfile : BaseEntity<long>
{
    [SugarColumn(Length = 30, IsNullable = false)]
    public string OwnerCode { get; set; } = string.Empty;

    [SugarColumn(Length = 100, IsNullable = false)]
    public string OwnerName { get; set; } = string.Empty;

    [SugarColumn(Length = 50, IsNullable = true)]
    public string? ContactName { get; set; }

    [SugarColumn(Length = 20, IsNullable = true)]
    public string? ContactPhone { get; set; }

    [SugarColumn(Length = 500, IsNullable = true)]
    public string? Address { get; set; }

    [SugarColumn(Length = 500, IsNullable = true)]
    public string? Remark { get; set; }

    [SugarColumn(ColumnDataType = "nvarchar(max)", IsNullable = true)]
    public string? ExtData { get; set; }
}
