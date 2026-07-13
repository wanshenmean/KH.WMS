using KH.WMS.Core.Api.Responses;
using KH.WMS.Core.Database.Repositories;
using KH.WMS.Core.Database.UnitOfWorks;
using KH.WMS.Core.DependencyInjection.ServiceLifetimes;
using KH.WMS.Core.Services;
using KH.WMS.Entities.Warehouse;
using KH.WMS.Entities.Constants;
using KH.WMS.Modules.WarehouseModule.DTOs;
using KH.WMS.Modules.WarehouseModule.Interfaces;

namespace KH.WMS.Modules.WarehouseModule.Services
{
    [RegisteredService(ServiceType = typeof(IWarehouseZoneService))]
    public class WarehouseZoneService(
        IRepository<MdWarehouseZone, long> repository,
        IUnitOfWork unitOfWork,
        IDetailSaveService detailSaveService) : CrudService<MdWarehouseZone>(repository, unitOfWork, detailSaveService), IWarehouseZoneService
    {
        public async Task<ApiResponse> GetZonesByWarehouseIdAsync(long warehouseId)
        {
            var zones = await _repository.GetListAsync(x => x.WarehouseId == warehouseId);
            var zoneDtos = zones.Select(z => new { z.Id, z.ZoneCode, z.ZoneName }).ToList();
            return ApiResponse.Ok(zoneDtos);
        }

        public async Task<ApiResponse> GetStorageZonesByWarehouseIdAsync(long warehouseId)
        {
            var zones = await _repository.GetListAsync(x => x.WarehouseId == warehouseId);
            var storageZones = zones.Where(z => BizConstants.ZoneTypes.IsStorageZone(z.ZoneType)).ToList();
            var zoneDtos = storageZones.Select(z => new { z.Id, z.ZoneCode, z.ZoneName }).ToList();
            return ApiResponse.Ok(zoneDtos);
        }
    }
}
