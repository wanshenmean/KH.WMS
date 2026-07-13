using SqlSugar;
using System.ComponentModel.DataAnnotations;
using KH.WMS.Core.Models.Entities;
using KH.WMS.Entities.Constants;

namespace KH.WMS.Entities.Inventory
{
    /// <summary>
    /// 库存头表
    /// 一个容器在同一仓库同一货位的库存汇总，包含多种物料明细
    /// </summary>
    [SugarTable("inv_inventory_header")]
    [SugarIndex("uk_container", nameof(ContainerCode), OrderByType.Asc)]
    [SugarIndex("idx_warehouse", nameof(WarehouseId), OrderByType.Asc)]
    [SugarIndex("idx_location", nameof(LocationId), OrderByType.Asc)]
    [SugarIndex("idx_status", nameof(InventoryStatus), OrderByType.Asc)]
    public class InvInventoryHeader : BaseEntity<long>
    {
        /// <summary>
        /// 容器编号
        /// </summary>
        
        [SugarColumn(Length = 50, IsNullable = false, ColumnDescription = "容器编号")]
        public string ContainerCode { get; set; } = string.Empty;

        /// <summary>
        /// 仓库ID
        /// </summary>
        
        [SugarColumn(IsNullable = false, ColumnDescription = "仓库ID")]
        public long? WarehouseId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "仓库编码")]
        public string? WarehouseCode { get; set; } = string.Empty;

        /// <summary>
        /// 当前所在货位ID
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "当前所在货位ID")]
        public long? LocationId { get; set; }

        /// <summary>
        /// 当前所在货位编码
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "当前所在货位编码")]
        public string? LocationCode { get; set; } = string.Empty;

        /// <summary>
        /// 库存状态（AVAILABLE-可用 / LOCKED-锁定 / QC-质检中 / FROZEN-冻结）
        /// </summary>
        
        [SugarColumn(Length = 20, IsNullable = false, ColumnDescription = "库存状态", DefaultValue = "AVAILABLE")]
        public string InventoryStatus { get; set; } = BizConstants.InventoryStatus.AVAILABLE;

        /// <summary>
        /// 明细数量（该容器有几种物料）
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "明细数量", DefaultValue = "0")]
        public int DetailCount { get; set; }

        /// <summary>
        /// 入库时间
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "入库时间")]
        public DateTime InboundTime { get; set; }

        /// <summary>
        /// 最后入库时间
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "最后入库时间")]
        public DateTime? LastInboundTime { get; set; }

        /// <summary>
        /// 最后盘点时间
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "最后盘点时间")]
        public DateTime? LastStocktakeTime { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnDescription = "备注")]
        public string? Remark { get; set; }

        /// <summary>
        /// 库存明细（导航属性）
        /// </summary>
        [Navigate(NavigateType.OneToMany, nameof(InvInventoryDetail.HeaderId), nameof(Id))]
        public List<InvInventoryDetail>? Details { get; set; }
    }
}
