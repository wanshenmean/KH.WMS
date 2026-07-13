namespace KH.WMS.Modules.WarehouseModule.DTOs
{
    /// <summary>
    /// 输送线信息返回 DTO
    /// </summary>
    public class ConveyorLineDto
    {
        public long Id { get; set; }
        public string ConveyorCode { get; set; } = string.Empty;
        public string? ConveyorName { get; set; }
        public long WarehouseId { get; set; }
        public long? ZoneId { get; set; }
        public byte Status { get; set; }
        public int SortNo { get; set; }
        public string? Remark { get; set; }
        /// <summary>
        /// 仓库名称
        /// </summary>
        public string? WarehouseName { get; set; }
        /// <summary>
        /// 库区名称
        /// </summary>
        public string? ZoneName { get; set; }
        /// <summary>
        /// 输送线上的站台数量
        /// </summary>
        public int PortCount { get; set; }
        /// <summary>
        /// 输送线上的接驳口数量
        /// </summary>
        public int TransferPointCount { get; set; }
    }
}
