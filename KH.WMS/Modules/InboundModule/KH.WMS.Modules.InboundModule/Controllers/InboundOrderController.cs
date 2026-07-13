using KH.WMS.Core.Controllers;
using KH.WMS.Entities.Constants;
using KH.WMS.Entities.Inbound;
using KH.WMS.Config.Abstractions;
using KH.WMS.Modules.InboundModule;
using KH.WMS.Modules.InboundModule.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace KH.WMS.Modules.InboundModule.Controllers
{
    [Route("api/inbound-order")]
    public class InboundOrderController(IInboundOrderService inboundOrderService) : CrudController<InboundOrder>(inboundOrderService)
    {
        private readonly IInboundOrderService _inboundOrderService = inboundOrderService;

        /// <summary>
        /// 获取入库单的扩展字段表单配置
        /// 根据 CfgDocumentField 配置动态生成 KhForm columns
        /// </summary>
        [HttpGet("form-config")]
        public async Task<IActionResult> GetFormConfig()
        {
            var extService = HttpContext.RequestServices.GetRequiredService<ICfgDocumentFieldExtContract>();
            var fields = await extService.GetFieldsAsync(BizConstants.OrderTypes.PURCHASE_IN, "HEADER");
            var columns = extService.BuildFormColumns(fields);
            var lineFields = await extService.GetFieldsAsync(BizConstants.OrderTypes.PURCHASE_IN, "LINE");
            var lineColumns = extService.BuildFormColumns(lineFields);
            return Ok(new { success = true, data = new { columns, lineColumns } });
        }

        /// <summary>
        /// 获取入库单详情（含明细行）
        /// </summary>
        [HttpGet("{id}/detail")]
        public async Task<IActionResult> GetDetail(long id)
        {
            var result = await _inboundOrderService.GetDetailAsync(id);
            if (result == null)
                return NotFound(new { success = false, message = "入库单不存在" });
            return Ok(new { success = true, data = result });
        }

        /// <summary>
        /// 创建入库单（含明细行）
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] InboundOrderCreateDto request)
        {
            try
            {
                await ApplyExtDataAsync(request);
                var id = await _inboundOrderService.CreateWithLinesAsync(request);
                return Ok(new { success = true, data = new { id }, message = "创建成功" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// 更新入库单（含明细行，全量替换）
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(long id, [FromBody] InboundOrderCreateDto request)
        {
            try
            {
                if (id != request.Order.Id)
                    return BadRequest(new { success = false, message = "ID不匹配" });

                await ApplyExtDataAsync(request);
                var result = await _inboundOrderService.UpdateWithLinesAsync(id, request);
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
        private async Task ApplyExtDataAsync(InboundOrderCreateDto request)
        {
            var extService = HttpContext.RequestServices.GetRequiredService<ICfgDocumentFieldExtContract>();

            // 单据头扩展字段
            if (request.ExtDataRaw != null && request.ExtDataRaw.Count > 0)
            {
                var headerFields = await extService.GetFieldsAsync(BizConstants.OrderTypes.PURCHASE_IN, "HEADER");
                request.Order.ExtData = extService.SerializeExtData(request.ExtDataRaw, headerFields);
            }

            // 行级扩展字段
            if (request.LineExtDataRaw != null && request.LineExtDataRaw.Count > 0)
            {
                var lineFields = await extService.GetFieldsAsync(BizConstants.OrderTypes.PURCHASE_IN, "LINE");
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
    }
}
