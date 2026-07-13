using KH.WMS.Core.Database.Repositories;
using KH.WMS.Core.Database.UnitOfWorks;
using KH.WMS.Core.DependencyInjection.ServiceLifetimes;
using KH.WMS.Core.Services;
using KH.WMS.Entities.Outbound;
using KH.WMS.Modules.OutboundModule.Interfaces;

namespace KH.WMS.Modules.OutboundModule.Services
{
    [RegisteredService(ServiceType = typeof(IOutboundOrderLineService))]
    public class OutboundOrderLineService(
        IRepository<OutboundOrderLine, long> repository,
        IUnitOfWork unitOfWork,
        IDetailSaveService detailSaveService) : CrudService<OutboundOrderLine>(repository, unitOfWork, detailSaveService), IOutboundOrderLineService
    {
    }
}
