using SqlSugar;

namespace KH.WMS.Algorithms.Strategy.DTOs
{
    /// <summary>
    /// 逻辑分区 DTO（映射 md_logical_zone，Algorithms 包内自有的数据模型）
    /// </summary>
    [SugarTable("md_logical_zone")]
    public class MdLogicalZoneDTO
    {
        public long Id { get; set; }

        /// <summary>逻辑分区编码</summary>
        public string ZoneCode { get; set; } = string.Empty;

        /// <summary>逻辑分区名称</summary>
        public string? ZoneName { get; set; }

        /// <summary>逻辑分区类型（PICKING/STORAGE/STAGING/QC）</summary>
        public string? ZoneType { get; set; }

        /// <summary>仓库ID</summary>
        public long WarehouseId { get; set; }

        /// <summary>排序号</summary>
        public int SortNo { get; set; }

        /// <summary>状态（1启用 0禁用）</summary>
        public byte Status { get; set; } = 1;
    }
}
