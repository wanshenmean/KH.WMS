using KH.WMS.Core.Database.Repositories;
using KH.WMS.Core.Database.UnitOfWorks;
using KH.WMS.Core.DependencyInjection.ServiceLifetimes;
using KH.WMS.Core.Services;
using KH.WMS.Entities.BaseData;
using KH.WMS.Modules.BaseDataModule.Interfaces;

namespace KH.WMS.Modules.BaseDataModule.Services
{
    [RegisteredService(ServiceType = typeof(IMaterialUnitService))]
    public class MaterialUnitService(
        IRepository<MdMaterialUnit, long> repository,
        IUnitOfWork unitOfWork,
        IDetailSaveService detailSaveService) : CrudService<MdMaterialUnit>(repository, unitOfWork, detailSaveService), IMaterialUnitService
    {
    }
}
