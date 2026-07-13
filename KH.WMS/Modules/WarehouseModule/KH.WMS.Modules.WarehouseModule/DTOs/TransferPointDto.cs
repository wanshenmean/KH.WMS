namespace KH.WMS.Modules.WarehouseModule.DTOs
{
    /// <summary>
    /// 接驳口信息返回 DTO
    /// </summary>
    public class TransferPointDto
    {
        public long Id { get; set; }
        public string PointCode { get; set; } = string.Empty;
        public string? PointName { get; set; }
        public long WarehouseId { get; set; }
        public long? ZoneId { get; set; }
        public long ConveyorLineId { get; set; }
        public long? AisleId { get; set; }
        public string PointType { get; set; } = "MIXED";
        public int ConveyorSequence { get; set; }
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
        /// <summary>
        /// 服务巷道名称
        /// </summary>
        public string? AisleName { get; set; }
    }
}
