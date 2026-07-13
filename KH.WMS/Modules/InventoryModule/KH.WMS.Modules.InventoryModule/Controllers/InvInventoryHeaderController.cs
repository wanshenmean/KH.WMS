using KH.WMS.Core.Api.Responses;
using KH.WMS.Core.Controllers;
using KH.WMS.Entities.Inventory;
using KH.WMS.Modules.InventoryModule.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace KH.WMS.Modules.InventoryModule.Controllers
{
    [Route("api/inventory-header")]
    public class InvInventoryHeaderController(IInvInventoryHeaderService invInventoryHeaderService) : CrudController<InvInventoryHeader>(invInventoryHeaderService)
    {
        [HttpGet("stat")]
        public async Task<ApiResponse> GetStatData()
        {
            return await invInventoryHeaderService.GetStatDataAsync();
        }

        [HttpPost("freeze/{id}")]
        public async Task<ApiResponse> Freeze(long id, [FromBody] FreezeRequest request)
        {
            return await invInventoryHeaderService.FreezeAsync(id, request.Reason);
        }

        [HttpPost("unfreeze/{id}")]
        public async Task<ApiResponse> Unfreeze(long id)
        {
            return await invInventoryHeaderService.UnfreezeAsync(id);
        }
    }

    public class FreezeRequest
    {
        public string Reason { get; set; } = string.Empty;
    }
}
