namespace KH.WMS.Algorithms.Strategy.DTOs
{
    /// <summary>
    /// 逻辑分区DTO（包含物理库区映射）
    /// </summary>
    public class LogicalZoneDTO
    {
        /// <summary>逻辑分区ID</summary>
        public long Id { get; set; }

        /// <summary>逻辑分区编码</summary>
        public string ZoneCode { get; set; } = string.Empty;

        /// <summary>逻辑分区名称</summary>
        public string? ZoneName { get; set; }

        /// <summary>逻辑分区类型（STORAGE/PICKING/STAGING/QC）</summary>
        public string? ZoneType { get; set; }

        /// <summary>仓库ID</summary>
        public long WarehouseId { get; set; }

        /// <summary>物理库区映射列表（按优先级排序）</summary>
        public List<LogicalZonePhysicalMapping> PhysicalZones { get; set; } = new();
    }

    /// <summary>
    /// 逻辑分区-物理库区映射项
    /// </summary>
    public class LogicalZonePhysicalMapping
    {
        /// <summary>物理库区ID</summary>
        public long PhysicalZoneId { get; set; }

        /// <summary>物理库区编码</summary>
        public string PhysicalZoneCode { get; set; } = string.Empty;

        /// <summary>物理库区名称</summary>
        public string? PhysicalZoneName { get; set; }

        /// <summary>物理库区类型</summary>
        public string? PhysicalZoneType { get; set; }

        /// <summary>映射优先级（数字越小越优先）</summary>
        public int Priority { get; set; }
    }
}
