using SqlSugar;
using System.ComponentModel.DataAnnotations;
using KH.WMS.Core.Models.Entities;

namespace KH.WMS.Entities.Warehouse
{
    /// <summary>
    /// 仓库
    /// </summary>
    [SugarTable("md_warehouse")]
    [SugarIndex("uk_warehouse_code", nameof(WarehouseCode), OrderByType.Asc)]
    public class MdWarehouse : BaseEntity<long>, IEnableDisableEntity
    {

        /// <summary>
        /// 仓库编码
        /// </summary>
        
        [SugarColumn(ColumnDataType = "VARCHAR(20)", IsNullable = false, ColumnDescription = "仓库编码")]
        public string WarehouseCode { get; set; }

        /// <summary>
        /// 仓库名称
        /// </summary>
        
        [SugarColumn(ColumnDataType = "VARCHAR(100)", IsNullable = false, ColumnDescription = "仓库名称")]
        public string WarehouseName { get; set; }

        /// <summary>
        /// 仓库类型
        /// </summary>
        [SugarColumn(ColumnDataType = "VARCHAR(20)", IsNullable = true, ColumnDescription = "仓库类型")]
        public string? WarehouseType { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "状态", DefaultValue = "1")]
        public byte Status { get; set; } = 1;

        /// <summary>
        /// 备注
        /// </summary>
        [SugarColumn(ColumnDataType = "VARCHAR(500)", IsNullable = true, ColumnDescription = "备注")]
        public string? Remark { get; set; }
    }
}
