using KH.WMS.Core.Models.Entities;
using SqlSugar;
using System.ComponentModel.DataAnnotations;
using KH.WMS.Core.Database.SqlSugar;

namespace KH.WMS.Entities.Inventory
{
    /// <summary>
    /// 库存预警规则配置
    /// </summary>
    [ConfigDb]
    [SugarTable("cfg_inventory_alert")]
    [SugarIndex("idx_warehouse", nameof(WarehouseId), OrderByType.Asc)]
    [SugarIndex("idx_material", nameof(MaterialId), OrderByType.Asc)]
    [SugarIndex("idx_zone", nameof(ZoneId), OrderByType.Asc)]
    [SugarIndex("idx_alert_type", nameof(AlertType), OrderByType.Asc)]
    public class CfgInventoryAlert : BaseEntity<long>, IEnableDisableEntity
    {
        /// <summary>
        /// 预警名称
        /// </summary>
        
        [SugarColumn(Length = 100, IsNullable = false, ColumnDescription = "预警名称")]
        public string AlertName { get; set; } = string.Empty;

        /// <summary>
        /// 预警类型（MIN_STOCK-最低库存 / MAX_STOCK-最高库存 / EXPIRY-有效期预警）
        /// </summary>
        
        [SugarColumn(Length = 20, IsNullable = false, ColumnDescription = "预警类型")]
        public string AlertType { get; set; } = string.Empty;

        /// <summary>
        /// 仓库ID（空表示所有仓库）
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "仓库ID")]
        public long? WarehouseId { get; set; }

        /// <summary>
        /// 库区ID（空表示所有库区）
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "库区ID")]
        public long? ZoneId { get; set; }

        /// <summary>
        /// 物料ID（空表示所有物料）
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "物料ID")]
        public long? MaterialId { get; set; }

        /// <summary>
        /// 预警阈值（库存数量，最低库存时为下限值，最高库存时为上限值）
        /// </summary>
        [SugarColumn(DecimalDigits = 3, Length = 12, IsNullable = true, ColumnDescription = "预警阈值")]
        public decimal? AlertThreshold { get; set; }

        /// <summary>
        /// 有效期预警天数（距过期还有多少天时预警）
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "有效期预警天数")]
        public int? ExpiryDays { get; set; }

        /// <summary>
        /// 通知方式（NONE-不通知 / INTERNAL-站内信 / EMAIL-邮件 / ALL-全部）
        /// </summary>
        [SugarColumn(Length = 20, IsNullable = false, ColumnDescription = "通知方式", DefaultValue = "INTERNAL")]
        public string NotifyType { get; set; } = "INTERNAL";

        /// <summary>
        /// 通知人员ID列表（逗号分隔）
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnDescription = "通知人员ID列表")]
        public string? NotifyUserIds { get; set; }

        /// <summary>
        /// 检查频率（分钟，0表示实时检查）
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "检查频率(分钟)", DefaultValue = "30")]
        public int CheckInterval { get; set; } = 30;

        /// <summary>
        /// 是否启用
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "是否启用", DefaultValue = "1")]
        public byte IsActive { get; set; } = 1;

        /// <summary>
        /// 备注
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnDescription = "备注")]
        public string? Remark { get; set; }
    }
}
