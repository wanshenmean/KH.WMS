using SqlSugar;

namespace KH.WMS.Algorithms.Strategy.DTOs
{
    /// <summary>
    /// 逻辑分区-物理库区映射 DTO（映射 md_logical_zone_mapping，Algorithms 包内自有的数据模型）
    /// </summary>
    [SugarTable("md_logical_zone_mapping")]
    public class MdLogicalZoneMappingDTO
    {
        public long Id { get; set; }

        /// <summary>逻辑分区ID</summary>
        public long LogicalZoneId { get; set; }

        /// <summary>物理库区ID</summary>
        public long PhysicalZoneId { get; set; }

        /// <summary>优先级（数字越小越优先）</summary>
        public int Priority { get; set; }

        /// <summary>状态（1启用 0禁用）</summary>
        public byte Status { get; set; } = 1;
    }
}
