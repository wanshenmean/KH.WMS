using KH.WMS.Core.Database.Repositories;
using KH.WMS.Core.Database.UnitOfWorks;
using KH.WMS.Core.DependencyInjection.ServiceLifetimes;
using KH.WMS.Core.Services;
using KH.WMS.Config.Abstractions;
using KH.WMS.Config.Interfaces;

namespace KH.WMS.Config.Services
{
    [RegisteredService(ServiceType = typeof(ICfgExtFieldTypeConfigService))]
    public class CfgExtFieldTypeConfigService(
        IRepository<CfgExtFieldType, long> repository,
        IUnitOfWork unitOfWork,
        IDetailSaveService detailSaveService) : CrudService<CfgExtFieldType>(repository, unitOfWork, detailSaveService), ICfgExtFieldTypeConfigService
    {
    }
}
