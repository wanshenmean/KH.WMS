namespace KH.WMS.Modules.InboundModule.DTOs
{
    /// <summary>
    /// 组盘DTO
    /// </summary>
    public class ContainerBindDto
    {
        /// <summary>
        /// 容器编号
        /// </summary>
        public string ContainerCode { get; set; } = string.Empty;

        /// <summary>
        /// 入库单明细ID
        /// </summary>
        public long InboundOrderLineId { get; set; }

        /// <summary>
        /// 组盘数量
        /// </summary>
        public decimal Qty { get; set; }

        /// <summary>
        /// 批次号（可选，默认取明细行的）
        /// </summary>
        public string? BatchNo { get; set; }

        /// <summary>
        /// 生产日期
        /// </summary>
        public DateOnly? ProductionDate { get; set; }

        /// <summary>
        /// 有效期
        /// </summary>
        public DateOnly? ExpiryDate { get; set; }
    }
}
