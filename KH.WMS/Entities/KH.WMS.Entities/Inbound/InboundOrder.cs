using KH.WMS.Core.Models.Entities;
using KH.WMS.Core.Services;
using KH.WMS.Entities.Constants;
using SqlSugar;
using System.ComponentModel.DataAnnotations;
using CoreValidationResult = KH.WMS.Core.Services.ValidationResult;

namespace KH.WMS.Entities.Inbound
{
    /// <summary>
    /// 入库单
    /// </summary>
    [SugarTable("bd_inbound_order")]
    [SugarIndex("uk_order_no", nameof(OrderNo), OrderByType.Asc)]
    [SugarIndex("idx_status", nameof(OrderStatus), OrderByType.Asc)]
    [SugarIndex("idx_source", nameof(SourceSystem), OrderByType.Asc, nameof(SourceDocNo), OrderByType.Asc)]
    [SugarIndex("idx_warehouse", nameof(WarehouseId), OrderByType.Asc)]
    [SugarIndex("idx_supplier", nameof(SupplierId), OrderByType.Asc)]
    [SugarIndex("idx_query", nameof(OrderStatus), OrderByType.Asc, nameof(OrderDate), OrderByType.Asc)]
    public class InboundOrder : BaseEntity<long>
    {
        /// <summary>
        /// 入库单号
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = false, ColumnDescription = "入库单号")]
        public string OrderNo { get; set; }

        /// <summary>
        /// 入库类型
        /// </summary>
        
        [SugarColumn(Length = 30, IsNullable = false, ColumnDescription = "入库类型")]
        public string OrderType { get; set; }

        /// <summary>
        /// 单据状态
        /// </summary>
        [SugarColumn(Length = 20, IsNullable = false, ColumnDescription = "单据状态", DefaultValue = "DRAFT")]
        public string OrderStatus { get; set; } = BizConstants.InboundOrderStatus.DRAFT;

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
        /// 供应商ID
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "供应商ID")]
        public long? SupplierId { get; set; }

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
        /// 
        /// </summary>
        [Navigate(NavigateType.OneToMany, nameof(InboundOrderLine.OrderId), nameof(Id))]
        public List<InboundOrderLine>? Items { get; set; }

        // === 领域方法 ===

        /// <summary>
        /// 是否允许收货操作（基于配置表的 NextStatuses 判断）
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public bool CanReceiveOrder => OrderStatus == BizConstants.InboundOrderStatus.DRAFT
                                    || OrderStatus == BizConstants.InboundOrderStatus.RECEIVING;

        /// <summary>
        /// 验证是否允许收货（由 Service 层通过 _statusValidator 做状态流转校验，此处仅做基本校验）
        /// </summary>
        public CoreValidationResult ValidateCanReceive()
        {
            if (!CanReceiveOrder)
                return CoreValidationResult.Fail($"入库单状态为 {OrderStatus}，不允许收货");
            return CoreValidationResult.Success();
        }

        /// <summary>
        /// 更新订单状态（由 Service 层根据配置表校验后调用）
        /// </summary>
        public void SetStatus(string newStatus)
        {
            OrderStatus = newStatus;
        }

        /// <summary>
        /// 根据所有明细行状态刷新订单状态。若全部收货完成则使用传入的目标状态
        /// </summary>
        public void RefreshStatusFromLines(List<InboundOrderLine> allLines, string receivedStatus)
        {
            if (allLines.All(l => l.LineStatus == BizConstants.InboundLineStatus.RECEIVED))
                OrderStatus = receivedStatus;
        }

        /// <summary>
        /// 判断所有明细行是否已全部组盘完成。
        /// 组盘基准量：需收货(采购)类型为 ReceivedQty，不需收货(生产/退货/其他)类型为 OrderedQty。
        /// </summary>
        public bool IsFullyBound(List<InboundOrderLine> allLines, Dictionary<long, decimal> boundQtyByLineId)
        {
            var needReceive = BizConstants.OrderTypes.IsReceiveRequired(OrderType);
            return allLines.All(ol =>
            {
                boundQtyByLineId.TryGetValue(ol.Id, out var boundQty);
                var baseQty = needReceive ? ol.ReceivedQty : (ol.OrderedQty ?? 0);
                return boundQty >= baseQty;
            });
        }

        /// <summary>
        /// 标记为已组盘（由 Service 层根据配置表校验后调用）
        /// </summary>
        public void MarkAsBound(string boundStatus)
        {
            OrderStatus = boundStatus;
        }

        /// <summary>
        /// 当前允许的操作列表（由 Service 根据状态配置计算，不映射数据库）
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public List<string> AllowedActions { get; set; } = new();
    }
}
