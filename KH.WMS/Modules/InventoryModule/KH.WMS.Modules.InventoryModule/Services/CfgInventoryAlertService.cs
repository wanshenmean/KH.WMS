using KH.WMS.Core.Database.Repositories;
using KH.WMS.Core.Database.UnitOfWorks;
using KH.WMS.Core.DependencyInjection.ServiceLifetimes;
using KH.WMS.Core.Services;
using KH.WMS.Entities.Inventory;
using KH.WMS.Modules.InventoryModule.Interfaces;

namespace KH.WMS.Modules.InventoryModule.Services
{
    [RegisteredService(ServiceType = typeof(ICfgInventoryAlertService))]
    public class CfgInventoryAlertService(
        IRepository<CfgInventoryAlert, long> repository,
        IUnitOfWork unitOfWork,
        IDetailSaveService detailSaveService) : CrudService<CfgInventoryAlert>(repository, unitOfWork, detailSaveService), ICfgInventoryAlertService
    {
    }
}
