using SqlSugar;
using System.ComponentModel.DataAnnotations;
using KH.WMS.Core.Models.Entities;
using KH.WMS.Core.Database.SqlSugar;

namespace KH.WMS.Config.Abstractions
{
    /// <summary>
    /// 接驳口类型
    /// 定义接驳口的业务用途分类（如入库接驳、出库接驳、混合等）
    /// </summary>
    [ConfigDb]
    [SugarTable("cfg_transfer_point_type")]
    [SugarIndex("uk_type_code", nameof(TypeCode), OrderByType.Asc)]
    public class CfgTransferPointType : BaseEntity<long>, IEnableDisableEntity
    {
        /// <summary>
        /// 类型编码（如 INBOUND、OUTBOUND、MIXED）
        /// </summary>
        
        [SugarColumn(Length = 20, IsNullable = false, ColumnDescription = "类型编码")]
        public string TypeCode { get; set; } = string.Empty;

        /// <summary>
        /// 类型名称
        /// </summary>
        
        [SugarColumn(Length = 50, IsNullable = false, ColumnDescription = "类型名称")]
        public string TypeName { get; set; } = string.Empty;

        /// <summary>
        /// 是否入库接驳（0-否 1-是）
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "是否入库接驳", DefaultValue = "0")]
        public byte AllowInbound { get; set; } = 0;

        /// <summary>
        /// 是否出库接驳（0-否 1-是）
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "是否出库接驳", DefaultValue = "0")]
        public byte AllowOutbound { get; set; } = 0;

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
