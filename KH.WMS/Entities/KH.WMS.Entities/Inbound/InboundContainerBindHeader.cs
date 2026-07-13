using KH.WMS.Core.Models.Entities;
using KH.WMS.Entities.Constants;
using SqlSugar;
using System.ComponentModel.DataAnnotations;

namespace KH.WMS.Entities.Inbound
{
    /// <summary>
    /// 入库组盘头表
    /// 一个容器一次组盘的汇总信息，包含多种物料明细
    /// 与库存头表的区别：无库位绑定，上架分配库位后转库存
    /// </summary>
    [SugarTable("bd_inbound_container_bind_header")]
    [SugarIndex("idx_container", nameof(ContainerCode), OrderByType.Asc)]
    [SugarIndex("idx_status", nameof(BindStatus), OrderByType.Asc)]
    [SugarIndex("idx_order", nameof(InboundOrderId), OrderByType.Asc)]
    [SugarIndex("idx_source", nameof(SourceType), OrderByType.Asc, nameof(SourceDocNo), OrderByType.Asc)]
    [SugarIndex("idx_warehouse", nameof(WarehouseId), OrderByType.Asc)]
    public class InboundContainerBindHeader : BaseEntity<long>
    {
        /// <summary>
        /// 容器编号
        /// </summary>
        
        [SugarColumn(Length = 50, IsNullable = false, ColumnDescription = "容器编号")]
        public string ContainerCode { get; set; } = string.Empty;

        /// <summary>
        /// 组盘状态（BOUND-已绑定 / PUT_AWAY-已上架 / CANCELLED-已取消）
        /// </summary>
        
        [SugarColumn(Length = 20, IsNullable = false, ColumnDescription = "组盘状态", DefaultValue = "BOUND")]
        public string BindStatus { get; set; } = BizConstants.BindStatus.BOUND;

        /// <summary>
        /// 仓库ID
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "仓库ID")]
        public long? WarehouseId { get; set; }

        /// <summary>
        /// 仓库编码
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnDescription = "仓库编码")]
        public string? WarehouseCode { get; set; }

        /// <summary>
        /// 明细数量（该容器有几种物料）
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "明细数量", DefaultValue = "0")]
        public int DetailCount { get; set; }

        /// <summary>
        /// 来源类型（INBOUND-入库 / TRANSFER-移库 / STOCKTAKE-盘点）
        /// </summary>
        
        [SugarColumn(Length = 20, IsNullable = false, ColumnDescription = "来源类型", DefaultValue = "INBOUND")]
        public string SourceType { get; set; } = BizConstants.SourceTypes.INBOUND;

        /// <summary>
        /// 来源单据号
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnDescription = "来源单据号")]
        public string? SourceDocNo { get; set; }

        /// <summary>
        /// 入库单ID（可空，支持通用组盘）
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "入库单ID")]
        public long? InboundOrderId { get; set; }

        /// <summary>
        /// 质量状态
        /// </summary>
        
        [SugarColumn(Length = 20, IsNullable = false, ColumnDescription = "质量状态", DefaultValue = "PENDING")]
        public string QualityStatus { get; set; } = BizConstants.QualityStatus.PENDING;

        /// <summary>
        /// 组盘时间
        /// </summary>
        
        [SugarColumn(IsNullable = false, ColumnDescription = "组盘时间")]
        public DateTime BindTime { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnDescription = "备注")]
        public string? Remark { get; set; }

        /// <summary>
        /// 组盘明细列表（导航属性，不映射数据库列）
        /// </summary>
        [Navigate(NavigateType.OneToMany, nameof(InboundContainerBindDetail.HeaderId), nameof(Id))]
        public List<InboundContainerBindDetail>? Details { get; set; }
    }
}
