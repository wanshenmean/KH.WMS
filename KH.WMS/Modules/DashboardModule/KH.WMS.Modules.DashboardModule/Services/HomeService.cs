using KH.WMS.Core.Api.Responses;
using KH.WMS.Core.Database.Repositories;
using KH.WMS.Core.Database.UnitOfWorks;
using KH.WMS.Core.DependencyInjection.ServiceLifetimes;
using KH.WMS.Entities.Constants;
using KH.WMS.Entities.Inbound;
using KH.WMS.Entities.Inventory;
using KH.WMS.Entities.Outbound;
using KH.WMS.Entities.Task;
using KH.WMS.Entities.Warehouse;
using KH.WMS.Modules.DashboardModule.DTOs;
using KH.WMS.Modules.DashboardModule.Interfaces;
using SqlSugar;

namespace KH.WMS.Modules.DashboardModule.Services;

/// <summary>
/// 首页面板服务实现
/// </summary>
[RegisteredService(ServiceType = typeof(IHomeService))]
public class HomeService(IUnitOfWork unitOfWork) : IHomeService
{
    /// <inheritdoc />
    public async Task<ApiResponse> GetStatAsync()
    {
        var inboundRepo = unitOfWork.GetRepository<InboundOrder, long>();
        var outboundRepo = unitOfWork.GetRepository<OutboundOrder, long>();
        var taskRepo = unitOfWork.GetRepository<TaskHeader, long>();
        var locationRepo = unitOfWork.GetRepository<MdLocation, long>();
        var headerRepo = unitOfWork.GetRepository<InvInventoryHeader, long>();

        var today = DateTime.Today;

        var todayInbound = await inboundRepo.CountAsync(e => e.CreatedTime >= today && e.OrderStatus != BizConstants.InboundOrderStatus.CANCELLED);
        var todayOutbound = await outboundRepo.CountAsync(e => e.CreatedTime >= today && e.OrderStatus != BizConstants.OutboundOrderStatus.CANCELLED);
        var todayCompletedTask = await taskRepo.CountAsync(e => e.TaskStatus == BizConstants.TaskStatus.COMPLETED && e.CompleteTime >= today);
        var inProgressTask = await taskRepo.CountAsync(e => e.TaskStatus == BizConstants.TaskStatus.IN_PROGRESS);
        var containerCount = await headerRepo.CountAsync();

        var occupied = await locationRepo.CountAsync(e => e.Status == BizConstants.LocationStatus.OCCUPIED);
        var locationTotal = await locationRepo.CountAsync();
        var usagePercent = locationTotal > 0 ? (int)Math.Round((double)occupied / locationTotal * 100) : 0;

        return ApiResponse.Ok(new HomeStatDto
        {
            TodayInboundCount = todayInbound,
            TodayOutboundCount = todayOutbound,
            TodayCompletedTaskCount = todayCompletedTask,
            InProgressTaskCount = inProgressTask,
            LocationUsagePercent = usagePercent,
            ContainerCount = containerCount,
        });
    }

    /// <inheritdoc />
    public async Task<ApiResponse> GetTrendAsync(int days = 7)
    {
        var inboundRepo = unitOfWork.GetRepository<InboundOrder, long>();
        var outboundRepo = unitOfWork.GetRepository<OutboundOrder, long>();

        var startDate = DateTime.Today.AddDays(-days + 1);

        var inboundAll = await inboundRepo.GetListAsync(e =>
            e.CreatedTime >= startDate &&
            e.OrderStatus != BizConstants.InboundOrderStatus.CANCELLED);

        var outboundAll = await outboundRepo.GetListAsync(e =>
            e.CreatedTime >= startDate &&
            e.OrderStatus != BizConstants.OutboundOrderStatus.CANCELLED);

        var inboundTrend = BuildTrend(inboundAll, startDate, days);
        var outboundTrend = BuildTrend(outboundAll, startDate, days);

        return ApiResponse.Ok(new TrendDto
        {
            Inbound = inboundTrend,
            Outbound = outboundTrend,
        });
    }

    /// <inheritdoc />
    public async Task<ApiResponse> GetRecentDocumentsAsync(int top = 10)
    {
        if (top <= 0)
        {
            top = 10;
        }

        var inboundRepo = unitOfWork.GetRepository<InboundOrder, long>();
        var outboundRepo = unitOfWork.GetRepository<OutboundOrder, long>();

        // 服务端过滤+排序+Take，避免把历史所有完成单据拉入内存；每边各取 top 条后合并再裁剪
        var inbound = await inboundRepo.AsQueryable()
            .Where(e => e.OrderStatus == BizConstants.InboundOrderStatus.COMPLETED)
            .OrderBy(e => e.LastModifiedTime, OrderByType.Desc)
            .Take(top)
            .Select(e => new
            {
                e.OrderNo,
                e.OrderType,
                e.TotalLines,
                e.LastModifiedTime,
                e.CreatedTime,
            })
            .ToListAsync();

        var outbound = await outboundRepo.AsQueryable()
            .Where(e => e.OrderStatus == BizConstants.OutboundOrderStatus.COMPLETED)
            .OrderBy(e => e.LastModifiedTime, OrderByType.Desc)
            .Take(top)
            .Select(e => new
            {
                e.OrderNo,
                e.OrderType,
                e.TotalLines,
                e.LastModifiedTime,
                e.CreatedTime,
            })
            .ToListAsync();

        var result = inbound
            .Select(e => new RecentDocumentDto
            {
                Category = "INBOUND",
                OrderNo = e.OrderNo ?? string.Empty,
                OrderType = e.OrderType ?? string.Empty,
                TotalLines = e.TotalLines,
                CompletedTime = e.LastModifiedTime ?? e.CreatedTime,
            })
            .Concat(outbound.Select(e => new RecentDocumentDto
            {
                Category = "OUTBOUND",
                OrderNo = e.OrderNo ?? string.Empty,
                OrderType = e.OrderType ?? string.Empty,
                TotalLines = e.TotalLines,
                CompletedTime = e.LastModifiedTime ?? e.CreatedTime,
            }))
            .OrderByDescending(e => e.CompletedTime)
            .Take(top)
            .ToList();

        return ApiResponse.Ok(result);
    }

    /// <inheritdoc />
    public async Task<ApiResponse> GetInventoryOverviewAsync()
    {
        var headerRepo = unitOfWork.GetRepository<InvInventoryHeader, long>();
        var detailRepo = unitOfWork.GetRepository<InvInventoryDetail, long>();

        // 全部为单位无关的计数指标，避免不同单位（个/千克…）的数量被错误求和
        var headers = await headerRepo.GetAllAsync();
        var details = await detailRepo.GetAllAsync();

        return ApiResponse.Ok(new InventoryOverviewDto
        {
            ContainerCount = headers.Count,
            MaterialTypeCount = details.Select(d => d.MaterialCode).Distinct().Count(),
            DetailCount = details.Count,
            LockedDetailCount = details.Count(d => d.LockedQty > 0),
            StatusAvailable = headers.Count(h => h.InventoryStatus == BizConstants.InventoryStatus.AVAILABLE),
            StatusLocked = headers.Count(h => h.InventoryStatus == BizConstants.InventoryStatus.LOCKED),
            StatusQC = headers.Count(h => h.InventoryStatus == BizConstants.InventoryStatus.QC),
            StatusFrozen = headers.Count(h => h.InventoryStatus == BizConstants.InventoryStatus.FROZEN),
        });
    }

    /// <inheritdoc />
    public async Task<ApiResponse> GetOperationsOverviewAsync()
    {
        var taskRepo = unitOfWork.GetRepository<TaskHeader, long>();
        var locationRepo = unitOfWork.GetRepository<MdLocation, long>();

        // 近7日任务类型分布
        var since = DateTime.Today.AddDays(-6);
        var recentTasks = await taskRepo.GetListAsync(e => e.CreatedTime >= since);
        var taskTypeDistribution = recentTasks
            .Where(e => !string.IsNullOrEmpty(e.TaskType))
            .GroupBy(e => e.TaskType!)
            .Select(g => new TaskTypeStatDto { TaskType = g.Key, Count = g.Count() })
            .OrderByDescending(g => g.Count)
            .ToList();

        var locationTotal = await locationRepo.CountAsync();
        var locationEmpty = await locationRepo.CountAsync(e => e.Status == BizConstants.LocationStatus.EMPTY);
        var locationOccupied = await locationRepo.CountAsync(e => e.Status == BizConstants.LocationStatus.OCCUPIED);
        var locationLocked = await locationRepo.CountAsync(e => e.LockStatus != BizConstants.LocationLockStatus.NONE);
        var locationDisabled = await locationRepo.CountAsync(e => e.IsDisabled == BizConstants.BoolFlag.YES);

        return ApiResponse.Ok(new OperationsOverviewDto
        {
            TaskTypeDistribution = taskTypeDistribution,
            LocationTotal = locationTotal,
            LocationEmpty = locationEmpty,
            LocationOccupied = locationOccupied,
            LocationLocked = locationLocked,
            LocationDisabled = locationDisabled,
        });
    }

    private static List<TrendItemDto> BuildTrend<T>(List<T> items, DateTime startDate, int days)
        where T : Core.Models.Entities.RootEntity
    {
        var result = new List<TrendItemDto>();

        for (var i = 0; i < days; i++)
        {
            var day = startDate.AddDays(i);
            var nextDay = day.AddDays(1);
            var date = day.ToString("MM-dd");

            var count = items.Count(e => e.CreatedTime >= day && e.CreatedTime < nextDay);

            double change = 0;
            if (i > 0)
            {
                var prevDay = startDate.AddDays(i - 1);
                var prevNextDay = prevDay.AddDays(1);
                var prevCount = items.Count(e => e.CreatedTime >= prevDay && e.CreatedTime < prevNextDay);
                change = prevCount > 0 ? Math.Round((double)(count - prevCount) / prevCount * 100, 1) : 0;
            }

            result.Add(new TrendItemDto { Date = date, Value = count, Change = change });
        }

        return result;
    }
}
