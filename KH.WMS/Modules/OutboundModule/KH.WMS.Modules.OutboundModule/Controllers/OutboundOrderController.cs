using KH.WMS.Core.Api.Responses;
using KH.WMS.Core.Controllers;
using KH.WMS.Entities.Outbound;
using KH.WMS.Config.Abstractions;
using KH.WMS.Modules.OutboundModule.Interfaces;
using KH.WMS.Modules.OutboundModule.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace KH.WMS.Modules.OutboundModule.Controllers
{
    [Route("api/outbound-order")]
    public class OutboundOrderController(IOutboundOrderService outboundOrderService) : CrudController<OutboundOrder>(outboundOrderService)
    {
        private readonly IOutboundOrderService _outboundOrderService = outboundOrderService;

        /// <summary>
        /// 获取出库单的扩展字段表单配置
        /// </summary>
        [HttpGet("form-config")]
        public async Task<IActionResult> GetFormConfig()
        {
            var extService = HttpContext.RequestServices.GetRequiredService<ICfgDocumentFieldExtContract>();
            var fields = await extService.GetFieldsAsync("SALE_OUT", "HEADER");
            var columns = extService.BuildFormColumns(fields);
            var lineFields = await extService.GetFieldsAsync("SALE_OUT", "LINE");
            var lineColumns = extService.BuildFormColumns(lineFields);
            return Ok(new { success = true, data = new { columns, lineColumns } });
        }

        /// <summary>
        /// 获取出库单详情（含明细行）
        /// </summary>
        [HttpGet("{id}/detail")]
        public async Task<IActionResult> GetDetail(long id)
        {
            var result = await _outboundOrderService.GetDetailAsync(id);
            if (result == null)
                return NotFound(new { success = false, message = "出库单不存在" });
            return Ok(new { success = true, data = result });
        }

        /// <summary>
        /// 创建出库单（含明细行）
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] OutboundOrderCreateDto request)
        {
            try
            {
                await ApplyExtDataAsync(request);
                var id = await _outboundOrderService.CreateWithLinesAsync(request);
                return Ok(new { success = true, data = new { id }, message = "创建成功" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 更新出库单（含明细行，全量替换）
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(long id, [FromBody] OutboundOrderCreateDto request)
        {
            try
            {
                if (id != request.Order.Id)
                    return BadRequest(new { success = false, message = "ID不匹配" });

                await ApplyExtDataAsync(request);
                var result = await _outboundOrderService.UpdateWithLinesAsync(id, request);
                return Ok(new { success = true, message = result ? "更新成功" : "更新失败" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 将前端提交的 ExtDataRaw 序列化到实体 ExtData 字段
        /// </summary>
        private async Task ApplyExtDataAsync(OutboundOrderCreateDto request)
        {
            var extService = HttpContext.RequestServices.GetRequiredService<ICfgDocumentFieldExtContract>();

            // 单据头扩展字段
            if (request.ExtDataRaw != null && request.ExtDataRaw.Count > 0)
            {
                var headerFields = await extService.GetFieldsAsync("SALE_OUT", "HEADER");
                request.Order.ExtData = extService.SerializeExtData(request.ExtDataRaw, headerFields);
            }

            // 行级扩展字段
            if (request.LineExtDataRaw != null && request.LineExtDataRaw.Count > 0)
            {
                var lineFields = await extService.GetFieldsAsync("SALE_OUT", "LINE");
                foreach (var line in request.Lines)
                {
                    var lineIndex = line.LineNo.ToString();
                    if (lineIndex != null && request.LineExtDataRaw.TryGetValue(lineIndex, out var lineExtData) && lineExtData != null)
                    {
                        line.ExtData = extService.SerializeExtData(lineExtData, lineFields);
                    }
                }
            }
        }

        /// <summary>
        /// 确认出库单（DRAFT → CONFIRMED）
        /// </summary>
        [HttpPost("confirm/{id}")]
        public async Task<ApiResponse> Confirm(long id)
        {
            var result = await _outboundOrderService.ConfirmAsync(id);
            if (result.Success)
                return ApiResponse.Ok(message: result.Message);
            return ApiResponse.Fail(ResponseCode.BAD_REQUEST, result.Message ?? "确认失败");
        }
    }
}
