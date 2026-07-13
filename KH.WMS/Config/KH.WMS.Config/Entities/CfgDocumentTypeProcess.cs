using SqlSugar;
using System.ComponentModel.DataAnnotations;
using KH.WMS.Core.Models.Entities;
using KH.WMS.Core.Database.SqlSugar;
using KH.WMS.Core.Constants;

namespace KH.WMS.Config.Abstractions
{
    /// <summary>
    /// 单据类型流程控制配置
    /// 配置单据在各环节是否需要审批、收货确认、自动上架/拣货等
    /// 通过 DocTypeId 与 CfgDocumentType 关联，一个单据类型对应一条记录
    /// </summary>
    [ConfigDb]
    [SugarTable("cfg_document_type_process")]
    [SugarIndex("uk_doc_type", nameof(DocTypeId), OrderByType.Asc, true)]
    public class CfgDocumentTypeProcess : BaseEntity<long>
    {

        /// <summary>
        /// 单据类型ID
        /// </summary>
        
        [SugarColumn(IsNullable = false, ColumnDescription = "单据类型ID")]
        public long DocTypeId { get; set; }

        /// <summary>
        /// 初始状态
        /// </summary>
        [SugarColumn(Length = 30, IsNullable = true, ColumnDescription = "初始状态")]
        public string? InitialStatus { get; set; }

        /// <summary>
        /// 需要审批
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "需要审批", DefaultValue = "0")]
        public byte RequireApproval { get; set; } = BoolFlag.NO;

        /// <summary>
        /// 需要收货确认（入库时是否需要收货环节，生产入库等可跳过）
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "需要收货确认", DefaultValue = "1")]
        public byte RequireReceiving { get; set; } = BoolFlag.YES;

        /// <summary>
        /// 自动上架（入库后自动生成上架任务）
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "自动上架", DefaultValue = "1")]
        public byte AutoPutaway { get; set; } = BoolFlag.YES;

        /// <summary>
        /// 自动分配拣货（出库时自动分配库位生成拣货任务）
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "自动分配拣货", DefaultValue = "1")]
        public byte AutoPick { get; set; } = BoolFlag.YES;

        /// <summary>
        /// 自动分配库位
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "自动分配库位", DefaultValue = "0")]
        public byte AutoAllocate { get; set; } = BoolFlag.NO;

        /// <summary>
        /// 自动打印标签
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "自动打印标签", DefaultValue = "0")]
        public byte AutoPrintLabel { get; set; } = BoolFlag.NO;

        /// <summary>
        /// 备注
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnDescription = "备注")]
        public string? Remark { get; set; }
    }
}
