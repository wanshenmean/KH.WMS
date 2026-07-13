using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KH.WMS.Core.Api.Responses;
using KH.WMS.Core.Database.Repositories;
using KH.WMS.Core.Database.UnitOfWorks;
using KH.WMS.Core.DependencyInjection.ServiceLifetimes;
using KH.WMS.Core.Services;
using KH.WMS.Entities.Warehouse;
using KH.WMS.Modules.WarehouseModule.Interfaces;

namespace KH.WMS.Modules.WarehouseModule.Services
{
    [RegisteredService(ServiceType = typeof(IWarehouseService))]
    public class WarehouseService(
        IRepository<MdWarehouse, long> repository,
        IRepository<MdWarehouseZone, long> warehouseZoneRepository,
        IRepository<MdAisle, long> aisleRepository,
        IUnitOfWork unitOfWork,
        IDetailSaveService detailSaveService) : CrudService<MdWarehouse>(repository, unitOfWork, detailSaveService), IWarehouseService
    {
        private readonly IRepository<MdWarehouseZone, long> _warehouseZoneRepository = warehouseZoneRepository;
        private readonly IRepository<MdAisle, long> _aisleRepository = aisleRepository;
        public async Task<ApiResponse> GetZoneAndAisleAsync(long warehouseId)
        {
            var zones = await _warehouseZoneRepository.GetListAsync(x => x.WarehouseId == warehouseId);
            var zoneDtos = zones.Select(z => new { z.Id, z.ZoneName }).ToList();

            var aisles = await _aisleRepository.GetListAsync(x => x.WarehouseId == warehouseId);
            var aisleDtos = aisles.Select(a => new { a.Id, a.AisleName }).ToList();

            return ApiResponse.Ok(new { Zones = zoneDtos, Aisles = aisleDtos });
        }
    }
}
