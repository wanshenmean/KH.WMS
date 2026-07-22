using KH.WMS.Core.Database.Repositories;
using KH.WMS.Core.Database.UnitOfWorks;
using KH.WMS.Core.DependencyInjection.ServiceLifetimes;
using KH.WMS.Core.Services;
using KH.WMS.Entities.Training;
using KH.WMS.Modules.TrainingModule.Interfaces;

namespace KH.WMS.Modules.TrainingModule.Services;

[RegisteredService(ServiceType = typeof(ITrainingCarrierService))]
public class TrainingCarrierService(
    IRepository<TrnCarrier, long> repository,
    IUnitOfWork unitOfWork,
    IDetailSaveService detailSaveService)
    : CrudService<TrnCarrier>(repository, unitOfWork, detailSaveService), ITrainingCarrierService
{
    protected override async Task BeforeCreateAsync(TrnCarrier entity)
    {
        await EnsureCodeUnique(entity.CarrierCode, 0);
    }

    protected override async Task BeforeUpdateAsync(TrnCarrier entity)
    {
        await EnsureCodeUnique(entity.CarrierCode, entity.Id);
    }

    private async Task EnsureCodeUnique(string code, long currentId)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new InvalidOperationException("承运商编码不能为空");

        if (await _repository.ExistsAsync(x => x.CarrierCode == code.Trim() && x.Id != currentId))
            throw new InvalidOperationException($"承运商编码 {code.Trim()} 已存在");
    }
}
