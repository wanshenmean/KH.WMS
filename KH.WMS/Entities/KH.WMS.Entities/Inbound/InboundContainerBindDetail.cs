using KH.WMS.Core.Models.Entities;
using SqlSugar;
using System.ComponentModel.DataAnnotations;

namespace KH.WMS.Entities.Inbound
{
    /// <summary>
    /// 入库组盘明细表
    /// 一个组盘头下对应一种物料的组盘信息（含批次、数量等）
    /// 与库存明细表的区别：无锁定数量、无序列号、无供应商
    /// </summary>
    [SugarTable("bd_inbound_container_bind_detail")]
    [SugarIndex("idx_header", nameof(HeaderId), OrderByType.Asc)]
    [SugarIndex("idx_material", nameof(MaterialId), OrderByType.Asc)]
    [SugarIndex("idx_batch", nameof(BatchNo), OrderByType.Asc)]
    [SugarIndex("idx_order_line", nameof(InboundOrderLineId), OrderByType.Asc)]
    [SugarIndex("idx_header_material", nameof(HeaderId), OrderByType.Asc, nameof(MaterialId), OrderByType.Asc)]
    public class InboundContainerBindDetail : BaseEntity<long>
    {
        /// <summary>
        /// 组盘头ID
        /// </summary>
        
        [SugarColumn(IsNullable = false, ColumnDescription = "组盘头ID")]
        public long HeaderId { get; set; }

        /// <summary>
        /// 入库单明细ID（可空，支持通用组盘）
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "入库单明细ID")]
        public long? InboundOrderLineId { get; set; }

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
        /// 物料名称（冗余字段，查询时使用）
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true, ColumnDescription = "物料名称")]
        public string? MaterialName { get; set; }

        /// <summary>
        /// 组盘数量
        /// </summary>
        
        [SugarColumn(DecimalDigits = 3, Length = 12, IsNullable = false, ColumnDescription = "组盘数量", DefaultValue = "0")]
        public decimal Qty { get; set; }

        /// <summary>
        /// 单位ID
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "单位ID")]
        public long? UnitId { get; set; }

        /// <summary>
        /// 批次号
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnDescription = "批次号")]
        public string? BatchNo { get; set; }

        /// <summary>
        /// 生产日期
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "生产日期")]
        public DateOnly? ProductionDate { get; set; }

        /// <summary>
        /// 有效期
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "有效期")]
        public DateOnly? ExpiryDate { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnDescription = "备注")]
        public string? Remark { get; set; }
    }
}
