using KH.WMS.Core.Database.Repositories;
using KH.WMS.Core.Database.UnitOfWorks;
using KH.WMS.Core.DependencyInjection.ServiceLifetimes;
using KH.WMS.Core.Services;
using KH.WMS.Entities.Inbound;
using Microsoft.Extensions.DependencyInjection;

namespace KH.WMS.Modules.InboundModule
{
    [RegisteredService(Lifetime = ServiceLifetime.Scoped, ServiceType = typeof(IInboundOrderLineService))]
    public class InboundOrderLineService(
        IRepository<InboundOrderLine, long> repository,
        IUnitOfWork unitOfWork,
        IDetailSaveService detailSaveService)
        : CrudService<InboundOrderLine>(repository, unitOfWork, detailSaveService), IInboundOrderLineService
    {
    }
}
