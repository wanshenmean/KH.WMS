using SqlSugar;
using System.ComponentModel.DataAnnotations;
using KH.WMS.Core.Models.Entities;
using KH.WMS.Core.Database.SqlSugar;

namespace KH.WMS.Config.Abstractions
{
    /// <summary>
    /// 单据类型配置
    /// 定义单据类型的基础信息，流程控制和业务规则通过子表配置
    /// </summary>
    [ConfigDb]
    [SugarTable("cfg_document_type")]
    [SugarIndex("uk_type_code", nameof(TypeCode), OrderByType.Asc, true)]
    [SugarIndex("idx_category", nameof(TypeCategory), OrderByType.Asc)]
    public class CfgDocumentType : BaseEntity<long>, IEnableDisableEntity
    {

        /// <summary>
        /// 类型编码（如 PURCHASE_IN、PRODUCTION_IN、SALE_OUT、PRODUCTION_PICK）
        /// </summary>
        
        [SugarColumn(Length = 50, IsNullable = false, ColumnDescription = "类型编码")]
        public string TypeCode { get; set; } = string.Empty;

        /// <summary>
        /// 类型名称
        /// </summary>
        
        [SugarColumn(Length = 100, IsNullable = false, ColumnDescription = "类型名称")]
        public string TypeName { get; set; } = string.Empty;

        /// <summary>
        /// 类型分类（INBOUND/OUTBOUND/TRANSFER/INVENTORY）
        /// </summary>
        
        [SugarColumn(Length = 20, IsNullable = false, ColumnDescription = "类型分类")]
        public string TypeCategory { get; set; } = string.Empty;

        /// <summary>
        /// 编号规则ID
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "编号规则ID")]
        public long? NumberRuleId { get; set; }

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
        /// 描述
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnDescription = "描述")]
        public string? Description { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnDescription = "备注")]
        public string? Remark { get; set; }
    }
}
