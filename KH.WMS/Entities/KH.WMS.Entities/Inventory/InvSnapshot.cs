using KH.WMS.Core.Models.Entities;
using SqlSugar;
using System.ComponentModel.DataAnnotations;

namespace KH.WMS.Entities.Inventory
{
    /// <summary>
    /// 库存快照
    /// 支持每日定时快照和盘点快照两种场景
    /// </summary>
    [SugarTable("inv_snapshot")]
    [SugarIndex("idx_snapshot_date", nameof(SnapshotDate), OrderByType.Asc)]
    [SugarIndex("idx_type", nameof(SnapshotType), OrderByType.Asc)]
    [SugarIndex("idx_warehouse", nameof(WarehouseId), OrderByType.Asc)]
    [SugarIndex("idx_location", nameof(LocationId), OrderByType.Asc)]
    [SugarIndex("idx_material", nameof(MaterialId), OrderByType.Asc)]
    [SugarIndex("idx_date_warehouse", nameof(SnapshotDate), OrderByType.Asc, nameof(WarehouseId), OrderByType.Asc)]
    [SugarIndex("idx_date_location", nameof(SnapshotDate), OrderByType.Asc, nameof(LocationId), OrderByType.Asc)]
    [SugarIndex("idx_stocktake", nameof(StocktakeId), OrderByType.Asc)]
    [SugarIndex("idx_snapshot_header", nameof(SnapshotHeaderId), OrderByType.Asc)]
    public class InvSnapshot : BaseEntity<long>
    {
        /// <summary>
        /// 快照头ID
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "快照头ID")]
        public long? SnapshotHeaderId { get; set; }

        /// <summary>
        /// 快照日期
        /// </summary>
        
        [SugarColumn(IsNullable = false, ColumnDescription = "快照日期")]
        public DateOnly SnapshotDate { get; set; }

        /// <summary>
        /// 快照类型（DAILY-每日定时 / STOCKTAKE-盘点触发）
        /// </summary>
        [SugarColumn(Length = 20, IsNullable = false, ColumnDescription = "快照类型", DefaultValue = "DAILY")]
        public string SnapshotType { get; set; } = "DAILY";

        /// <summary>
        /// 关联盘点单ID（盘点快照时记录来源）
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "关联盘点单ID")]
        public long? StocktakeId { get; set; }

        /// <summary>
        /// 仓库ID
        /// </summary>
        
        [SugarColumn(IsNullable = false, ColumnDescription = "仓库ID")]
        public long WarehouseId { get; set; }

        /// <summary>
        /// 库位ID
        /// </summary>
        
        [SugarColumn(IsNullable = false, ColumnDescription = "库位ID")]
        public long LocationId { get; set; }

        /// <summary>
        /// 物料ID
        /// </summary>
        
        [SugarColumn(IsNullable = false, ColumnDescription = "物料ID")]
        public long MaterialId { get; set; }

        /// <summary>
        /// 批次号
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnDescription = "批次号")]
        public string? BatchNo { get; set; }

        /// <summary>
        /// 容器ID
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "容器ID")]
        public long? ContainerId { get; set; }

        /// <summary>
        /// 库存数量
        /// </summary>
        [SugarColumn(DecimalDigits = 3, Length = 12, IsNullable = false, ColumnDescription = "库存数量", DefaultValue = "0")]
        public decimal Qty { get; set; }

        /// <summary>
        /// 快照生成时间
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "快照生成时间")]
        public DateTime SnapshotTime { get; set; }
    }
}
