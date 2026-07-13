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
using KH.WMS.Entities.Constants;
using KH.WMS.Modules.WarehouseModule.Interfaces;

namespace KH.WMS.Modules.WarehouseModule.Services
{
    [RegisteredService(ServiceType = typeof(ILocationService))]
    public class LocationService(
        IRepository<MdLocation, long> locationRepository,
        IRepository<MdWarehouse, long> warehouseRepository,
        IRepository<MdWarehouseZone, long> warehouseZoneRepository,
        IUnitOfWork unitOfWork,
        IDetailSaveService detailSaveService) : CrudService<MdLocation>(locationRepository, unitOfWork, detailSaveService), ILocationService
    {
        private readonly IRepository<MdWarehouse, long> _warehouseRepository = warehouseRepository;
        private readonly IRepository<MdWarehouseZone, long> _warehouseZoneRepository = warehouseZoneRepository;
        private readonly IRepository<MdLocation, long> _locationRepository = locationRepository;

        public override async Task<ApiResponse> CreateAsync(MdLocation entity)
        {
            var warehouse = await _warehouseRepository.GetByIdAsync(entity.WarehouseId);
            var zone = await _warehouseZoneRepository.GetByIdAsync(entity.ZoneId ?? 0);

            entity.WarehouseCode = warehouse?.WarehouseCode ?? "";
            entity.ZoneCode = zone?.ZoneCode ?? "";
            entity.GenerateLocationCode();
            return await base.CreateAsync(entity);
        }

        public async Task<ApiResponse> GetStatData()
        {
            var locations = await _locationRepository.GetAllAsync();
            return ApiResponse.Ok(new { totalCount = locations.Count, emptyCount = locations.Count(l => l.Status == BizConstants.LocationStatus.EMPTY), occupiedCount = locations.Count(l => l.Status == BizConstants.LocationStatus.OCCUPIED) });
        }

        public async Task<ApiResponse> GetAvailableByZoneAsync(long warehouseId, string zoneCode)
        {
            var locations = await _locationRepository.GetListAsync(l =>
                l.WarehouseId == warehouseId &&
                l.ZoneCode == zoneCode &&
                l.Status == BizConstants.LocationStatus.EMPTY &&
                l.LockStatus == BizConstants.LocationLockStatus.NONE &&
                l.IsDisabled == BizConstants.BoolFlag.NO);

            var dtos = locations.Select(l => new { l.Id, l.LocationCode, l.AisleNo, l.RowNo, l.ColNo, l.LayerNo }).ToList();
            return ApiResponse.Ok(dtos);
        }

        /// <summary>
        /// 重写 SetStatus：MdLocation 使用 IsDisabled 字段（0=启用 1=禁用），与标准 Status 逻辑相反
        /// </summary>
        public override async Task<ApiResponse> SetStatusAsync(long id, byte status)
        {
            var entity = await GetEntityOrThrowAsync(id);

            // status=1(启用) → IsDisabled=0, status=0(禁用) → IsDisabled=1
            // IsDisabled 语义与标准 Status 相反：0=启用 / 1=禁用（见 MdLocation 实体）
            byte newIsDisabled = status;

            if (entity.IsDisabled == newIsDisabled)
                return ApiResponse.Fail(ResponseCode.BAD_REQUEST, status == 1 ? "当前已启用" : "当前已禁用");

            await BeforeSetStatusAsync(entity, status);

            entity.IsDisabled = newIsDisabled;
            await _repository.UpdateAsync(entity);

            await AfterSetStatusAsync(entity, status);

            return ApiResponse.Ok(message: status == 1 ? "已启用" : "已禁用");
        }
    }
}
