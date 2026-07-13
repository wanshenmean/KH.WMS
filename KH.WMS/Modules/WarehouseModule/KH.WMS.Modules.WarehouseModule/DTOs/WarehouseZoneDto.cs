namespace KH.WMS.Modules.WarehouseModule.DTOs
{
    /// <summary>
    /// 库区信息返回 DTO
    /// </summary>
    public class WarehouseZoneDto
    {
        public long Id { get; set; }
        public long WarehouseId { get; set; }
        public string WarehouseCode { get; set; } = string.Empty;
        public string ZoneCode { get; set; } = string.Empty;
        public string? ZoneName { get; set; }
        public string? ZoneType { get; set; }
        public long? ParentZoneId { get; set; }
        public int SortNo { get; set; }
        /// <summary>
        /// 库区下的库位数
        /// </summary>
        public int LocationCount { get; set; }
    }

    /// <summary>
    /// 库区树节点 DTO（支持父子级联）
    /// </summary>
    public class WarehouseZoneTreeNodeDto
    {
        public long Id { get; set; }
        public string ZoneCode { get; set; } = string.Empty;
        public string? ZoneName { get; set; }
        public string? ZoneType { get; set; }
        public long? ParentZoneId { get; set; }
        public int SortNo { get; set; }
        public List<WarehouseZoneTreeNodeDto> Children { get; set; } = new();
    }
}
