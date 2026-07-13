namespace KH.WMS.Modules.InboundModule.DTOs
{
    /// <summary>
    /// 收货并组盘DTO
    /// </summary>
    public class ReceiveAndBindDto
    {
        /// <summary>
        /// 入库单ID
        /// </summary>
        public long InboundOrderId { get; set; }

        /// <summary>
        /// 收货明细
        /// </summary>
        public List<ReceiveLineDto> ReceiveLines { get; set; } = new();

        /// <summary>
        /// 组盘明细
        /// </summary>
        public List<ContainerBindDto> ContainerBinds { get; set; } = new();
    }
}
