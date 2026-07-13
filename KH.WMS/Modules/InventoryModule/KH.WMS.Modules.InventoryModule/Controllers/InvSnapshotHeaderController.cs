using KH.WMS.Core.Api.Responses;
using KH.WMS.Core.Attributes;
using KH.WMS.Core.Controllers;
using KH.WMS.Entities.Constants;
using KH.WMS.Entities.Inventory;
using KH.WMS.Modules.InventoryModule.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace KH.WMS.Modules.InventoryModule.Controllers
{
    [Route("api/inventory-snapshot")]
    public class InvSnapshotHeaderController(IInvSnapshotHeaderService invSnapshotHeaderService) : CrudController<InvSnapshotHeader>(invSnapshotHeaderService)
    {
        /// <summary>
        /// 获取快照详情（含明细列表）
        /// </summary>
        [HttpGet("{id}"), Cache(Enable = false)]
        public override async Task<ApiResponse> GetById(long id)
        {
            return await invSnapshotHeaderService.GetSnapshotDetailsAsync(id);
        }

        [HttpPost("create-snapshot")]
        public async Task<ApiResponse> CreateSnapshot([FromBody] CreateSnapshotRequest request)
        {
            return await invSnapshotHeaderService.CreateSnapshotAsync(request.SnapshotName, request.SnapshotType, request.Description);
        }

        [HttpGet("{id}/details")]
        public async Task<ApiResponse> GetDetails(long id)
        {
            return await invSnapshotHeaderService.GetSnapshotDetailsAsync(id);
        }
    }

    public class CreateSnapshotRequest
    {
        public string SnapshotName { get; set; } = string.Empty;
        public string SnapshotType { get; set; } = BizConstants.SnapshotTypes.MANUAL;
        public string? Description { get; set; }
    }
}
