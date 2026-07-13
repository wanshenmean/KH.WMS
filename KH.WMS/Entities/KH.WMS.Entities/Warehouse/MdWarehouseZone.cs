using SqlSugar;
using System.ComponentModel.DataAnnotations;
using KH.WMS.Core.Models.Entities;

namespace KH.WMS.Entities.Warehouse
{
    /// <summary>
    /// 库区
    /// </summary>
    [SugarTable("md_warehouse_zone")]
    [SugarIndex("uk_zone", nameof(WarehouseId), OrderByType.Asc, nameof(ZoneCode), OrderByType.Asc)]
    [SugarIndex("idx_warehouse", nameof(WarehouseId), OrderByType.Asc)]
    [SugarIndex("idx_parent", nameof(ParentZoneId), OrderByType.Asc)]
    public class MdWarehouseZone : BaseEntity<long>, IEnableDisableEntity
    {

        /// <summary>
        /// 仓库ID
        /// </summary>
        
        [SugarColumn(IsNullable = false, ColumnDescription = "仓库ID")]
        public long WarehouseId { get; set; }

        /// <summary>
        /// 库区编码
        /// </summary>
        
        [SugarColumn(ColumnDataType = "VARCHAR(20)", IsNullable = false, ColumnDescription = "库区编码")]
        public string ZoneCode { get; set; }

        /// <summary>
        /// 库区名称
        /// </summary>
        [SugarColumn(ColumnDataType = "VARCHAR(100)", IsNullable = true, ColumnDescription = "库区名称")]
        public string? ZoneName { get; set; }

        /// <summary>
        /// 库区类型
        /// </summary>
        [SugarColumn(ColumnDataType = "VARCHAR(20)", IsNullable = true, ColumnDescription = "库区类型")]
        public string? ZoneType { get; set; }

        /// <summary>
        /// 父库区ID
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "父库区ID")]
        public long? ParentZoneId { get; set; }

        /// <summary>
        /// ABC分类（A-高频/B-中频/C-低频），库区级默认ABC；
        /// 货位(MdLocation)的AbcClass为空时回退到此值。
        /// </summary>
        [SugarColumn(ColumnDataType = "VARCHAR(2)", IsNullable = true, ColumnDescription = "ABC分类")]
        public string? AbcClass { get; set; }

        /// <summary>
        /// 排序号
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "排序号", DefaultValue = "0")]
        public int SortNo { get; set; } = 0;

        /// <summary>
        /// 状态
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "状态", DefaultValue = "1")]
        public byte Status { get; set; } = 1;

    }
}
