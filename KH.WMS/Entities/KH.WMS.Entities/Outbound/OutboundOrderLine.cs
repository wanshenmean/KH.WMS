using KH.WMS.Core.Models.Entities;
using KH.WMS.Entities.Constants;
using SqlSugar;
using System.ComponentModel.DataAnnotations;

namespace KH.WMS.Entities.Outbound
{
    /// <summary>
    /// 出库单行
    /// </summary>
    [SugarTable("bd_outbound_order_line")]
    [SugarIndex("uk_order_line", nameof(OrderId), OrderByType.Asc, nameof(LineNo), OrderByType.Asc)]
    [SugarIndex("idx_order_id", nameof(OrderId), OrderByType.Asc)]
    [SugarIndex("idx_material", nameof(MaterialId), OrderByType.Asc)]
    [SugarIndex("idx_order_material", nameof(OrderId), OrderByType.Asc, nameof(MaterialId), OrderByType.Asc)]
    public class OutboundOrderLine : BaseEntity<long>
    {

        /// <summary>
        /// 出库单ID
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "出库单ID")]
        public long OrderId { get; set; }

        /// <summary>
        /// 行号
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "行号")]
        public int LineNo { get; set; }

        /// <summary>
        /// 物料ID
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "物料ID")]
        public long MaterialId { get; set; }

        /// <summary>
        /// 物料编码
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnDescription = "物料编码")]
        public string? MaterialCode { get; set; }

        /// <summary>
        /// 物料名称
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true, ColumnDescription = "物料名称")]
        public string? MaterialName { get; set; }

        /// <summary>
        /// 订单数量
        /// </summary>
        [SugarColumn(DecimalDigits = 3, Length = 12, IsNullable = true, ColumnDescription = "订单数量")]
        public decimal? OrderedQty { get; set; }

        /// <summary>
        /// 发货数量
        /// </summary>
        [SugarColumn(DecimalDigits = 3, Length = 12, IsNullable = false, ColumnDescription = "发货数量", DefaultValue = "0")]
        public decimal ShippedQty { get; set; }

        /// <summary>
        /// 计量单位ID
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "计量单位ID")]
        public long? UnitId { get; set; }

        /// <summary>
        /// 批次号
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnDescription = "批次号")]
        public string? BatchNo { get; set; }

        /// <summary>
        /// 质量状态
        /// </summary>
        [SugarColumn(Length = 20, IsNullable = false, ColumnDescription = "质量状态", DefaultValue = "PENDING")]
        public string QualityStatus { get; set; } = BizConstants.QualityStatus.PENDING;

        /// <summary>
        /// 行状态
        /// </summary>
        [SugarColumn(Length = 20, IsNullable = false, ColumnDescription = "行状态", DefaultValue = "OPEN")]
        public string LineStatus { get; set; } = BizConstants.OutboundLineStatus.OPEN;

        /// <summary>
        /// 扩展数据（JSON，存储配置化的扩展字段值）
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDataType = "TEXT", ColumnDescription = "扩展数据")]
        public string? ExtData { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnDescription = "备注")]
        public string? Remark { get; set; }
    }
}
