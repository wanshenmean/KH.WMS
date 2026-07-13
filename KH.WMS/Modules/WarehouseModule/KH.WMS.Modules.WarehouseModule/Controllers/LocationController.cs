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
    [Route("api/location"), Cache(Duration = 60 * 30)]
    public class LocationController(ILocationService locationService) : CrudController<MdLocation>(locationService)
    {
        [HttpGet("stat"), Cache(Enable = false)]
        public async Task<ApiResponse> GetStatData()
        {
            return await locationService.GetStatData();
        }

        [HttpGet("available-by-zone")]
        public async Task<ApiResponse> GetAvailableByZone(long warehouseId, string zoneCode)
        {
            return await locationService.GetAvailableByZoneAsync(warehouseId, zoneCode);
        }
    }
}
