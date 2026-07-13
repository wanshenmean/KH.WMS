using KH.WMS.Config.Abstractions;
using KH.WMS.Core.Database.Repositories;
using KH.WMS.Core.Database.UnitOfWorks;
using KH.WMS.Core.DependencyInjection.ServiceLifetimes;
using KH.WMS.Core.Services;
using KH.WMS.Config.Abstractions;
using KH.WMS.Config.Interfaces;

namespace KH.WMS.Config.Services
{
    [RegisteredService(ServiceType = typeof(ICfgDocumentFieldService))]
    public class CfgDocumentFieldService(
        IRepository<CfgDocumentField, long> repository,
        IRepository<CfgDocumentType, long> docTypeRepository,
        IUnitOfWork unitOfWork,
        IDetailSaveService detailSaveService,
        ICfgDocumentFieldExtContract docFieldExtContract) : CrudService<CfgDocumentField>(repository, unitOfWork, detailSaveService), ICfgDocumentFieldService
    {
        protected override async Task AfterCreateAsync(CfgDocumentField entity)
        {
            await ClearCacheByDocTypeAsync(entity.DocTypeId);
        }

        protected override async Task AfterUpdateAsync(CfgDocumentField entity)
        {
            await ClearCacheByDocTypeAsync(entity.DocTypeId);
        }

        protected override async Task AfterDeleteAsync(long id, CfgDocumentField entity)
        {
            await ClearCacheByDocTypeAsync(entity.DocTypeId);
        }

        protected override async Task AfterBatchDeleteAsync(List<long> ids)
        {
            var docTypeIds = (await repository.GetListAsync(f => ids.Contains(f.Id)))
                .Select(f => f.DocTypeId).Distinct();
            foreach (var docTypeId in docTypeIds)
                await ClearCacheByDocTypeAsync(docTypeId);
        }

        private async Task ClearCacheByDocTypeAsync(long docTypeId)
        {
            var docType = await docTypeRepository.GetByIdAsync(docTypeId);
            if (docType != null)
                docFieldExtContract.ClearCache(docType.TypeCode);
        }
    }
}
