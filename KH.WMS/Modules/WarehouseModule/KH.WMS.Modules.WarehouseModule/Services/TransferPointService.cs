using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KH.WMS.Core.Database.Repositories;
using KH.WMS.Core.Database.UnitOfWorks;
using KH.WMS.Core.DependencyInjection.ServiceLifetimes;
using KH.WMS.Core.Services;
using KH.WMS.Entities.Warehouse;
using KH.WMS.Modules.WarehouseModule.Interfaces;

namespace KH.WMS.Modules.WarehouseModule.Services
{
    [RegisteredService(ServiceType = typeof(ITransferPointService))]
    public class TransferPointService(
        IRepository<MdTransferPoint, long> repository,
        IUnitOfWork unitOfWork,
        IDetailSaveService detailSaveService) : CrudService<MdTransferPoint>(repository, unitOfWork, detailSaveService), ITransferPointService
    {

    }
}
