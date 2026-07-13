using KH.WMS.Core.Models.Entities;
using SqlSugar;
using System.ComponentModel.DataAnnotations;

namespace KH.WMS.Entities.System
{
    /// <summary>
    /// 系统参数
    /// </summary>
    [SugarTable("sys_parameter")]
    [SugarIndex("uk_param_code", nameof(ParamCode), OrderByType.Asc, true)]
    [SugarIndex("idx_group", nameof(ParamGroup), OrderByType.Asc)]
    public class SysParameter : BaseEntity<long>, IEnableDisableEntity
    {

        /// <summary>
        /// 参数编码
        /// </summary>
        
        [SugarColumn(Length = 50, IsNullable = false, ColumnDescription = "参数编码")]
        public string ParamCode { get; set; } = string.Empty;

        /// <summary>
        /// 参数名称
        /// </summary>
        
        [SugarColumn(Length = 100, IsNullable = false, ColumnDescription = "参数名称")]
        public string ParamName { get; set; } = string.Empty;

        /// <summary>
        /// 参数值
        /// </summary>
        
        [SugarColumn(Length = 500, IsNullable = false, ColumnDescription = "参数值")]
        public string ParamValue { get; set; } = string.Empty;

        /// <summary>
        /// 参数分组
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnDescription = "参数分组")]
        public string? ParamGroup { get; set; }

        /// <summary>
        /// 参数类型
        /// </summary>
        [SugarColumn(Length = 20, IsNullable = false, ColumnDescription = "参数类型", DefaultValue = "STRING")]
        public string ParamType { get; set; } = "STRING";

        /// <summary>
        /// 备注
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnDescription = "备注")]
        public string? Remark { get; set; }

        /// <summary>
        /// 系统内置标识（0=自定义 1=系统内置）
        /// </summary>
        [SugarColumn(Length = 20, IsNullable = false, ColumnDescription = "系统内置标识", DefaultValue = "0")]
        public byte SystemFlag { get; set; } = 0;

        /// <summary>
        /// 状态（0=禁用 1=启用）
        /// </summary>
        [SugarColumn(Length = 20, IsNullable = false, ColumnDescription = "状态", DefaultValue = "1")]
        public byte Status { get; set; } = 1;
    }
}
