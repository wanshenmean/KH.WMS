using KH.WMS.Core.Database.Repositories;
using KH.WMS.Core.Database.UnitOfWorks;
using KH.WMS.Core.DependencyInjection.ServiceLifetimes;
using KH.WMS.Core.Services;
using KH.WMS.Entities.Training;
using KH.WMS.Modules.TrainingModule.Interfaces;

namespace KH.WMS.Modules.TrainingModule.Services;

[RegisteredService(ServiceType = typeof(ITrainingOwnerProfileService))]
public class TrainingOwnerProfileService(
    IRepository<TrnOwnerProfile, long> repository,
    IUnitOfWork unitOfWork,
    IDetailSaveService detailSaveService)
    : CrudService<TrnOwnerProfile>(repository, unitOfWork, detailSaveService), ITrainingOwnerProfileService;
