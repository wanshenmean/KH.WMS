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
    [Route("api/warehouse"), Cache(Duration = 60 * 30)]
    public class WarehouseController(IWarehouseService warehouseService) : CrudController<MdWarehouse>(warehouseService)
    {
        private readonly IWarehouseService _warehouseService = warehouseService;

        [HttpGet("zone-aisle/{warehouseId}")]
        public async Task<ApiResponse> GetZoneAndAisleAsync(long warehouseId)
        {
            return await _warehouseService.GetZoneAndAisleAsync(warehouseId);
        }
    }
}
