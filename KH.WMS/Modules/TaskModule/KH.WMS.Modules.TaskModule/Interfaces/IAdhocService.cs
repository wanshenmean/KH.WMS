using KH.WMS.Core.Models;
using KH.WMS.Modules.TaskModule.DTOs;

namespace KH.WMS.Modules.TaskModule.Interfaces;

/// <summary>
/// 无单据操作服务接口
/// 处理不需要创建入库单/出库单的特殊仓库操作
/// </summary>
public interface IAdhocService
{
    /// <summary>
    /// 无单据组盘再入库（组盘 → 创建上架任务，不创建入库单）
    /// </summary>
    Task<ServiceResult<string>> AdhocInboundAsync(AdhocInboundRequest request);

    /// <summary>
    /// 无单据出库（按库存明细ID列表）
    /// </summary>
    Task<ServiceResult<string>> AdhocOutboundAsync(AdhocOutboundRequest request);

    /// <summary>
    /// 指定托盘号出库（全部或选择物料）
    /// </summary>
    Task<ServiceResult<string>> AdhocOutboundByContainerAsync(AdhocOutboundByContainerRequest request);

    /// <summary>
    /// 指定货位出库
    /// </summary>
    Task<ServiceResult<string>> AdhocOutboundByLocationAsync(AdhocOutboundByLocationRequest request);

    /// <summary>
    /// 起始地址→目的地址上架
    /// </summary>
    Task<ServiceResult<string>> AdhocPutawayFromToAsync(AdhocPutawayFromToRequest request);

    /// <summary>
    /// 直接上架到指定地址（跳过策略计算）
    /// </summary>
    Task<ServiceResult<string>> AdhocPutawayToAsync(AdhocPutawayToRequest request);

    // ==================== 新增方法 ====================

    /// <summary>
    /// 按多种条件查询库存（用于出库页筛选）
    /// </summary>
    Task<ServiceResult<List<object>>> QueryInventoryAsync(AdhocInventoryQueryRequest request);

    /// <summary>
    /// 按库区出库
    /// </summary>
    Task<ServiceResult<string>> AdhocOutboundByZoneAsync(AdhocOutboundByZoneRequest request);

    /// <summary>
    /// 按巷道出库
    /// </summary>
    Task<ServiceResult<string>> AdhocOutboundByAisleAsync(AdhocOutboundByAisleRequest request);

    /// <summary>
    /// 按出库口出库
    /// </summary>
    Task<ServiceResult<string>> AdhocOutboundByPortAsync(AdhocOutboundByPortRequest request);

    /// <summary>
    /// 路线校验（起点能否到达目标）
    /// </summary>
    Task<ServiceResult<AdhocRouteCheckResult>> CheckRouteAsync(AdhocRouteCheckRequest request);

}
