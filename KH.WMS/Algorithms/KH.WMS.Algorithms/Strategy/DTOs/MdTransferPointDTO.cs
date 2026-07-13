using SqlSugar;

namespace KH.WMS.Algorithms.Strategy.DTOs
{
    /// <summary>
    /// 接驳口DTO
    /// 映射 md_transfer_point 表
    /// </summary>
    [SugarTable("md_transfer_point")]
    public class MdTransferPointDTO
    {
        public long Id { get; set; }

        /// <summary>接驳口编码</summary>
        public string PointCode { get; set; } = string.Empty;

        /// <summary>接驳口名称</summary>
        public string? PointName { get; set; }

        /// <summary>仓库ID</summary>
        public long WarehouseId { get; set; }

        /// <summary>库区ID</summary>
        public long? ZoneId { get; set; }

        /// <summary>所属输送线ID</summary>
        public long ConveyorLineId { get; set; }

        /// <summary>服务巷道ID</summary>
        public long? AisleId { get; set; }

        /// <summary>接驳口类型（INBOUND/OUTBOUND/MIXED）</summary>
        public string PointType { get; set; } = "MIXED";

        /// <summary>状态（1启用 0禁用 2维护中）</summary>
        public byte Status { get; set; } = 1;
    }
}
