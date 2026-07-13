using KH.WMS.Core.Models.Entities;
using SqlSugar;
using System.ComponentModel.DataAnnotations;
using KH.WMS.Core.Database.SqlSugar;

namespace KH.WMS.Config.Abstractions
{
    /// <summary>
    /// 扩展字段实体类型注册
    /// 用于非单据类实体（库存操作、基础数据等）的扩展字段配置
    /// </summary>
    [ConfigDb]
    [SugarTable("cfg_ext_field_type")]
    [SugarIndex("uk_entity_code", nameof(EntityCode), OrderByType.Asc, true)]
    [SugarIndex("idx_category", nameof(EntityCategory), OrderByType.Asc)]
    public class CfgExtFieldType : BaseEntity<long>, IEnableDisableEntity
    {
        /// <summary>
        /// 实体编码（如 INV_ADJUST、INV_STOCKTAKE、MD_MATERIAL）
        /// </summary>
        
        [SugarColumn(Length = 50, IsNullable = false, ColumnDescription = "实体编码")]
        public string EntityCode { get; set; } = string.Empty;

        /// <summary>
        /// 实体名称（如 库存调整、盘点、物料）
        /// </summary>
        
        [SugarColumn(Length = 100, IsNullable = false, ColumnDescription = "实体名称")]
        public string EntityName { get; set; } = string.Empty;

        /// <summary>
        /// 实体分类（INVENTORY-库存操作 / BASEDATA-基础数据）
        /// </summary>
        
        [SugarColumn(Length = 20, IsNullable = false, ColumnDescription = "实体分类")]
        public string EntityCategory { get; set; } = string.Empty;

        /// <summary>
        /// 是否有行级别扩展字段（1是 0否）
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "是否有行级别扩展字段", DefaultValue = "0")]
        public byte HasLineLevel { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "是否启用", DefaultValue = "1")]
        public byte IsActive { get; set; } = 1;

        /// <summary>
        /// 排序
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "排序")]
        public int? SortOrder { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnDescription = "备注")]
        public string? Remark { get; set; }
    }
}
