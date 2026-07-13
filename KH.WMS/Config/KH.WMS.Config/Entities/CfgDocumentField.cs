using KH.WMS.Core.Models.Entities;
using SqlSugar;
using System.ComponentModel.DataAnnotations;
using KH.WMS.Core.Database.SqlSugar;

namespace KH.WMS.Config.Abstractions
{
    /// <summary>
    /// 单据类型扩展字段定义
    /// 为不同单据类型配置额外的业务字段，无需修改表结构
    /// </summary>
    [ConfigDb]
    [SugarTable("cfg_document_field")]
    [SugarIndex("idx_doctype", nameof(DocTypeId), OrderByType.Asc)]
    [SugarIndex("idx_field_key", nameof(DocTypeId), OrderByType.Asc, nameof(FieldKey), OrderByType.Asc)]
    public class CfgDocumentField : BaseEntity<long>
    {
        /// <summary>
        /// 单据类型ID
        /// </summary>
        
        [SugarColumn(IsNullable = false, ColumnDescription = "单据类型ID")]
        public long DocTypeId { get; set; }

        /// <summary>
        /// 字段标识（如 FromWarehouseId、ToWarehouseId、ProjectCode）
        /// </summary>
        
        [SugarColumn(Length = 50, IsNullable = false, ColumnDescription = "字段标识")]
        public string FieldKey { get; set; } = string.Empty;

        /// <summary>
        /// 显示名称（如 源仓库、目标仓库、项目编号）
        /// </summary>
        
        [SugarColumn(Length = 100, IsNullable = false, ColumnDescription = "显示名称")]
        public string FieldName { get; set; } = string.Empty;

        /// <summary>
        /// 字段类型（STRING/INT/DECIMAL/DATETIME/BOOLEAN）
        /// </summary>
        [SugarColumn(Length = 20, IsNullable = false, ColumnDescription = "字段类型", DefaultValue = "STRING")]
        public string FieldType { get; set; } = "STRING";

        /// <summary>
        /// 是否参与WMS业务处理（1-是 0-否，仅存储和回传）
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
        /// 作用层级（HEADER-单据头 / LINE-单据行）
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
