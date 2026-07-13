using KH.WMS.Core.Models.Entities;
using KH.WMS.Entities.Constants;
using SqlSugar;
using System.ComponentModel.DataAnnotations;

namespace KH.WMS.Entities.Outbound
{
    /// <summary>
    /// 出库分配明细表
    /// 记录出库单每一行物料具体从哪个库位、哪个容器、哪个批次取出多少数量。
    /// 分配成功时，对应库存的锁定数量（InvInventoryDetail.LockedQty）会增加。
    /// 出库确认后扣减实际库存并记录库存流水（InvMovement）。
    /// </summary>
    [SugarTable("bd_outbound_allocation_detail")]
    [SugarIndex("idx_header", nameof(HeaderId), OrderByType.Asc)]
    [SugarIndex("idx_order_line", nameof(OrderLineId), OrderByType.Asc)]
    [SugarIndex("idx_inventory", nameof(InventoryHeaderId), OrderByType.Asc)]
    [SugarIndex("idx_container", nameof(ContainerCode), OrderByType.Asc)]
    [SugarIndex("idx_location", nameof(LocationId), OrderByType.Asc)]
    [SugarIndex("idx_material", nameof(MaterialId), OrderByType.Asc)]
    [SugarIndex("idx_status", nameof(LineStatus), OrderByType.Asc)]
    [SugarIndex("idx_task", nameof(TaskHeaderId), OrderByType.Asc)]
    public class OutboundAllocationDetail : BaseEntity<long>
    {
        /// <summary>
        /// 分配头ID
        /// </summary>
        
        [SugarColumn(IsNullable = false, ColumnDescription = "分配头ID")]
        public long HeaderId { get; set; }

        /// <summary>
        /// 出库单明细ID
        /// </summary>
        
        [SugarColumn(IsNullable = false, ColumnDescription = "出库单明细ID")]
        public long OrderLineId { get; set; }

        /// <summary>
        /// 物料ID
        /// </summary>
        
        [SugarColumn(IsNullable = false, ColumnDescription = "物料ID")]
        public long MaterialId { get; set; }

        /// <summary>
        /// 物料编码（冗余字段，查询时使用）
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnDescription = "物料编码")]
        public string? MaterialCode { get; set; }

        /// <summary>
        /// 批次号
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnDescription = "批次号")]
        public string? BatchNo { get; set; }

        /// <summary>
        /// 库存头ID（从哪个容器取）
        /// </summary>

        [SugarColumn(IsNullable = false, ColumnDescription = "库存头ID")]
        public long InventoryHeaderId { get; set; }

        /// <summary>
        /// 库存明细ID（具体批次明细，扣减/解锁库存时定位精确行）
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "库存明细ID", DefaultValue = "0")]
        public long InventoryDetailId { get; set; }

        /// <summary>
        /// 容器ID
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "容器ID")]
        public long? ContainerId { get; set; }

        /// <summary>
        /// 容器编号
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnDescription = "容器编号")]
        public string? ContainerCode { get; set; }

        /// <summary>
        /// 库位ID（从哪个库位取）
        /// </summary>
        
        [SugarColumn(IsNullable = false, ColumnDescription = "库位ID")]
        public long LocationId { get; set; }

        /// <summary>
        /// 库位编码（冗余字段，查询时使用）
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnDescription = "库位编码")]
        public string? LocationCode { get; set; }

        /// <summary>
        /// 分配数量
        /// </summary>
        
        [SugarColumn(DecimalDigits = 3, Length = 12, IsNullable = false, ColumnDescription = "分配数量", DefaultValue = "0")]
        public decimal AllocQty { get; set; }

        /// <summary>
        /// 已拣数量（出库确认时更新）
        /// </summary>
        [SugarColumn(DecimalDigits = 3, Length = 12, IsNullable = false, ColumnDescription = "已拣数量", DefaultValue = "0")]
        public decimal PickedQty { get; set; }

        /// <summary>
        /// 行状态（ALLOCATED-已分配 / PICKED-已拣货 / CANCELLED-已取消）
        /// </summary>
        
        [SugarColumn(Length = 20, IsNullable = false, ColumnDescription = "行状态", DefaultValue = "ALLOCATED")]
        public string LineStatus { get; set; } = BizConstants.AllocationStatus.ALLOCATED;

        /// <summary>
        /// 关联的出库任务ID（分配明细生成搬运任务后记录）
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "关联的出库任务ID")]
        public long? TaskHeaderId { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnDescription = "备注")]
        public string? Remark { get; set; }
    }
}
