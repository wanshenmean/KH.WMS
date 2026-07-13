using KH.WMS.Core.Models.Entities;
using KH.WMS.Core.Services;
using KH.WMS.Entities.Constants;
using SqlSugar;
using System.ComponentModel.DataAnnotations;
using CoreValidationResult = KH.WMS.Core.Services.ValidationResult;

namespace KH.WMS.Entities.Inbound
{
    /// <summary>
    /// 入库单行
    /// </summary>
    [SugarTable("bd_inbound_order_line")]
    [SugarIndex("uk_order_line", nameof(OrderId), OrderByType.Asc, nameof(LineNo), OrderByType.Asc)]
    [SugarIndex("idx_order_id", nameof(OrderId), OrderByType.Asc)]
    [SugarIndex("idx_material", nameof(MaterialId), OrderByType.Asc)]
    [SugarIndex("idx_order_material", nameof(OrderId), OrderByType.Asc, nameof(MaterialId), OrderByType.Asc)]
    public class InboundOrderLine : BaseEntity<long>
    {

        /// <summary>
        /// 入库单ID
        /// </summary>
        
        [SugarColumn(IsNullable = false, ColumnDescription = "入库单ID")]
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
        /// 收货数量
        /// </summary>
        [SugarColumn(DecimalDigits = 3, Length = 12, IsNullable = false, ColumnDescription = "收货数量", DefaultValue = "0")]
        public decimal ReceivedQty { get; set; }

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
        /// 生产日期
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "生产日期")]
        public DateOnly? ManufactureDate { get; set; }

        /// <summary>
        /// 有效期
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "有效期")]
        public DateOnly? ExpiryDate { get; set; }

        /// <summary>
        /// 质量状态
        /// </summary>
        [SugarColumn(Length = 20, IsNullable = false, ColumnDescription = "质量状态", DefaultValue = "PENDING")]
        public string QualityStatus { get; set; } = BizConstants.QualityStatus.PENDING;

        /// <summary>
        /// 行状态
        /// </summary>
        [SugarColumn(Length = 20, IsNullable = false, ColumnDescription = "行状态", DefaultValue = "OPEN")]
        public string LineStatus { get; set; } = BizConstants.InboundLineStatus.OPEN;

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

        // === 领域方法 ===

        /// <summary>
        /// 剩余应收数量
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public decimal RemainingQty => (OrderedQty ?? 0) - ReceivedQty;

        /// <summary>
        /// 是否已全部收货
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        public bool IsFullyReceived => ReceivedQty >= (OrderedQty ?? 0);

        /// <summary>
        /// 是否可以收货（非 RECEIVED 状态）
        /// </summary>
        public bool CanReceive() => LineStatus != BizConstants.InboundLineStatus.RECEIVED;

        /// <summary>
        /// 校验收货数量合法性
        /// </summary>
        public CoreValidationResult ValidateReceiveQty(decimal qty)
        {
            if (qty <= 0)
                return CoreValidationResult.Fail($"明细行 {LineNo} 收货数量必须大于0");
            if (qty > RemainingQty)
                return CoreValidationResult.Fail($"明细行 {LineNo} 本次收货数量({qty})超过剩余应收数量({RemainingQty})");
            return CoreValidationResult.Success();
        }

        /// <summary>
        /// 执行收货：累加数量，更新批次，推进行状态
        /// </summary>
        public void Receive(decimal qty, string? batchNo, DateOnly? manufactureDate, DateOnly? expiryDate)
        {
            ReceivedQty += qty;
            if (!string.IsNullOrEmpty(batchNo)) BatchNo = batchNo;
            if (manufactureDate.HasValue) ManufactureDate = manufactureDate;
            if (expiryDate.HasValue) ExpiryDate = expiryDate;
            LineStatus = IsFullyReceived ? BizConstants.InboundLineStatus.RECEIVED : BizConstants.InboundLineStatus.RECEIVING;
        }

        /// <summary>
        /// 校验组盘数量合法性
        /// </summary>
        /// <param name="bindQty">本次组盘数量</param>
        /// <param name="alreadyBoundQty">该明细行已组盘数量</param>
        /// <param name="baseQty">组盘基准量：需收货类型为 ReceivedQty，不需收货类型为 OrderedQty</param>
        public CoreValidationResult ValidateBindQty(decimal bindQty, decimal alreadyBoundQty, decimal baseQty)
        {
            var availableQty = baseQty - alreadyBoundQty;
            if (bindQty <= 0)
                return CoreValidationResult.Fail($"明细行 {LineNo} 组盘数量必须大于0");
            if (bindQty > availableQty)
                return CoreValidationResult.Fail($"明细行 {LineNo} 组盘数量({bindQty})超过可组盘数量({availableQty})");
            return CoreValidationResult.Success();
        }
    }
}
