using SqlSugar;
using System.ComponentModel.DataAnnotations;
using KH.WMS.Core.Models.Entities;

namespace KH.WMS.Entities.Inventory
{
    /// <summary>
    /// 库存明细
    /// 一个库存头下对应一种物料的库存信息（含批次、数量等）
    /// </summary>
    [SugarTable("inv_inventory_detail")]
    [SugarIndex("idx_header", nameof(HeaderId), OrderByType.Asc)]
    [SugarIndex("idx_material", nameof(MaterialId), OrderByType.Asc)]
    [SugarIndex("idx_batch", nameof(BatchNo), OrderByType.Asc)]
    [SugarIndex("idx_serial", nameof(SerialNo), OrderByType.Asc)]
    [SugarIndex("idx_supplier", nameof(SupplierId), OrderByType.Asc)]
    [SugarIndex("idx_inbound_doc", nameof(InboundDocNo), OrderByType.Asc)]
    [SugarIndex("idx_expiry", nameof(ExpiryDate), OrderByType.Asc)]
    [SugarIndex("idx_header_material", nameof(HeaderId), OrderByType.Asc, nameof(MaterialId), OrderByType.Asc)]
    public class InvInventoryDetail : BaseEntity<long>
    {
        /// <summary>
        /// 库存头ID
        /// </summary>
        
        [SugarColumn(IsNullable = false, ColumnDescription = "库存头ID")]
        public long HeaderId { get; set; }

        /// <summary>
        /// 物料ID
        /// </summary>
        
        [SugarColumn(IsNullable = false, ColumnDescription = "物料ID")]
        public long MaterialId { get; set; }

        /// <summary>
        /// 物料编码（冗余字段，查询时使用）
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "物料ID")]
        public string MaterialCode { get; set; } = string.Empty;

        /// <summary>
        /// 批次号
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnDescription = "批次号")]
        public string? BatchNo { get; set; }

        /// <summary>
        /// 序列号（单品管理时使用）
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnDescription = "序列号")]
        public string? SerialNo { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        
        [SugarColumn(DecimalDigits = 3, Length = 12, IsNullable = false, ColumnDescription = "数量", DefaultValue = "0")]
        public decimal Qty { get; set; }

        /// <summary>
        /// 单位
        /// </summary>
        [SugarColumn(Length = 20, IsNullable = true, ColumnDescription = "单位")]
        public string? Unit { get; set; }

        /// <summary>
        /// 锁定数量
        /// </summary>
        [SugarColumn(DecimalDigits = 3, Length = 12, IsNullable = false, ColumnDescription = "锁定数量", DefaultValue = "0")]
        public decimal LockedQty { get; set; }

        /// <summary>
        /// 生产日期
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "生产日期")]
        public DateOnly? ProductionDate { get; set; }

        /// <summary>
        /// 过期日期
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "过期日期")]
        public DateOnly? ExpiryDate { get; set; }

        /// <summary>
        /// 入库单号
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnDescription = "入库单号")]
        public string? InboundDocNo { get; set; }

        /// <summary>
        /// 供应商ID
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "供应商ID")]
        public long? SupplierId { get; set; }

        /// <summary>
        /// 入库时间
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "入库时间")]
        public DateTime? InboundTime { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnDescription = "备注")]
        public string? Remark { get; set; }

        /// <summary>
        /// 扩展字段（JSON格式，CfgExtField配置驱动）
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDataType = "TEXT", ColumnDescription = "扩展字段")]
        public string? ExtData { get; set; }
    }
}
