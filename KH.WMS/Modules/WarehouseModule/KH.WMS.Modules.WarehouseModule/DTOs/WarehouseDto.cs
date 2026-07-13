namespace KH.WMS.Modules.WarehouseModule.DTOs
{
    /// <summary>
    /// 仓库信息返回 DTO（含统计信息）
    /// </summary>
    public class WarehouseDto
    {
        public long Id { get; set; }
        public string WarehouseCode { get; set; } = string.Empty;
        public string WarehouseName { get; set; } = string.Empty;
        public string? WarehouseType { get; set; }
        public byte Status { get; set; }
        public string? Remark { get; set; }
        /// <summary>
        /// 库区数量
        /// </summary>
        public int ZoneCount { get; set; }
        /// <summary>
        /// 总库位数
        /// </summary>
        public int TotalLocations { get; set; }
    }
}
