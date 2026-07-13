namespace KH.WMS.Modules.InboundModule.DTOs
{
    /// <summary>
    /// 收货行DTO
    /// </summary>
    public class ReceiveLineDto
    {
        /// <summary>
        /// 入库单明细ID
        /// </summary>
        public long LineId { get; set; }

        /// <summary>
        /// 本次收货数量
        /// </summary>
        public decimal ReceiveQty { get; set; }

        /// <summary>
        /// 批次号
        /// </summary>
        public string? BatchNo { get; set; }

        /// <summary>
        /// 生产日期
        /// </summary>
        public DateOnly? ManufactureDate { get; set; }

        /// <summary>
        /// 有效期
        /// </summary>
        public DateOnly? ExpiryDate { get; set; }
    }
}
