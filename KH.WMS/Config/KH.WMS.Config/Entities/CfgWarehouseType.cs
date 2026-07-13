using SqlSugar;
using System.ComponentModel.DataAnnotations;
using KH.WMS.Core.Models.Entities;
using KH.WMS.Core.Database.SqlSugar;

namespace KH.WMS.Config.Abstractions
{
    /// <summary>
    /// 仓库类型
    /// 定义仓库的业务分类（如原材料仓、成品仓、半成品仓、退货仓等）
    /// </summary>
    [ConfigDb]
    [SugarTable("cfg_warehouse_type")]
    [SugarIndex("uk_type_code", nameof(TypeCode), OrderByType.Asc)]
    public class CfgWarehouseType : BaseEntity<long>, IEnableDisableEntity
    {
        /// <summary>
        /// 类型编码（如 RAW_MATERIAL、FINISHED_GOODS、SEMI_FINISHED、RETURN）
        /// </summary>
        
        [SugarColumn(Length = 20, IsNullable = false, ColumnDescription = "类型编码")]
        public string TypeCode { get; set; } = string.Empty;

        /// <summary>
        /// 类型名称
        /// </summary>
        
        [SugarColumn(Length = 50, IsNullable = false, ColumnDescription = "类型名称")]
        public string TypeName { get; set; } = string.Empty;

        /// <summary>
        /// 颜色标识（前端展示用）
        /// </summary>
        [SugarColumn(Length = 20, IsNullable = true, ColumnDescription = "颜色标识")]
        public string? Color { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true, ColumnDescription = "描述")]
        public string? Description { get; set; }

        /// <summary>
        /// 排序号
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "排序号", DefaultValue = "0")]
        public int SortNo { get; set; }

        /// <summary>
        /// 状态（1启用 0禁用）
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "状态", DefaultValue = "1")]
        public byte Status { get; set; } = 1;
    }
}
