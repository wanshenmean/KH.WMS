using KH.WMS.Config.Abstractions;
using KH.WMS.Core.Controllers;
using KH.WMS.Entities.Inventory;
using KH.WMS.Modules.InventoryModule.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace KH.WMS.Modules.InventoryModule.Controllers
{
    [Route("api/inventory-movement")]
    public class InvMovementController(IInvMovementService invMovementService) : ExtDataCrudController<InvMovement>(invMovementService)
    {
        [HttpGet("form-config")]
        public async Task<IActionResult> GetFormConfig()
        {
            var extService = HttpContext.RequestServices.GetRequiredService<ICfgExtFieldContract>();
            var fields = await extService.GetFieldsAsync("INV_MOVEMENT", "HEADER");
            var columns = extService.BuildFormColumns(fields);
            return Ok(new { success = true, data = new { columns } });
        }
    }
}
