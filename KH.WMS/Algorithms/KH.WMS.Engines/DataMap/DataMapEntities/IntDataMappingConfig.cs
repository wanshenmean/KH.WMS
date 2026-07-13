using KH.WMS.Core.Models.Entities;
using SqlSugar;
using System.ComponentModel.DataAnnotations;

namespace KH.WMS.Engines.DataMap
{
    /// <summary>
    /// 数据映射配置
    /// 配置WMS与外部系统之间的数据字段映射规则
    /// </summary>
    [SugarTable("int_data_mapping_config")]
    [SugarIndex("uk_mapping_code", nameof(MappingCode), OrderByType.Asc)]
    [SugarIndex("idx_system", nameof(TargetSystem), OrderByType.Asc)]
    public class IntDataMappingConfig : BaseEntity<long>
    {
        /// <summary>
        /// 映射编码
        /// </summary>
        
        [SugarColumn(Length = 50, IsNullable = false, ColumnDescription = "映射编码")]
        public string MappingCode { get; set; } = string.Empty;

        /// <summary>
        /// 映射名称
        /// </summary>
        
        [SugarColumn(Length = 100, IsNullable = false, ColumnDescription = "映射名称")]
        public string MappingName { get; set; } = string.Empty;

        /// <summary>
        /// 源系统
        /// </summary>
        [SugarColumn(Length = 20, IsNullable = true, ColumnDescription = "源系统")]
        public string? SourceSystem { get; set; }

        /// <summary>
        /// 目标系统
        /// </summary>
        [SugarColumn(Length = 20, IsNullable = true, ColumnDescription = "目标系统")]
        public string? TargetSystem { get; set; }

        /// <summary>
        /// 方向
        /// </summary>
        [SugarColumn(Length = 20, IsNullable = true, ColumnDescription = "方向")]
        public string? Direction { get; set; }

        /// <summary>
        /// 映射规则（JSON数组）
        /// </summary>
        
        [SugarColumn(IsNullable = false, ColumnDataType = "TEXT", ColumnDescription = "映射规则(JSON数组)")]
        public string MappingRules { get; set; } = string.Empty;

        /// <summary>
        /// 目标类型（完整类型名，用于反序列化）
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true, ColumnDescription = "目标类型(完整类型名)")]
        public string? TargetType { get; set; }

        /// <summary>
        /// 转换脚本
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDataType = "TEXT", ColumnDescription = "转换脚本")]
        public string? TransformScript { get; set; }

        /// <summary>
        /// 脚本类型（CSHARP/LUA/JAVASCRIPT）
        /// </summary>
        [SugarColumn(Length = 20, IsNullable = true, ColumnDescription = "脚本类型", DefaultValue = DataMapConstants.ScriptLanguage.CSHARP)]
        public string? ScriptType { get; set; }

        /// <summary>
        /// 验证规则（JSON）
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDataType = "TEXT", ColumnDescription = "验证规则(JSON)")]
        public string? ValidationRules { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnDescription = "描述")]
        public string? Description { get; set; }
    }
}
