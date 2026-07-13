using SqlSugar;
using System.ComponentModel.DataAnnotations;
using KH.WMS.Core.Models.Entities;
using KH.WMS.Core.Database.SqlSugar;

namespace KH.WMS.Config.Abstractions
{
    /// <summary>
    /// 库位状态
    /// 定义库位的生命周期状态及流转规则（如空闲、占用、预留、锁定、禁用等）
    /// </summary>
    [ConfigDb]
    [SugarTable("cfg_location_status")]
    [SugarIndex("uk_status_code", nameof(StatusCode), OrderByType.Asc)]
    public class CfgLocationStatus : BaseEntity<long>, IEnableDisableEntity
    {
        /// <summary>
        /// 状态编码（如 EMPTY、OCCUPIED、RESERVED、LOCKED、DISABLED）
        /// </summary>
        
        [SugarColumn(Length = 20, IsNullable = false, ColumnDescription = "状态编码")]
        public string StatusCode { get; set; } = string.Empty;

        /// <summary>
        /// 状态名称
        /// </summary>
        
        [SugarColumn(Length = 50, IsNullable = false, ColumnDescription = "状态名称")]
        public string StatusName { get; set; } = string.Empty;

        /// <summary>
        /// 状态分类（AVAILABLE-可用 / UNAVAILABLE-不可用）
        /// </summary>
        [SugarColumn(Length = 20, IsNullable = false, ColumnDescription = "状态分类", DefaultValue = "AVAILABLE")]
        public string StatusCategory { get; set; } = "AVAILABLE";

        /// <summary>
        /// 是否允许上架（0-不允许 1-允许）
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "是否允许上架", DefaultValue = "0")]
        public byte AllowPutaway { get; set; } = 0;

        /// <summary>
        /// 是否允许下架（0-不允许 1-允许）
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "是否允许下架", DefaultValue = "0")]
        public byte AllowPicking { get; set; } = 0;

        /// <summary>
        /// 是否允许移库（0-不允许 1-允许）
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "是否允许移库", DefaultValue = "0")]
        public byte AllowTransfer { get; set; } = 0;

        /// <summary>
        /// 允许的前置状态（多个状态编码用逗号分隔，如 EMPTY,LOCKED）
        /// 留空表示不限制转入来源
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true, ColumnDescription = "允许的前置状态")]
        public string? AllowedFromStatuses { get; set; }

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
