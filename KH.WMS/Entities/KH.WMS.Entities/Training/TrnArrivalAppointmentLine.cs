using KH.WMS.Core.Models.Entities;
using SqlSugar;

namespace KH.WMS.Entities.Training;

[SugarTable("trn_arrival_appointment_line")]
[SugarIndex("uk_trn_arrival_appointment_line_no", nameof(AppointmentId), OrderByType.Asc, nameof(LineNo), OrderByType.Asc, true)]
public class TrnArrivalAppointmentLine : BaseEntity<long>
{
    [SugarColumn(IsNullable = false)]
    public long AppointmentId { get; set; }

    [SugarColumn(IsNullable = false)]
    public int LineNo { get; set; }

    [SugarColumn(IsNullable = true)]
    public long? MaterialId { get; set; }

    [SugarColumn(Length = 50, IsNullable = false)]
    public string MaterialCode { get; set; } = string.Empty;

    [SugarColumn(Length = 200, IsNullable = false)]
    public string MaterialName { get; set; } = string.Empty;

    [SugarColumn(ColumnDataType = "decimal(12,3)", IsNullable = false)]
    public decimal ExpectedQty { get; set; }

    [SugarColumn(IsNullable = true)]
    public long? UnitId { get; set; }

    [SugarColumn(Length = 50, IsNullable = true)]
    public string? BatchNo { get; set; }

    [SugarColumn(Length = 500, IsNullable = true)]
    public string? Remark { get; set; }
}
