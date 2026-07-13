using KH.WMS.Core.Models.Entities;
using KH.WMS.Entities.Constants;
using SqlSugar;
using System.ComponentModel.DataAnnotations;

namespace KH.WMS.Entities.Inventory
{
    /// <summary>
    /// 库存预警记录
    /// </summary>
    [SugarTable("inv_alert_record")]
    [SugarIndex("idx_alert_type", nameof(AlertType), OrderByType.Asc)]
    [SugarIndex("idx_warehouse", nameof(WarehouseId), OrderByType.Asc)]
    [SugarIndex("idx_material", nameof(MaterialId), OrderByType.Asc)]
    [SugarIndex("idx_status", nameof(Status), OrderByType.Asc)]
    [SugarIndex("idx_alert_time", nameof(AlertTime), OrderByType.Asc)]
    public class InvAlertRecord : BaseEntity<long>
    {
        /// <summary>
        /// 预警规则ID
        /// </summary>
        
        [SugarColumn(IsNullable = false, ColumnDescription = "预警规则ID")]
        public long AlertRuleId { get; set; }

        /// <summary>
        /// 预警类型（MIN_STOCK / MAX_STOCK / EXPIRY）
        /// </summary>
        
        [SugarColumn(Length = 20, IsNullable = false, ColumnDescription = "预警类型")]
        public string AlertType { get; set; } = string.Empty;

        /// <summary>
        /// 仓库ID
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "仓库ID")]
        public long WarehouseId { get; set; }

        /// <summary>
        /// 库区ID
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "库区ID")]
        public long? ZoneId { get; set; }

        /// <summary>
        /// 库位ID
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "库位ID")]
        public long? LocationId { get; set; }

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
        /// 当前库存数量
        /// </summary>
        [SugarColumn(DecimalDigits = 3, Length = 12, IsNullable = false, ColumnDescription = "当前库存数量")]
        public decimal CurrentQty { get; set; }

        /// <summary>
        /// 预警阈值
        /// </summary>
        [SugarColumn(DecimalDigits = 3, Length = 12, IsNullable = true, ColumnDescription = "预警阈值")]
        public decimal? AlertThreshold { get; set; }

        /// <summary>
        /// 过期日期（有效期预警时有值）
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "过期日期")]
        public DateOnly? ExpiryDate { get; set; }

        /// <summary>
        /// 剩余天数（有效期预警时有值）
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "剩余天数")]
        public int? RemainingDays { get; set; }

        /// <summary>
        /// 预警时间
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "预警时间")]
        public DateTime AlertTime { get; set; }

        /// <summary>
        /// 状态（PENDING-待处理 / HANDLED-已处理 / IGNORED-已忽略）
        /// </summary>
        [SugarColumn(Length = 20, IsNullable = false, ColumnDescription = "状态", DefaultValue = "PENDING")]
        public string Status { get; set; } = BizConstants.Status.PENDING;

        /// <summary>
        /// 处理人
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "处理人")]
        public long? HandledBy { get; set; }

        /// <summary>
        /// 处理时间
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "处理时间")]
        public DateTime? HandleTime { get; set; }

        /// <summary>
        /// 处理备注
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnDescription = "处理备注")]
        public string? HandleRemark { get; set; }
    }
}
