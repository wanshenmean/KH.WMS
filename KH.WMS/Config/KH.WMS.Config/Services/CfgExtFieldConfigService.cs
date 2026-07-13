using KH.WMS.Config.Abstractions;
using KH.WMS.Core.Database.Repositories;
using KH.WMS.Core.Database.UnitOfWorks;
using KH.WMS.Core.DependencyInjection.ServiceLifetimes;
using KH.WMS.Core.Services;
using KH.WMS.Config.Abstractions;
using KH.WMS.Config.Interfaces;

namespace KH.WMS.Config.Services
{
    [RegisteredService(ServiceType = typeof(ICfgExtFieldConfigService))]
    public class CfgExtFieldConfigService(
        IRepository<CfgExtField, long> repository,
        IUnitOfWork unitOfWork,
        IDetailSaveService detailSaveService,
        IRepository<CfgExtFieldType, long> extFieldTypeRepository,
        ICfgExtFieldContract extFieldContract) : CrudService<CfgExtField>(repository, unitOfWork, detailSaveService), ICfgExtFieldConfigService
    {
        private readonly IRepository<CfgExtFieldType, long> _extFieldTypeRepository = extFieldTypeRepository;
        protected override async Task AfterCreateAsync(CfgExtField entity)
        {
            await ClearCacheByEntityAsync(entity.EntityTypeId);
        }

        protected override async Task AfterUpdateAsync(CfgExtField entity)
        {
            await ClearCacheByEntityAsync(entity.EntityTypeId);
        }

        protected override async Task AfterDeleteAsync(long id, CfgExtField entity)
        {
            await ClearCacheByEntityAsync(entity.EntityTypeId);
        }

        protected override async Task AfterBatchDeleteAsync(List<long> ids)
        {
            var entityIds = (await repository.GetListAsync(f => ids.Contains(f.Id)))
                .Select(f => f.EntityTypeId).Distinct();
            foreach (var entityTypeId in entityIds)
                await ClearCacheByEntityAsync(entityTypeId);
        }

        private async Task ClearCacheByEntityAsync(long entityTypeId)
        {
            var entityType = await _extFieldTypeRepository.GetByIdAsync(entityTypeId);
            if (entityType != null)
                extFieldContract.ClearCache(entityType.EntityCode);
        }
    }
}
