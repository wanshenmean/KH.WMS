using KH.WMS.Core.Api.Responses;
using KH.WMS.Core.Controllers;
using KH.WMS.Core.Models;
using KH.WMS.Entities.Outbound;
using Microsoft.AspNetCore.Mvc;

namespace KH.WMS.Modules.OutboundModule.Controllers
{
    [Route("api/outbound-allocation")]
    public class OutboundAllocationController(
        IOutboundAllocationService allocationService)
        : CrudController<OutboundAllocationHeader>(allocationService)
    {
        private readonly IOutboundAllocationService _allocationService = allocationService;

        /// <summary>
        /// 按出库单ID查询分配记录（头+明细）
        /// </summary>
        [HttpGet("by-order/{orderId}")]
        public async Task<ApiResponse> GetByOrderId(long orderId)
        {
            var data = await _allocationService.GetByOrderIdAsync(orderId);
            return ApiResponse.Ok(data: data);
        }

        /// <summary>
        /// 执行库存分配
        /// </summary>
        [HttpPost("allocate/{orderId}")]
        public async Task<ApiResponse> Allocate(long orderId)
        {
            var result = await _allocationService.AllocateAsync(orderId);
            if (result.Success)
                return ApiResponse.Ok(message: result.Message);
            return ApiResponse.Fail(ResponseCode.BAD_REQUEST, result.Message ?? "分配失败");
        }

        /// <summary>
        /// 根据分配单生成出库搬运任务
        /// </summary>
        [HttpPost("generate-tasks/{allocationHeaderId}")]
        public async Task<ApiResponse> GenerateTasks(long allocationHeaderId)
        {
            var result = await _allocationService.GenerateTasksAsync(allocationHeaderId);
            if (result.Success)
                return ApiResponse.Ok(message: result.Message);
            return ApiResponse.Fail(ResponseCode.BAD_REQUEST, result.Message ?? "生成出库任务失败");
        }
    }
}
