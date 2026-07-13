using SqlSugar;
using System.ComponentModel.DataAnnotations;
using KH.WMS.Core.Models.Entities;
using KH.WMS.Core.Database.SqlSugar;

namespace KH.WMS.Config.Abstractions
{
    /// <summary>
    /// 单据状态配置
    /// 状态分类由 IsInitial/IsFinal/IsCancel 标记推导：
    ///   IsInitial=1 → 初始态, IsFinal=1 → 终态, IsCancel=1 → 取消态, 其余 → 流程中
    /// </summary>
    [ConfigDb]
    [SugarTable("cfg_document_status")]
    [SugarIndex("idx_doc_type", nameof(DocTypeId), OrderByType.Asc)]
    public class CfgDocumentStatus : BaseEntity<long>, IEnableDisableEntity
    {

        /// <summary>
        /// 单据类型ID
        /// </summary>
        
        [SugarColumn(IsNullable = false, ColumnDescription = "单据类型ID")]
        public long DocTypeId { get; set; }

        /// <summary>
        /// 状态编码
        /// </summary>
        
        [SugarColumn(Length = 30, IsNullable = false, ColumnDescription = "状态编码")]
        public string StatusCode { get; set; } = string.Empty;

        /// <summary>
        /// 状态名称
        /// </summary>
        
        [SugarColumn(Length = 100, IsNullable = false, ColumnDescription = "状态名称")]
        public string StatusName { get; set; } = string.Empty;

        /// <summary>
        /// 是否初始状态
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "是否初始状态", DefaultValue = "0")]
        public byte IsInitial { get; set; } = 0;

        /// <summary>
        /// 是否终态
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "是否终态", DefaultValue = "0")]
        public byte IsFinal { get; set; } = 0;

        /// <summary>
        /// 是否取消状态
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "是否取消状态", DefaultValue = "0")]
        public byte IsCancel { get; set; } = 0;

        /// <summary>
        /// 允许编辑
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "允许编辑", DefaultValue = "1")]
        public byte AllowEdit { get; set; } = 1;

        /// <summary>
        /// 允许删除
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "允许删除", DefaultValue = "1")]
        public byte AllowDelete { get; set; } = 1;

        /// <summary>
        /// 颜色
        /// </summary>
        [SugarColumn(Length = 10, IsNullable = true, ColumnDescription = "颜色")]
        public string? Color { get; set; }

        /// <summary>
        /// 排序号
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "排序号", DefaultValue = "0")]
        public int SortNo { get; set; } = 0;

        /// <summary>
        /// 描述
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnDescription = "描述")]
        public string? Description { get; set; }

        /// <summary>
        /// 可流转到的目标状态（JSON数组，如 ["RECEIVING","CANCELLED"]）
        /// </summary>
        [SugarColumn(ColumnDataType = "VARCHAR(500)", IsNullable = true, ColumnDescription = "可流转到的目标状态")]
        public string? NextStatuses { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "是否启用", DefaultValue = "1")]
        public byte IsActive { get; set; } = 1;

        /// <summary>
        /// 推导状态分类（不映射数据库，由 IsInitial/IsFinal/IsCancel 计算）
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public string StatusCategory =>
            IsCancel == 1 ? "CANCEL" :
            IsFinal == 1 ? "TERMINAL" :
            IsInitial == 1 ? "INITIAL" :
            "PROCESS";
    }
}
