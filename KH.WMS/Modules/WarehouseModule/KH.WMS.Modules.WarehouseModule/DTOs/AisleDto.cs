namespace KH.WMS.Modules.WarehouseModule.DTOs
{
    /// <summary>
    /// 巷道信息返回 DTO
    /// </summary>
    public class AisleDto
    {
        public long Id { get; set; }
        public int AisleNo { get; set; }
        public string AisleCode { get; set; } = string.Empty;
        public string? AisleName { get; set; }
        public long WarehouseId { get; set; }
        public long? ZoneId { get; set; }
        public string? EquipmentCode { get; set; }
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
    }
}
