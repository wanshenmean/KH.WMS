using KH.WMS.Core.Database.Repositories;
using KH.WMS.Core.Database.UnitOfWorks;
using KH.WMS.Core.DependencyInjection.ServiceLifetimes;
using KH.WMS.Core.Services;
using KH.WMS.Entities.BaseData;
using KH.WMS.Modules.BaseDataModule.Interfaces;

namespace KH.WMS.Modules.BaseDataModule.Services
{
    [RegisteredService(ServiceType = typeof(IContainerService))]
    public class ContainerService(
        IRepository<MdContainer, long> repository,
        IUnitOfWork unitOfWork,
        IDetailSaveService detailSaveService) : CrudService<MdContainer>(repository, unitOfWork, detailSaveService), IContainerService
    {
    }
}
