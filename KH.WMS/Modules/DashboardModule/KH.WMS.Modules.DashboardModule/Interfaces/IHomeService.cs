using KH.WMS.Core.Api.Responses;
using KH.WMS.Modules.DashboardModule.DTOs;

namespace KH.WMS.Modules.DashboardModule.Interfaces;

/// <summary>
/// 首页面板服务接口
/// </summary>
public interface IHomeService
{
    /// <summary>
    /// 获取首页统计卡片数据
    /// </summary>
    Task<ApiResponse> GetStatAsync();

    /// <summary>
    /// 获取近N日出入库趋势
    /// </summary>
    Task<ApiResponse> GetTrendAsync(int days = 7);

    /// <summary>
    /// 获取最近完成的出入库单据
    /// </summary>
    Task<ApiResponse> GetRecentDocumentsAsync(int top = 10);

    /// <summary>
    /// 获取库存概览数据
    /// </summary>
    Task<ApiResponse> GetInventoryOverviewAsync();

    /// <summary>
    /// 获取作业概览（任务类型分布 + 库位状态）
    /// </summary>
    Task<ApiResponse> GetOperationsOverviewAsync();
}
