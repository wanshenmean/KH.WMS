namespace KH.WMS.Modules.DashboardModule.DTOs;

/// <summary>
/// 作业概览（任务类型分布 + 库位状态）
/// </summary>
public class OperationsOverviewDto
{
    /// <summary>
    /// 任务类型分布（近7日）
    /// </summary>
    public List<TaskTypeStatDto> TaskTypeDistribution { get; set; } = [];

    /// <summary>
    /// 总库位数
    /// </summary>
    public int LocationTotal { get; set; }

    /// <summary>
    /// 空闲库位数
    /// </summary>
    public int LocationEmpty { get; set; }

    /// <summary>
    /// 占用库位数
    /// </summary>
    public int LocationOccupied { get; set; }

    /// <summary>
    /// 锁定库位数（LockStatus != 0）
    /// </summary>
    public int LocationLocked { get; set; }

    /// <summary>
    /// 禁用库位数（IsDisabled = 1）
    /// </summary>
    public int LocationDisabled { get; set; }
}

/// <summary>
/// 任务类型统计项
/// </summary>
public class TaskTypeStatDto
{
    /// <summary>
    /// 任务类型
    /// </summary>
    public string TaskType { get; set; } = string.Empty;

    /// <summary>
    /// 数量
    /// </summary>
    public int Count { get; set; }
}
