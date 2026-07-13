namespace KH.WMS.Modules.DashboardModule.DTOs;

/// <summary>
/// 首页统计卡片数据
/// </summary>
public class HomeStatDto
{
    /// <summary>
    /// 今日入库单数
    /// </summary>
    public int TodayInboundCount { get; set; }

    /// <summary>
    /// 今日出库单数
    /// </summary>
    public int TodayOutboundCount { get; set; }

    /// <summary>
    /// 今日完成任务数（自动化设备吞吐）
    /// </summary>
    public int TodayCompletedTaskCount { get; set; }

    /// <summary>
    /// 执行中任务数（当前自动化活动）
    /// </summary>
    public int InProgressTaskCount { get; set; }

    /// <summary>
    /// 库位使用率（百分比，0-100）
    /// </summary>
    public int LocationUsagePercent { get; set; }

    /// <summary>
    /// 在库容器数（库存头数）
    /// </summary>
    public int ContainerCount { get; set; }
}

/// <summary>
/// 库存概览数据（全部为单位无关的计数指标，避免跨单位求和）
/// </summary>
public class InventoryOverviewDto
{
    /// <summary>
    /// 在库容器数（库存头数）
    /// </summary>
    public int ContainerCount { get; set; }

    /// <summary>
    /// 物料种类数（去重物料编码）
    /// </summary>
    public int MaterialTypeCount { get; set; }

    /// <summary>
    /// 库存明细行数
    /// </summary>
    public int DetailCount { get; set; }

    /// <summary>
    /// 锁定明细行数（LockedQty > 0）
    /// </summary>
    public int LockedDetailCount { get; set; }

    /// <summary>
    /// 可用库存头数（按库存头状态分布）
    /// </summary>
    public int StatusAvailable { get; set; }

    /// <summary>
    /// 锁定库存头数
    /// </summary>
    public int StatusLocked { get; set; }

    /// <summary>
    /// 质检中库存头数
    /// </summary>
    public int StatusQC { get; set; }

    /// <summary>
    /// 冻结库存头数
    /// </summary>
    public int StatusFrozen { get; set; }
}

/// <summary>
/// 趋势数据项
/// </summary>
public class TrendItemDto
{
    /// <summary>
    /// 日期（MM-dd）
    /// </summary>
    public string Date { get; set; } = string.Empty;

    /// <summary>
    /// 数量
    /// </summary>
    public int Value { get; set; }

    /// <summary>
    /// 较前日变化百分比
    /// </summary>
    public double Change { get; set; }
}

/// <summary>
/// 趋势数据
/// </summary>
public class TrendDto
{
    /// <summary>
    /// 入库趋势
    /// </summary>
    public List<TrendItemDto> Inbound { get; set; } = [];

    /// <summary>
    /// 出库趋势
    /// </summary>
    public List<TrendItemDto> Outbound { get; set; } = [];
}
