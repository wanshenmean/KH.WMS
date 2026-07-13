using KH.WMS.Core.Models.Entities;
using KH.WMS.Entities.Constants;
using SqlSugar;
using System.ComponentModel.DataAnnotations;

namespace KH.WMS.Entities.Outbound
{
    /// <summary>
    /// 出库分配头表
    /// 一次库存分配的汇总信息，对应一张出库单的分配结果。
    /// 分配过程中系统按出库策略（FIFO/FEFO等）从现有库存中锁定具体库位和批次，
    /// 生成的分配明细记录了每行物料从哪个库位、哪个容器取多少数量。
    /// 与入库组盘表对称：入库组盘记录"物料放入容器"，出库分配记录"从库位容器取出物料"。
    /// </summary>
    [SugarTable("bd_outbound_allocation_header")]
    [SugarIndex("idx_status", nameof(AllocStatus), OrderByType.Asc)]
    [SugarIndex("idx_order", nameof(OutboundOrderId), OrderByType.Asc)]
    [SugarIndex("idx_order_no", nameof(OutboundOrderNo), OrderByType.Asc)]
    [SugarIndex("idx_warehouse", nameof(WarehouseId), OrderByType.Asc)]
    public class OutboundAllocationHeader : BaseEntity<long>
    {
        /// <summary>
        /// 出库单ID
        /// </summary>
        
        [SugarColumn(IsNullable = false, ColumnDescription = "出库单ID")]
        public long OutboundOrderId { get; set; }

        /// <summary>
        /// 出库单号（冗余字段，来源于 OutboundOrder.OrderNo，方便查询和展示）
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnDescription = "出库单号")]
        public string? OutboundOrderNo { get; set; }

        /// <summary>
        /// 分配状态（ALLOCATED-已分配 / PICKING-拣货中 / PICKED-已拣货 / CANCELLED-已取消）
        /// </summary>
        
        [SugarColumn(Length = 20, IsNullable = false, ColumnDescription = "分配状态", DefaultValue = "ALLOCATED")]
        public string AllocStatus { get; set; } = BizConstants.AllocationStatus.ALLOCATED;

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
        /// 分配策略类型（FIFO-先进先出 / FEFO-先到期先出 / BATCH-批次策略等）
        /// </summary>
        [SugarColumn(Length = 30, IsNullable = true, ColumnDescription = "分配策略类型")]
        public string? StrategyType { get; set; }

        /// <summary>
        /// 总行数（分配明细行数）
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "总行数", DefaultValue = "0")]
        public int TotalLines { get; set; }

        /// <summary>
        /// 分配时间
        /// </summary>
        
        [SugarColumn(IsNullable = false, ColumnDescription = "分配时间")]
        public DateTime AllocTime { get; set; }

        /// <summary>
        /// 完成时间
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "完成时间")]
        public DateTime? CompleteTime { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnDescription = "备注")]
        public string? Remark { get; set; }

        /// <summary>
        /// 分配明细列表（导航属性，不映射数据库列）
        /// </summary>
        [Navigate(NavigateType.OneToMany, nameof(OutboundAllocationDetail.HeaderId), nameof(Id))]
        public List<OutboundAllocationDetail>? Details { get; set; }
    }
}
