using SqlSugar;
using System.ComponentModel.DataAnnotations;
using KH.WMS.Core.Models.Entities;

namespace KH.WMS.Entities.Warehouse
{
    /// <summary>
    /// 逻辑分区
    /// 将多个物理库区映射为一个逻辑区域，用于出库分配策略
    /// 例如：多个 STORAGE 库区 → 一个"存储逻辑区"
    /// </summary>
    [SugarTable("md_logical_zone")]
    [SugarIndex("uk_code", nameof(ZoneCode), OrderByType.Asc, IsUnique = true)]
    [SugarIndex("idx_warehouse", nameof(WarehouseId), OrderByType.Asc)]
    public class MdLogicalZone : BaseEntity<long>, IEnableDisableEntity
    {
        /// <summary>
        /// 逻辑分区编码
        /// </summary>
        
        [SugarColumn(ColumnDataType = "VARCHAR(30)", IsNullable = false, ColumnDescription = "逻辑分区编码")]
        public string ZoneCode { get; set; } = string.Empty;

        /// <summary>
        /// 逻辑分区名称
        /// </summary>
        [SugarColumn(ColumnDataType = "VARCHAR(100)", IsNullable = true, ColumnDescription = "逻辑分区名称")]
        public string? ZoneName { get; set; }

        /// <summary>
        /// 逻辑分区类型（PICKING-拣选区 / STORAGE-存储区 / STAGING-暂存区 / QC-质检区）
        /// </summary>
        [SugarColumn(ColumnDataType = "VARCHAR(20)", IsNullable = true, ColumnDescription = "逻辑分区类型")]
        public string? ZoneType { get; set; }

        /// <summary>
        /// 仓库ID
        /// </summary>
        
        [SugarColumn(IsNullable = false, ColumnDescription = "仓库ID")]
        public long WarehouseId { get; set; }

        /// <summary>
        /// 排序号
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "排序号", DefaultValue = "0")]
        public int SortNo { get; set; } = 0;

        /// <summary>
        /// 状态（1启用 0禁用）
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "状态", DefaultValue = "1")]
        public byte Status { get; set; } = 1;

        /// <summary>
        /// 备注
        /// </summary>
        [SugarColumn(ColumnDataType = "VARCHAR(500)", IsNullable = true, ColumnDescription = "备注")]
        public string? Remark { get; set; }

        /// <summary>
        /// 物理库区映射列表（导航属性）
        /// </summary>
        [Navigate(NavigateType.OneToMany, nameof(MdLogicalZoneMapping.LogicalZoneId), nameof(Id))]
        public List<MdLogicalZoneMapping>? Mappings { get; set; }
    }
}
