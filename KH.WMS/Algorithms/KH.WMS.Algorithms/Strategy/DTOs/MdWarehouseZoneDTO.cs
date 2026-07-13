using SqlSugar;

namespace KH.WMS.Algorithms.Strategy.DTOs
{
    /// <summary>
    /// 库区DTO
    /// 映射 md_warehouse_zone 表
    /// </summary>
    [SugarTable("md_warehouse_zone")]
    public class MdWarehouseZoneDTO
    {
        public long Id { get; set; }

        /// <summary>仓库ID</summary>
        public long WarehouseId { get; set; }

        /// <summary>库区编码</summary>
        public string ZoneCode { get; set; } = string.Empty;

        /// <summary>库区名称</summary>
        public string? ZoneName { get; set; }

        /// <summary>库区类型（如 HIGH_FREQ/MID_FREQ/LOW_FREQ/PICKING/STORAGE）</summary>
        public string? ZoneType { get; set; }

        /// <summary>父库区ID</summary>
        public long? ParentZoneId { get; set; }

        /// <summary>ABC分类（A/B/C，可为空；作为该库区下货位AbcClass为空时的回退默认值）</summary>
        public string? AbcClass { get; set; }

        /// <summary>排序号</summary>
        public int SortNo { get; set; }

        /// <summary>状态（1启用 0禁用）</summary>
        public byte Status { get; set; } = 1;
    }
}
