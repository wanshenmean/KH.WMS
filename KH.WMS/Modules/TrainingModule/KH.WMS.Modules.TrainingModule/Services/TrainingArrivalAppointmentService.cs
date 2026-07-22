using KH.WMS.Core.Database.Repositories;
using KH.WMS.Core.Database.UnitOfWorks;
using KH.WMS.Core.DependencyInjection.ServiceLifetimes;
using KH.WMS.Core.Services;
using KH.WMS.Entities.Training;
using KH.WMS.Modules.TrainingModule.Interfaces;

namespace KH.WMS.Modules.TrainingModule.Services;

[RegisteredService(ServiceType = typeof(ITrainingArrivalAppointmentService))]
public class TrainingArrivalAppointmentService(
    IRepository<TrnArrivalAppointment, long> repository,
    IUnitOfWork unitOfWork,
    IDetailSaveService detailSaveService)
    : CrudService<TrnArrivalAppointment>(repository, unitOfWork, detailSaveService), ITrainingArrivalAppointmentService
{
    protected override async Task BeforeCreateAsync(TrnArrivalAppointment entity)
    {
        await Validate(entity);
    }

    protected override async Task BeforeUpdateAsync(TrnArrivalAppointment entity)
    {
        await Validate(entity);
    }

    private async Task Validate(TrnArrivalAppointment entity)
    {
        if (string.IsNullOrWhiteSpace(entity.AppointmentNo))
            throw new InvalidOperationException("预约单号不能为空");
        if (await _repository.ExistsAsync(x => x.AppointmentNo == entity.AppointmentNo.Trim() && x.Id != entity.Id))
            throw new InvalidOperationException($"预约单号 {entity.AppointmentNo.Trim()} 已存在");
        if (entity.Items is not { Count: > 0 })
            throw new InvalidOperationException("到货预约至少需要一条明细");
        if (entity.Items.Any(x => x.LineNo <= 0))
            throw new InvalidOperationException("明细行号必须大于 0");
        if (entity.Items.GroupBy(x => x.LineNo).Any(x => x.Count() > 1))
            throw new InvalidOperationException("同一预约单的明细行号不能重复");
        if (entity.Items.Any(x => string.IsNullOrWhiteSpace(x.MaterialCode) || string.IsNullOrWhiteSpace(x.MaterialName)))
            throw new InvalidOperationException("明细物料编码和名称不能为空");
        if (entity.Items.Any(x => x.ExpectedQty <= 0))
            throw new InvalidOperationException("明细预计到货数量必须大于 0");
    }
}
