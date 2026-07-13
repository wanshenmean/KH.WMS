using KH.WMS.Core.Models.Entities;
using SqlSugar;
using System.ComponentModel.DataAnnotations;
using KH.WMS.Core.Database.SqlSugar;

namespace KH.WMS.Config.Abstractions
{
    /// <summary>
    /// 扩展字段定义（非单据类）
    /// 为库存操作、基础数据等实体配置额外字段
    /// </summary>
    [ConfigDb]
    [SugarTable("cfg_ext_field")]
    [SugarIndex("idx_entity_type", nameof(EntityTypeId), OrderByType.Asc)]
    [SugarIndex("idx_entity_field_key", nameof(EntityTypeId), OrderByType.Asc, nameof(FieldKey), OrderByType.Asc)]
    public class CfgExtField : BaseEntity<long>
    {
        /// <summary>
        /// 实体类型ID
        /// </summary>
        
        [SugarColumn(IsNullable = false, ColumnDescription = "实体类型ID")]
        public long EntityTypeId { get; set; }

        /// <summary>
        /// 字段标识（如 originWarehouse、stocktakeOrg）
        /// </summary>
        
        [SugarColumn(Length = 50, IsNullable = false, ColumnDescription = "字段标识")]
        public string FieldKey { get; set; } = string.Empty;

        /// <summary>
        /// 显示名称
        /// </summary>
        
        [SugarColumn(Length = 100, IsNullable = false, ColumnDescription = "显示名称")]
        public string FieldName { get; set; } = string.Empty;

        /// <summary>
        /// 字段类型（STRING/INT/DECIMAL/DATETIME/BOOLEAN）
        /// </summary>
        [SugarColumn(Length = 20, IsNullable = false, ColumnDescription = "字段类型", DefaultValue = "STRING")]
        public string FieldType { get; set; } = "STRING";

        /// <summary>
        /// 是否参与WMS业务处理（1-是 0-否）
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "是否参与业务处理", DefaultValue = "0")]
        public byte IsProcessable { get; set; }

        /// <summary>
        /// 是否必填
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "是否必填", DefaultValue = "0")]
        public byte IsRequired { get; set; }

        /// <summary>
        /// 默认值
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true, ColumnDescription = "默认值")]
        public string? DefaultValue { get; set; }

        /// <summary>
        /// 作用层级（HEADER-主表 / LINE-子表）
        /// </summary>
        [SugarColumn(Length = 10, IsNullable = false, ColumnDescription = "作用层级", DefaultValue = "HEADER")]
        public string FieldLevel { get; set; } = "HEADER";

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
