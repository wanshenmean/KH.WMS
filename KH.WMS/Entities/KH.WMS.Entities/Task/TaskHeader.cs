using SqlSugar;
using System.ComponentModel.DataAnnotations;
using KH.WMS.Core.Models.Entities;
using KH.WMS.Entities.Constants;

namespace KH.WMS.Entities.Task
{
    /// <summary>
    /// 任务头
    /// 自动化立库以整托盘为搬运单元，任务结果为二元（整托成功/整托失败）。
    /// </summary>
    [SugarTable("bd_task_header")]
    [SugarIndex("uk_task_no", nameof(TaskNo), OrderByType.Asc)]
    [SugarIndex("idx_type_status", nameof(TaskType), OrderByType.Asc, nameof(TaskStatus), OrderByType.Asc)]
    [SugarIndex("idx_doc", nameof(DocType), OrderByType.Asc, nameof(DocNo), OrderByType.Asc)]
    [SugarIndex("idx_equipment", nameof(EquipmentCode), OrderByType.Asc, nameof(TaskStatus), OrderByType.Asc)]
    [SugarIndex("idx_from_location", nameof(FromLocationId), OrderByType.Asc)]
    [SugarIndex("idx_to_location", nameof(ToLocationId), OrderByType.Asc)]
    [SugarIndex("idx_container", nameof(ContainerId), OrderByType.Asc)]
    public class TaskHeader : BaseEntity<long>
    {
        /// <summary>
        /// 任务编号
        /// </summary>
        
        [SugarColumn(Length = 50, IsNullable = false, ColumnDescription = "任务编号")]
        public string TaskNo { get; set; } = string.Empty;

        /// <summary>
        /// 任务类型
        /// </summary>
        
        [SugarColumn(Length = 20, IsNullable = false, ColumnDescription = "任务类型")]
        public string TaskType { get; set; } = string.Empty;

        /// <summary>
        /// 任务优先级
        /// </summary>
        [SugarColumn(Length = 20, IsNullable = false, ColumnDescription = "任务优先级", DefaultValue = "NORMAL")]
        public string TaskPriority { get; set; } = BizConstants.TaskPriority.NORMAL;

        /// <summary>
        /// 仓库ID
        /// </summary>
        
        [SugarColumn(IsNullable = false, ColumnDescription = "仓库ID")]
        public long WarehouseId { get; set; }

        /// <summary>
        /// 库区ID
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "库区ID")]
        public long? ZoneId { get; set; }

        /// <summary>
        /// 库区类型
        /// </summary>
        [SugarColumn(Length = 20, IsNullable = true, ColumnDescription = "库区类型")]
        public string? ZoneType { get; set; }

        /// <summary>
        /// 单据ID
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "单据ID")]
        public long? DocId { get; set; }

        /// <summary>
        /// 单据类型
        /// </summary>
        [SugarColumn(Length = 30, IsNullable = true, ColumnDescription = "单据类型")]
        public string? DocType { get; set; }

        /// <summary>
        /// 单据编号
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnDescription = "单据编号")]
        public string? DocNo { get; set; }

        /// <summary>
        /// 任务状态
        /// </summary>
        [SugarColumn(Length = 20, IsNullable = false, ColumnDescription = "任务状态", DefaultValue = "PENDING")]
        public string TaskStatus { get; set; } = BizConstants.TaskStatus.PENDING;

        /// <summary>
        /// 执行模式（AUTO-设备自动执行 / MANUAL-人工通过终端操作设备）
        /// </summary>
        [SugarColumn(Length = 20, IsNullable = false, ColumnDescription = "执行模式", DefaultValue = "AUTO")]
        public string ExecutionMode { get; set; } = BizConstants.ExecutionMode.AUTO;

        /// <summary>
        /// 确认方式（AUTO-设备自动确认 / MANUAL-人工确认）
        /// </summary>
        [SugarColumn(Length = 20, IsNullable = false, ColumnDescription = "确认方式", DefaultValue = "AUTO")]
        public string ConfirmType { get; set; } = BizConstants.ConfirmTypes.AUTO;

        /// <summary>
        /// 执行设备编码（堆垛机/AGV/输送线，由 WCS 分配）
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnDescription = "执行设备编码")]
        public string? EquipmentCode { get; set; }

        /// <summary>
        /// WCS侧任务编号，用于WMS与WCS双向追溯和回调匹配
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnDescription = "WCS任务编号")]
        public string? WcsTaskNo { get; set; }

        /// <summary>
        /// 容器ID（一个任务对应一个托盘/容器）
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "容器ID")]
        public long? ContainerId { get; set; }

        /// <summary>
        /// 容器编号
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnDescription = "容器编号")]
        public string? ContainerNo { get; set; }

        /// <summary>
        /// 起始库位ID（可空，当起始位置为输送线入库口等非库位时为空）
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "起始库位ID")]
        public long? FromLocationId { get; set; }

        /// <summary>
        /// 起始位置编码（库位编码或输送线站台编码）
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnDescription = "起始位置编码")]
        public string? FromLocationCode { get; set; }

        /// <summary>
        /// 起始库区ID
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "起始库区ID")]
        public long? FromZoneId { get; set; }

        /// <summary>
        /// 目标库位ID
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "目标库位ID")]
        public long? ToLocationId { get; set; }

        /// <summary>
        /// 目标库位编码
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnDescription = "目标库位编码")]
        public string? ToLocationCode { get; set; }

        /// <summary>
        /// 目标库区ID
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "目标库区ID")]
        public long? ToZoneId { get; set; }

        /// <summary>
        /// 实际货位是否已分配（0-未分配/目标为接驳口 1-已分配实际货位）
        /// 两阶段上架模式：Phase 1 创建任务时 ToLocationId=接驳口ID，LocationAllocated=0；
        /// Phase 2 在接驳口分配实际货位后 ToLocationId=实际货位ID，LocationAllocated=1
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "实际货位是否已分配", DefaultValue = "0")]
        public byte LocationAllocated { get; set; } = 0;

        /// <summary>
        /// 开始时间
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "开始时间")]
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// 完成时间
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "完成时间")]
        public DateTime? CompleteTime { get; set; }

        /// <summary>
        /// 总行数（托盘上物料种类数，用于展示）
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "总行数", DefaultValue = "0")]
        public int TotalLines { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnDescription = "备注")]
        public string? Remark { get; set; }

        // === 导航属性 ===

        /// <summary>
        /// 任务行列表（导航属性，不映射数据库列）
        /// </summary>
        [Navigate(NavigateType.OneToMany, nameof(TaskLine.TaskId), nameof(Id))]
        public List<TaskLine>? Lines { get; set; }

        // === 领域方法 ===

        /// <summary>
        /// 是否允许取消（计算属性，不映射数据库列）
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public bool CanCancel => TaskStatus == BizConstants.TaskStatus.PENDING;

        /// <summary>
        /// 标记为执行中
        /// </summary>
        public void MarkAsInProgress()
        {
            if (TaskStatus == BizConstants.TaskStatus.PENDING)
            {
                TaskStatus = BizConstants.TaskStatus.IN_PROGRESS;
                StartTime = DateTime.Now;
            }
        }

        /// <summary>
        /// 标记为已完成
        /// </summary>
        public void MarkAsCompleted()
        {
            TaskStatus = BizConstants.TaskStatus.COMPLETED;
            CompleteTime = DateTime.Now;
        }

        /// <summary>
        /// 标记为失败
        /// </summary>
        public void MarkAsFailed()
        {
            TaskStatus = BizConstants.TaskStatus.FAILED;
        }

        /// <summary>
        /// 取消任务
        /// </summary>
        public void Cancel()
        {
            if (CanCancel)
                TaskStatus = BizConstants.TaskStatus.CANCELLED;
        }
    }
}
