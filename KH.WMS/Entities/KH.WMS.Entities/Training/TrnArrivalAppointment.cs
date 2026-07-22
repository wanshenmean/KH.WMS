using KH.WMS.Core.Models.Entities;
using SqlSugar;

namespace KH.WMS.Entities.Training;

[SugarTable("trn_arrival_appointment")]
[SugarIndex("uk_trn_arrival_appointment_no", nameof(AppointmentNo), OrderByType.Asc, true)]
public class TrnArrivalAppointment : BaseEntity<long>
{
    [SugarColumn(Length = 50, IsNullable = false)]
    public string AppointmentNo { get; set; } = string.Empty;

    [SugarColumn(IsNullable = true)]
    public long? CarrierId { get; set; }

    [SugarColumn(IsNullable = true)]
    public long? OwnerId { get; set; }

    [SugarColumn(IsNullable = true)]
    public long? WarehouseId { get; set; }

    [SugarColumn(IsNullable = false)]
    public DateOnly AppointmentDate { get; set; }

    [SugarColumn(Length = 20, IsNullable = true)]
    public string? AppointmentTimeSlot { get; set; }

    [SugarColumn(Length = 30, IsNullable = true)]
    public string? VehicleNo { get; set; }

    [SugarColumn(Length = 50, IsNullable = true)]
    public string? DriverName { get; set; }

    [SugarColumn(Length = 20, IsNullable = true)]
    public string? DriverPhone { get; set; }

    [SugarColumn(Length = 500, IsNullable = true)]
    public string? Remark { get; set; }

    [Navigate(NavigateType.OneToMany, nameof(TrnArrivalAppointmentLine.AppointmentId), nameof(Id))]
    public List<TrnArrivalAppointmentLine>? Items { get; set; }
}
