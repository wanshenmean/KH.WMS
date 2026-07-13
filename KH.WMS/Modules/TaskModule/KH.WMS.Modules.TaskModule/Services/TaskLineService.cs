using KH.WMS.Core.Database.Repositories;
using KH.WMS.Core.Database.UnitOfWorks;
using KH.WMS.Core.DependencyInjection.ServiceLifetimes;
using KH.WMS.Core.Services;
using KH.WMS.Entities.Task;
using KH.WMS.Modules.TaskModule.Interfaces;

namespace KH.WMS.Modules.TaskModule.Services
{
    [RegisteredService(ServiceType = typeof(ITaskLineService))]
    public class TaskLineService(
        IRepository<TaskLine, long> repository,
        IUnitOfWork unitOfWork,
        IDetailSaveService detailSaveService) : CrudService<TaskLine>(repository, unitOfWork, detailSaveService), ITaskLineService
    {
    }
}
