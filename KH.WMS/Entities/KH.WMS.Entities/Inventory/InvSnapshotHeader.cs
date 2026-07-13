using KH.WMS.Core.Models.Entities;
using SqlSugar;
using System.ComponentModel.DataAnnotations;

namespace KH.WMS.Entities.Inventory
{
    /// <summary>
    /// 库存快照头表
    /// 记录每次快照的汇总信息，快照明细存储在 InvSnapshot（明细表）
    /// </summary>
    [SugarTable("inv_snapshot_header")]
    [SugarIndex("uk_snapshot_no", nameof(SnapshotNo), OrderByType.Asc)]
    [SugarIndex("idx_type", nameof(SnapshotType), OrderByType.Asc)]
    [SugarIndex("idx_date", nameof(SnapshotDate), OrderByType.Asc)]
    public class InvSnapshotHeader : BaseEntity<long>
    {
        /// <summary>
        /// 快照编号
        /// </summary>
        
        [SugarColumn(Length = 50, IsNullable = false, ColumnDescription = "快照编号")]
        public string SnapshotNo { get; set; } = string.Empty;

        /// <summary>
        /// 快照名称
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true, ColumnDescription = "快照名称")]
        public string? SnapshotName { get; set; }

        /// <summary>
        /// 快照类型（DAILY-每日定时 / MANUAL-手动创建 / STOCKTAKE-盘点触发）
        /// </summary>
        
        [SugarColumn(Length = 20, IsNullable = false, ColumnDescription = "快照类型", DefaultValue = "MANUAL")]
        public string SnapshotType { get; set; } = "MANUAL";

        /// <summary>
        /// 快照日期
        /// </summary>
        
        [SugarColumn(IsNullable = false, ColumnDescription = "快照日期")]
        public DateOnly SnapshotDate { get; set; }

        /// <summary>
        /// 物料总数
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "物料总数", DefaultValue = "0")]
        public int MaterialCount { get; set; }

        /// <summary>
        /// 库存总量
        /// </summary>
        [SugarColumn(DecimalDigits = 3, Length = 12, IsNullable = false, ColumnDescription = "库存总量", DefaultValue = "0")]
        public decimal TotalStock { get; set; }

        /// <summary>
        /// 快照说明
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnDescription = "快照说明")]
        public string? Description { get; set; }

        /// <summary>
        /// 快照明细（导航属性）
        /// </summary>
        [Navigate(NavigateType.OneToMany, nameof(InvSnapshot.SnapshotHeaderId), nameof(Id))]
        public List<InvSnapshot>? Details { get; set; }
    }
}
