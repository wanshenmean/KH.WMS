using KH.WMS.Core.Models.Entities;
using SqlSugar;
using System.ComponentModel.DataAnnotations;
using KH.WMS.Entities.System.Enums;

namespace KH.WMS.Entities.System
{
    /// <summary>
    /// 字典类型（支持静态字典项和SQL动态数据源）
    /// </summary>
    [SugarTable("sys_dict_type")]
    [SugarIndex("uk_dict_code", nameof(DictCode), OrderByType.Asc)]
    public class SysDictType : BaseEntity<long>, IEnableDisableEntity
    {
        /// <summary>
        /// 字典编码（如 location_status、port_type、task_type）
        /// </summary>
        
        [SugarColumn(Length = 50, IsNullable = false, ColumnDescription = "字典编码")]
        public string DictCode { get; set; } = string.Empty;

        /// <summary>
        /// 字典名称（如 库位状态、站台类型、任务类型）
        /// </summary>
        
        [SugarColumn(Length = 100, IsNullable = false, ColumnDescription = "字典名称")]
        public string DictName { get; set; } = string.Empty;

        /// <summary>
        /// 数据来源类型（0=静态字典项, 1=SQL动态查询）
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "数据来源类型 0静态 1SQL", DefaultValue = "0")]
        public byte DataSourceType { get; set; } = (byte)DictDataSourceTypeEnum.Static;

        /// <summary>
        /// SQL 查询语句（当 DataSourceType=1 时有效）
        /// 只允许 SELECT 查询，返回的列名需与 ValueColumn/LabelColumn 匹配
        /// 示例：SELECT warehouse_code, warehouse_name FROM wh_warehouse WHERE is_active = 1
        /// </summary>
        [SugarColumn(Length = 2000, IsNullable = true, ColumnDescription = "SQL查询语句")]
        public string? SqlQuery { get; set; }

        /// <summary>
        /// 值列名（SQL查询结果中映射到 ItemValue 的列名）
        /// 示例：warehouse_code
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true, ColumnDescription = "值列名")]
        public string? ValueColumn { get; set; }

        /// <summary>
        /// 标签列名（SQL查询结果中映射到 ItemLabel 的列名）
        /// 示例：warehouse_name
        /// </summary>
        [SugarColumn(Length = 100, IsNullable = true, ColumnDescription = "标签列名")]
        public string? LabelColumn { get; set; }

        /// <summary>
        /// 缓存过期时间（分钟，0=不缓存，默认30分钟）
        /// 仅对SQL动态数据源生效
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "缓存过期时间(分钟)", DefaultValue = "30")]
        public int CacheMinutes { get; set; } = 30;

        /// <summary>
        /// 是否启用
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "是否启用", DefaultValue = "1")]
        public byte IsActive { get; set; } = 1;

        /// <summary>
        /// 备注
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnDescription = "备注")]
        public string? Remark { get; set; }
    }
}
