using KH.WMS.Core.Database.Repositories;
using KH.WMS.Core.Database.UnitOfWorks;
using KH.WMS.Core.DependencyInjection.ServiceLifetimes;
using KH.WMS.Core.Services;
using KH.WMS.Entities.Inventory;
using KH.WMS.Modules.InventoryModule.Interfaces;

namespace KH.WMS.Modules.InventoryModule.Services
{
    [RegisteredService(ServiceType = typeof(IInvMovementService))]
    public class InvMovementService(
        IRepository<InvMovement, long> repository,
        IUnitOfWork unitOfWork,
        IDetailSaveService detailSaveService) : CrudService<InvMovement>(repository, unitOfWork, detailSaveService), IInvMovementService
    {
    }
}
