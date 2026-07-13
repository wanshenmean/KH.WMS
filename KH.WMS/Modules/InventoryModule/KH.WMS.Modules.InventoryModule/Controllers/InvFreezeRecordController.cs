using KH.WMS.Config.Abstractions;
using KH.WMS.Core.Api.Responses;
using KH.WMS.Core.Controllers;
using KH.WMS.Entities.Inventory;
using KH.WMS.Modules.InventoryModule.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace KH.WMS.Modules.InventoryModule.Controllers
{
    [Route("api/inventory-freeze")]
    public class InvFreezeRecordController(IInvFreezeRecordService invFreezeRecordService) : ExtDataCrudController<InvFreezeRecord>(invFreezeRecordService)
    {
        [HttpGet("form-config")]
        public async Task<IActionResult> GetFormConfig()
        {
            var extService = HttpContext.RequestServices.GetRequiredService<ICfgExtFieldContract>();
            var fields = await extService.GetFieldsAsync("INV_FREEZE", "HEADER");
            var columns = extService.BuildFormColumns(fields);
            return Ok(new { success = true, data = new { columns } });
        }

        [HttpPost("unfreeze/{id}")]
        public async Task<ApiResponse> Unfreeze(long id)
        {
            return await invFreezeRecordService.UnfreezeAsync(id);
        }
    }
}
