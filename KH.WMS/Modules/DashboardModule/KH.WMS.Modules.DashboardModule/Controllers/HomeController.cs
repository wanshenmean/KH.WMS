using KH.WMS.Core.Api.Responses;
using KH.WMS.Modules.DashboardModule.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace KH.WMS.Modules.DashboardModule.Controllers;

/// <summary>
/// 首页面板
/// </summary>
[Route("api/home")]
public class HomeController(IHomeService homeService) : ControllerBase
{
    /// <summary>
    /// 获取首页统计卡片数据
    /// </summary>
    [HttpGet("stat")]
    public async Task<ApiResponse> GetStat()
    {
        return await homeService.GetStatAsync();
    }

    /// <summary>
    /// 获取近N日出入库趋势
    /// </summary>
    [HttpGet("trend")]
    public async Task<ApiResponse> GetTrend([FromQuery] int days = 7)
    {
        return await homeService.GetTrendAsync(days);
    }

    /// <summary>
    /// 获取最近完成的出入库单据
    /// </summary>
    [HttpGet("recent-documents")]
    public async Task<ApiResponse> GetRecentDocuments([FromQuery] int top = 10)
    {
        return await homeService.GetRecentDocumentsAsync(top);
    }

    /// <summary>
    /// 获取库存概览数据
    /// </summary>
    [HttpGet("inventory-overview")]
    public async Task<ApiResponse> GetInventoryOverview()
    {
        return await homeService.GetInventoryOverviewAsync();
    }

    /// <summary>
    /// 获取作业概览（任务类型分布 + 库位状态）
    /// </summary>
    [HttpGet("operations")]
    public async Task<ApiResponse> GetOperationsOverview()
    {
        return await homeService.GetOperationsOverviewAsync();
    }
}
