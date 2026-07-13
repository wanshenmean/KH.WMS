using KH.WMS.Core.Api.Responses;
using KH.WMS.Core.Controllers;
using KH.WMS.Core.Models;
using KH.WMS.Entities.Inbound;
using KH.WMS.Modules.InboundModule.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace KH.WMS.Modules.InboundModule.Controllers
{
    [Route("api/inbound-container-bind")]
    public class InboundContainerBindController(
        IInboundContainerBindService containerBindService,
        IInboundOrderService inboundOrderService)
        : CrudController<InboundContainerBindHeader>(containerBindService)
    {
        private readonly IInboundContainerBindService _containerBindService = containerBindService;
        private readonly IInboundOrderService _inboundOrderService = inboundOrderService;

        /// <summary>
        /// 收货
        /// </summary>
        [HttpPost("receive/{orderId}")]
        public async Task<IActionResult> Receive(long orderId, [FromBody] List<ReceiveLineDto> receiveLines)
        {
            var result = await _inboundOrderService.ReceiveAsync(orderId, receiveLines);
            if (!result.Success)
                return BadRequest(new { success = false, message = result.Message });
            return Ok(new { success = true, message = result.Message });
        }

        [HttpPost("bind")]
        public async Task<ApiResponse> ContainerBindAsync([FromBody] List<ContainerBindDto> binds)
        {
            var result = await _containerBindService.ContainerBindAsync(binds);
            if (result.Success)
                return ApiResponse.Ok();
            return ApiResponse.Error(message: result.Message);
        }

        /// <summary>
        /// 取消组盘（解除容器绑定，回滚容器状态与入库单组盘状态）
        /// </summary>
        [HttpPost("cancel/{id}")]
        public async Task<ApiResponse> CancelBind(long id)
        {
            var result = await _containerBindService.CancelBindAsync(id);
            if (result.Success)
                return ApiResponse.Ok(message: result.Message);
            return ApiResponse.Fail(ResponseCode.BAD_REQUEST, result.Message ?? "取消组盘失败");
        }

        /// <summary>
        /// 收货并组盘
        /// </summary>
        [HttpPost("receive-and-bind")]
        public async Task<IActionResult> ReceiveAndBind([FromBody] ReceiveAndBindDto dto)
        {
            var result = await _inboundOrderService.ReceiveAndBindAsync(dto);
            if (!result.Success)
                return BadRequest(new { success = false, message = result.Message });
            return Ok(new { success = true, message = result.Message });
        }

        /// <summary>
        /// 查询入库单的组盘记录（头+明细）
        /// </summary>
        [HttpGet("by-order/{orderId}")]
        public async Task<IActionResult> GetByOrderId(long orderId)
        {
            var binds = await _inboundOrderService.GetContainerBindsAsync(orderId);
            return Ok(new { success = true, data = binds });
        }

        /// <summary>
        /// 按容器编号查询组盘记录（头+明细）
        /// </summary>
        [HttpGet("by-container/{containerCode}")]
        public async Task<IActionResult> GetByContainerCode(string containerCode)
        {
            var binds = await _containerBindService.GetByContainerCodeAsync(containerCode);
            return Ok(new { success = true, data = binds });
        }

        /// <summary>
        /// 请求上架（分配库位 + 创建上架任务）
        /// </summary>
        [HttpPost("putaway")]
        public async Task<ApiResponse> RequestPutAway([FromBody] List<long> headerIds)
        {
            var result = await _containerBindService.RequestPutAwayAsync(headerIds);
            if (result.Success)
                return ApiResponse.Ok(message: result.Message);
            return ApiResponse.Fail(ResponseCode.BAD_REQUEST, result.Message ?? "请求上架失败");
        }

        /// <summary>
        /// WCS申请上架（输送线扫码触发，传入托盘号和入库口编号）
        /// </summary>
        [HttpPost("putaway-by-wcs")]
        public async Task<ApiResponse> RequestPutAwayByWcs([FromBody] WcsPutAwayRequestDto request)
        {
            var result = await _containerBindService.RequestPutAwayByWcsAsync(request.ContainerCode, request.PortCode);
            if (result.Success)
                return ApiResponse.Ok(message: result.Message);
            return ApiResponse.Fail(ResponseCode.BAD_REQUEST, result.Message ?? "WCS申请上架失败");
        }
    }
}
