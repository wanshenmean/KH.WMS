using SqlSugar;
using System.ComponentModel.DataAnnotations;
using KH.WMS.Core.Models.Entities;

namespace KH.WMS.Entities.Warehouse
{
    /// <summary>
    /// 输送线（站台与接驳口的分组关联）
    /// 用于WMS层面判断哪些站台和接驳口在同一条线上（可达性）
    /// 输送线的物理参数（速度、长度、类型等）由WCS管理
    /// </summary>
    [SugarTable("md_conveyor_line")]
    [SugarIndex("uk_warehouse_conveyor", nameof(WarehouseId), OrderByType.Asc, nameof(ConveyorCode), OrderByType.Asc)]
    [SugarIndex("idx_status", nameof(Status), OrderByType.Asc)]
    public class MdConveyorLine : BaseEntity<long>, IEnableDisableEntity
    {
        /// <summary>
        /// 输送线编码
        /// </summary>
        
        [SugarColumn(Length = 20, IsNullable = false, ColumnDescription = "输送线编码")]
        public string ConveyorCode { get; set; } = string.Empty;

        /// <summary>
        /// 输送线名称
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnDescription = "输送线名称")]
        public string? ConveyorName { get; set; }

        /// <summary>
        /// 仓库ID
        /// </summary>
        
        [SugarColumn(IsNullable = false, ColumnDescription = "仓库ID")]
        public long WarehouseId { get; set; }

        /// <summary>
        /// 库区ID（为空表示跨库区输送线）
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "库区ID")]
        public long? ZoneId { get; set; }

        /// <summary>
        /// 状态（1启用 0禁用 2维护中）
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "状态", DefaultValue = "1")]
        public byte Status { get; set; } = 1;

        /// <summary>
        /// 排序号
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "排序号", DefaultValue = "0")]
        public int SortNo { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true, ColumnDescription = "备注")]
        public string? Remark { get; set; }
    }
}
