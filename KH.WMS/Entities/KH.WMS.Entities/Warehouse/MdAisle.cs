using SqlSugar;
using System.ComponentModel.DataAnnotations;
using KH.WMS.Core.Models.Entities;

namespace KH.WMS.Entities.Warehouse
{
    /// <summary>
    /// 巷道（高位立库中堆垛机运行的通道）
    /// 一条巷道对应一台堆垛机，两侧各一排货位
    /// </summary>
    [SugarTable("md_aisle")]
    [SugarIndex("uk_warehouse_aisle", nameof(WarehouseId), OrderByType.Asc, nameof(AisleNo), OrderByType.Asc)]
    [SugarIndex("idx_zone", nameof(ZoneId), OrderByType.Asc)]
    [SugarIndex("idx_equipment", nameof(EquipmentCode), OrderByType.Asc)]
    [SugarIndex("idx_status", nameof(Status), OrderByType.Asc)]
    public class MdAisle : BaseEntity<long>, IEnableDisableEntity
    {
        /// <summary>
        /// 巷道号（仓库内唯一编号）
        /// </summary>
        
        [SugarColumn(IsNullable = false, ColumnDescription = "巷道号")]
        public int AisleNo { get; set; }

        /// <summary>
        /// 巷道编码
        /// </summary>
        
        [SugarColumn(Length = 20, IsNullable = false, ColumnDescription = "巷道编码")]
        public string AisleCode { get; set; } = string.Empty;

        /// <summary>
        /// 巷道名称
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnDescription = "巷道名称")]
        public string? AisleName { get; set; }

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
        /// 关联堆垛机设备编码
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnDescription = "关联堆垛机设备编码")]
        public string? EquipmentCode { get; set; }

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
