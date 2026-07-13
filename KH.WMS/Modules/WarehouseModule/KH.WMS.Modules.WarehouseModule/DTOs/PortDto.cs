namespace KH.WMS.Modules.WarehouseModule.DTOs
{
    /// <summary>
    /// 站台信息返回 DTO
    /// </summary>
    public class PortDto
    {
        public long Id { get; set; }
        public string PortCode { get; set; } = string.Empty;
        public string? PortName { get; set; }
        public long WarehouseId { get; set; }
        public long? ZoneId { get; set; }
        public long? ConveyorLineId { get; set; }
        public string PortType { get; set; } = string.Empty;
        public int ConveyorSequence { get; set; }
        public string? EquipmentCode { get; set; }
        public byte Status { get; set; }
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
        /// 所属输送线名称
        /// </summary>
        public string? ConveyorLineName { get; set; }
    }
}
