using SqlSugar;

namespace KH.WMS.Algorithms.Strategy.DTOs
{
    /// <summary>
    /// 站台DTO
    /// 映射 md_port 表
    /// </summary>
    [SugarTable("md_port")]
    public class MdPortDTO
    {
        public long Id { get; set; }

        /// <summary>站台编码</summary>
        public string PortCode { get; set; } = string.Empty;

        /// <summary>站台名称</summary>
        public string? PortName { get; set; }

        /// <summary>仓库ID</summary>
        public long WarehouseId { get; set; }

        /// <summary>库区ID</summary>
        public long? ZoneId { get; set; }

        /// <summary>所属输送线ID</summary>
        public long? ConveyorLineId { get; set; }

        /// <summary>站台类型（INBOUND-入库口 / OUTBOUND-出库口 / MIXED-混合口）</summary>
        public string PortType { get; set; } = string.Empty;

        /// <summary>关联设备编码</summary>
        public string? EquipmentCode { get; set; }

        /// <summary>状态（1启用 0禁用）</summary>
        public byte Status { get; set; } = 1;

        /// <summary>备注</summary>
        public string? Remark { get; set; }
    }
}
