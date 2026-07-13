using SqlSugar;
using System.ComponentModel.DataAnnotations;
using KH.WMS.Core.Models.Entities;

namespace KH.WMS.Entities.Warehouse
{
    /// <summary>
    /// 逻辑分区-物理库区映射
    /// 一个逻辑分区可以包含多个物理库区，每个映射有优先级
    /// </summary>
    [SugarTable("md_logical_zone_mapping")]
    [SugarIndex("idx_logical_zone", nameof(LogicalZoneId), OrderByType.Asc)]
    [SugarIndex("idx_physical_zone", nameof(PhysicalZoneId), OrderByType.Asc)]
    public class MdLogicalZoneMapping : BaseEntity<long>
    {
        /// <summary>
        /// 逻辑分区ID
        /// </summary>
        
        [SugarColumn(IsNullable = false, ColumnDescription = "逻辑分区ID")]
        public long LogicalZoneId { get; set; }

        /// <summary>
        /// 物理库区ID
        /// </summary>
        
        [SugarColumn(IsNullable = false, ColumnDescription = "物理库区ID")]
        public long PhysicalZoneId { get; set; }

        /// <summary>
        /// 优先级（数字越小越优先，用于出库分配时的库区选择顺序）
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "优先级", DefaultValue = "0")]
        public int Priority { get; set; } = 0;

        /// <summary>
        /// 状态（1启用 0禁用）
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "状态", DefaultValue = "1")]
        public byte Status { get; set; } = 1;
    }
}
