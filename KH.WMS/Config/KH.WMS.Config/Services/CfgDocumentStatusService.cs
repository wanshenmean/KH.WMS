using KH.WMS.Core.Database.Repositories;
using KH.WMS.Core.Database.UnitOfWorks;
using KH.WMS.Core.DependencyInjection.ServiceLifetimes;
using KH.WMS.Core.Services;
using KH.WMS.Config.Abstractions;
using KH.WMS.Config.Interfaces;

namespace KH.WMS.Config.Services
{
    [RegisteredService(ServiceType = typeof(ICfgDocumentStatusService))]
    public class CfgDocumentStatusService(
        IRepository<CfgDocumentStatus, long> repository,
        IUnitOfWork unitOfWork,
        IDetailSaveService detailSaveService) : CrudService<CfgDocumentStatus>(repository, unitOfWork, detailSaveService), ICfgDocumentStatusService
    {
    }
}
