using KH.WMS.Core.Database.Repositories;
using KH.WMS.Core.Database.UnitOfWorks;
using KH.WMS.Core.DependencyInjection.ServiceLifetimes;
using KH.WMS.Core.Services;
using KH.WMS.Entities.Warehouse;
using KH.WMS.Modules.WarehouseModule.Interfaces;

namespace KH.WMS.Modules.WarehouseModule.Services
{
    [RegisteredService(ServiceType = typeof(ILogicalZoneService))]
    public class LogicalZoneService(
        IRepository<MdLogicalZone, long> repository,
        IUnitOfWork unitOfWork,
        IDetailSaveService detailSaveService) : CrudService<MdLogicalZone>(repository, unitOfWork, detailSaveService), ILogicalZoneService
    {
    }
}
