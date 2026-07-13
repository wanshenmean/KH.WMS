using SqlSugar;
using System.ComponentModel.DataAnnotations;
using KH.WMS.Core.Models.Entities;
using KH.WMS.Core.Database.SqlSugar;
using KH.WMS.Core.Constants;

namespace KH.WMS.Config.Abstractions
{
    /// <summary>
    /// 单据类型业务规则配置
    /// 配置单据的操作约束（允许修改/取消/部分执行/多仓等）
    /// 通过 DocTypeId 与 CfgDocumentType 关联，一个单据类型对应一条记录
    /// </summary>
    [ConfigDb]
    [SugarTable("cfg_document_type_rule")]
    [SugarIndex("uk_doc_type", nameof(DocTypeId), OrderByType.Asc, true)]
    public class CfgDocumentTypeRule : BaseEntity<long>
    {

        /// <summary>
        /// 单据类型ID
        /// </summary>
        
        [SugarColumn(IsNullable = false, ColumnDescription = "单据类型ID")]
        public long DocTypeId { get; set; }

        /// <summary>
        /// 允许修改
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "允许修改", DefaultValue = "1")]
        public byte AllowModify { get; set; } = BoolFlag.YES;

        /// <summary>
        /// 允许取消
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "允许取消", DefaultValue = "1")]
        public byte AllowCancel { get; set; } = BoolFlag.YES;

        /// <summary>
        /// 允许部分执行
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "允许部分执行", DefaultValue = "0")]
        public byte AllowPartialExecute { get; set; } = BoolFlag.NO;

        /// <summary>
        /// 允许多仓
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "允许多仓", DefaultValue = "0")]
        public byte AllowMultipleWarehouse { get; set; } = BoolFlag.NO;

        /// <summary>
        /// 备注
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnDescription = "备注")]
        public string? Remark { get; set; }
    }
}
