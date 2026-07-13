using KH.WMS.Core.Api.Responses;
using KH.WMS.Modules.TaskModule.DTOs;
using KH.WMS.Modules.TaskModule.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace KH.WMS.Modules.TaskModule.Controllers;

/// <summary>
/// 无单据操作控制器
/// 处理不需要创建入库单/出库单的特殊仓库操作
/// </summary>
[Route("api/adhoc")]
public class AdhocController(IAdhocService adhocService) : ControllerBase
{
    /// <summary>
    /// 无单据组盘再入库（组盘 → 创建上架任务，不创建入库单）
    /// </summary>
    [HttpPost("inbound")]
    public async Task<ApiResponse> Inbound([FromBody] AdhocInboundRequest request)
    {
        var result = await adhocService.AdhocInboundAsync(request);
        if (result.Success)
            return ApiResponse.Ok(message: result.Message, data: result.Data);
        return ApiResponse.Fail(ResponseCode.BAD_REQUEST, result.Message ?? "无单据入库失败");
    }

    /// <summary>
    /// 无单据出库（按库存明细ID列表）
    /// </summary>
    [HttpPost("outbound")]
    public async Task<ApiResponse> Outbound([FromBody] AdhocOutboundRequest request)
    {
        var result = await adhocService.AdhocOutboundAsync(request);
        if (result.Success)
            return ApiResponse.Ok(message: result.Message, data: result.Data);
        return ApiResponse.Fail(ResponseCode.BAD_REQUEST, result.Message ?? "无单据出库失败");
    }

    /// <summary>
    /// 指定托盘号出库（全部或选择物料）
    /// </summary>
    [HttpPost("outbound-by-container")]
    public async Task<ApiResponse> OutboundByContainer([FromBody] AdhocOutboundByContainerRequest request)
    {
        var result = await adhocService.AdhocOutboundByContainerAsync(request);
        if (result.Success)
            return ApiResponse.Ok(message: result.Message, data: result.Data);
        return ApiResponse.Fail(ResponseCode.BAD_REQUEST, result.Message ?? "托盘出库失败");
    }

    /// <summary>
    /// 指定货位出库
    /// </summary>
    [HttpPost("outbound-by-location")]
    public async Task<ApiResponse> OutboundByLocation([FromBody] AdhocOutboundByLocationRequest request)
    {
        var result = await adhocService.AdhocOutboundByLocationAsync(request);
        if (result.Success)
            return ApiResponse.Ok(message: result.Message, data: result.Data);
        return ApiResponse.Fail(ResponseCode.BAD_REQUEST, result.Message ?? "货位出库失败");
    }

    /// <summary>
    /// 起始地址→目的地址上架
    /// </summary>
    [HttpPost("putaway-from-to")]
    public async Task<ApiResponse> PutawayFromTo([FromBody] AdhocPutawayFromToRequest request)
    {
        var result = await adhocService.AdhocPutawayFromToAsync(request);
        if (result.Success)
            return ApiResponse.Ok(message: result.Message, data: result.Data);
        return ApiResponse.Fail(ResponseCode.BAD_REQUEST, result.Message ?? "上架失败");
    }

    /// <summary>
    /// 直接上架到指定地址（跳过策略计算）
    /// </summary>
    [HttpPost("putaway-to")]
    public async Task<ApiResponse> PutawayTo([FromBody] AdhocPutawayToRequest request)
    {
        var result = await adhocService.AdhocPutawayToAsync(request);
        if (result.Success)
            return ApiResponse.Ok(message: result.Message, data: result.Data);
        return ApiResponse.Fail(ResponseCode.BAD_REQUEST, result.Message ?? "上架失败");
    }

    /// <summary>
    /// 按多种条件查询库存（用于无单据出库页筛选）
    /// </summary>
    [HttpPost("query-inventory")]
    public async Task<ApiResponse> QueryInventory([FromBody] AdhocInventoryQueryRequest request)
    {
        var result = await adhocService.QueryInventoryAsync(request);
        return result.Success
            ? ApiResponse.Ok(data: result.Data)
            : ApiResponse.Fail(ResponseCode.BAD_REQUEST, result.Message ?? "查询库存失败");
    }

    /// <summary>
    /// 按库区出库
    /// </summary>
    [HttpPost("outbound-by-zone")]
    public async Task<ApiResponse> OutboundByZone([FromBody] AdhocOutboundByZoneRequest request)
    {
        var result = await adhocService.AdhocOutboundByZoneAsync(request);
        if (result.Success)
            return ApiResponse.Ok(message: result.Message, data: result.Data);
        return ApiResponse.Fail(ResponseCode.BAD_REQUEST, result.Message ?? "按库区出库失败");
    }

    /// <summary>
    /// 按巷道出库
    /// </summary>
    [HttpPost("outbound-by-aisle")]
    public async Task<ApiResponse> OutboundByAisle([FromBody] AdhocOutboundByAisleRequest request)
    {
        var result = await adhocService.AdhocOutboundByAisleAsync(request);
        if (result.Success)
            return ApiResponse.Ok(message: result.Message, data: result.Data);
        return ApiResponse.Fail(ResponseCode.BAD_REQUEST, result.Message ?? "按巷道出库失败");
    }

    /// <summary>
    /// 按出库口出库
    /// </summary>
    [HttpPost("outbound-by-port")]
    public async Task<ApiResponse> OutboundByPort([FromBody] AdhocOutboundByPortRequest request)
    {
        var result = await adhocService.AdhocOutboundByPortAsync(request);
        if (result.Success)
            return ApiResponse.Ok(message: result.Message, data: result.Data);
        return ApiResponse.Fail(ResponseCode.BAD_REQUEST, result.Message ?? "按出库口出库失败");
    }

    /// <summary>
    /// 路线校验（起点地址能否到达目标地址）
    /// </summary>
    [HttpPost("check-route")]
    public async Task<ApiResponse> CheckRoute([FromBody] AdhocRouteCheckRequest request)
    {
        var result = await adhocService.CheckRouteAsync(request);
        return result.Success
            ? ApiResponse.Ok(data: result.Data)
            : ApiResponse.Fail(ResponseCode.BAD_REQUEST, result.Message ?? "路线校验失败");
    }

}
