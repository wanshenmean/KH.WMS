using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KH.WMS.Core.Api.Responses;
using KH.WMS.Core.Attributes;
using KH.WMS.Core.Controllers;
using KH.WMS.Entities.Warehouse;
using KH.WMS.Modules.WarehouseModule.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace KH.WMS.Modules.WarehouseModule.Controllers
{
    [Route("api/warehouse-zone"), Cache(Duration = 60 * 30)]
    public class WarehouseZoneController(IWarehouseZoneService warehouseZoneService) : CrudController<MdWarehouseZone>(warehouseZoneService)
    {
        private readonly IWarehouseZoneService _warehouseZoneService = warehouseZoneService;

        [HttpGet("by-warehouse/{warehouseId}")]
        public async Task<ApiResponse> GetZonesByWarehouseIdAsync(long warehouseId)
        {
            return await _warehouseZoneService.GetZonesByWarehouseIdAsync(warehouseId);
        }

        [HttpGet("storage-zones/{warehouseId}")]
        public async Task<ApiResponse> GetStorageZonesByWarehouseIdAsync(long warehouseId)
        {
            return await _warehouseZoneService.GetStorageZonesByWarehouseIdAsync(warehouseId);
        }
    }
}
