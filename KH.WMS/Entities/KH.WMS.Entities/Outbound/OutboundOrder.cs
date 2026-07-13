using KH.WMS.Core.Models.Entities;
using KH.WMS.Entities.Constants;
using SqlSugar;
using System.ComponentModel.DataAnnotations;

namespace KH.WMS.Entities.Outbound
{
    /// <summary>
    /// 出库单
    /// </summary>
    [SugarTable("bd_outbound_order")]
    [SugarIndex("uk_order_no", nameof(OrderNo), OrderByType.Asc)]
    [SugarIndex("idx_status", nameof(OrderStatus), OrderByType.Asc)]
    [SugarIndex("idx_source", nameof(SourceSystem), OrderByType.Asc, nameof(SourceDocNo), OrderByType.Asc)]
    [SugarIndex("idx_warehouse", nameof(WarehouseId), OrderByType.Asc)]
    [SugarIndex("idx_customer", nameof(CustomerId), OrderByType.Asc)]
    [SugarIndex("idx_query", nameof(OrderStatus), OrderByType.Asc, nameof(OrderDate), OrderByType.Asc)]
    public class OutboundOrder : BaseEntity<long>
    {

        /// <summary>
        /// 出库单号
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = false, ColumnDescription = "出库单号")]
        public string OrderNo { get; set; }

        /// <summary>
        /// 出库类型
        /// </summary>
        [SugarColumn(Length = 30, IsNullable = false, ColumnDescription = "出库类型")]
        public string OrderType { get; set; }

        /// <summary>
        /// 单据状态
        /// </summary>
        [SugarColumn(Length = 20, IsNullable = false, ColumnDescription = "单据状态", DefaultValue = "DRAFT")]
        public string OrderStatus { get; set; } = BizConstants.OutboundOrderStatus.DRAFT;

        /// <summary>
        /// 来源系统
        /// </summary>
        [SugarColumn(Length = 20, IsNullable = true, ColumnDescription = "来源系统")]
        public string? SourceSystem { get; set; }

        /// <summary>
        /// 来源单据编号
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnDescription = "来源单据编号")]
        public string? SourceDocNo { get; set; }

        /// <summary>
        /// 来源单据类型
        /// </summary>
        [SugarColumn(Length = 30, IsNullable = true, ColumnDescription = "来源单据类型")]
        public string? SourceDocType { get; set; }

        /// <summary>
        /// 仓库ID
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "仓库ID")]
        public long? WarehouseId { get; set; }

        /// <summary>
        /// 客户ID
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "客户ID")]
        public long? CustomerId { get; set; }

        /// <summary>
        /// 单据日期
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "单据日期")]
        public DateOnly? OrderDate { get; set; }

        /// <summary>
        /// 总行数
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "总行数", DefaultValue = "0")]
        public int TotalLines { get; set; }

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

        /// <summary>
        /// 出库单明细行（导航属性）
        /// </summary>
        [Navigate(NavigateType.OneToMany, nameof(OutboundOrderLine.OrderId), nameof(Id))]
        public List<OutboundOrderLine>? Items { get; set; }

        /// <summary>
        /// 当前允许的操作列表（由 Service 根据状态配置计算，不映射数据库）
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public List<string> AllowedActions { get; set; } = new();
    }
}
