using SqlSugar;
using System.ComponentModel.DataAnnotations;
using KH.WMS.Core.Models.Entities;

namespace KH.WMS.Entities.Task
{
    /// <summary>
    /// 任务行（托盘上的物料信息，用于追溯查看，不跟踪物料级别的完成进度）
    /// </summary>
    [SugarTable("bd_task_line")]
    [SugarIndex("uk_task_line", nameof(TaskId), OrderByType.Asc, nameof(LineNo), OrderByType.Asc)]
    [SugarIndex("idx_task", nameof(TaskId), OrderByType.Asc)]
    [SugarIndex("idx_inventory", nameof(InventoryHeaderId), OrderByType.Asc)]
    public class TaskLine : BaseEntity<long>
    {
        /// <summary>
        /// 任务ID
        /// </summary>
        
        [SugarColumn(IsNullable = false, ColumnDescription = "任务ID")]
        public long TaskId { get; set; }

        /// <summary>
        /// 行号
        /// </summary>
        
        [SugarColumn(IsNullable = false, ColumnDescription = "行号")]
        public int LineNo { get; set; }

        /// <summary>
        /// 物料ID
        /// </summary>
        
        [SugarColumn(IsNullable = false, ColumnDescription = "物料ID")]
        public long MaterialId { get; set; }

        /// <summary>
        /// 物料编码
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnDescription = "物料编码")]
        public string? MaterialCode { get; set; }

        /// <summary>
        /// 物料名称
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true, ColumnDescription = "物料名称")]
        public string? MaterialName { get; set; }

        /// <summary>
        /// 库存头ID（关联库存记录，用于任务→库存的追溯链）
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "库存头ID")]
        public long? InventoryHeaderId { get; set; }

        /// <summary>
        /// 批次号
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnDescription = "批次号")]
        public string? BatchNo { get; set; }

        /// <summary>
        /// 排序序号
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "排序序号")]
        public int? SortSeq { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnDescription = "备注")]
        public string? Remark { get; set; }

        // === 导航属性 ===

        /// <summary>
        /// 所属任务头（ManyToOne 导航）
        /// </summary>
        [Navigate(NavigateType.ManyToOne, nameof(TaskId), nameof(TaskHeader.Id))]
        public TaskHeader? Header { get; set; }
    }
}
