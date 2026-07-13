using SqlSugar;

namespace KH.WMS.Algorithms.Strategy.DTOs
{
    /// <summary>
    /// 巷道DTO
    /// 映射 md_aisle 表
    /// </summary>
    [SugarTable("md_aisle")]
    public class MdAisleDTO
    {
        public long Id { get; set; }

        /// <summary>巷道号</summary>
        public int AisleNo { get; set; }

        /// <summary>巷道编码</summary>
        public string AisleCode { get; set; } = string.Empty;

        /// <summary>巷道名称</summary>
        public string? AisleName { get; set; }

        /// <summary>仓库ID</summary>
        public long WarehouseId { get; set; }

        /// <summary>库区ID</summary>
        public long? ZoneId { get; set; }

        /// <summary>关联堆垛机设备编码</summary>
        public string? EquipmentCode { get; set; }

        /// <summary>状态（1启用 0禁用 2维护中）</summary>
        public byte Status { get; set; } = 1;

        /// <summary>排序号</summary>
        public int SortNo { get; set; }

        /// <summary>备注</summary>
        public string? Remark { get; set; }
    }
}
