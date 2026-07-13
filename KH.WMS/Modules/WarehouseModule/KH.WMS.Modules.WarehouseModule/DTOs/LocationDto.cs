using KH.WMS.Entities.Constants;

namespace KH.WMS.Modules.WarehouseModule.DTOs
{
    /// <summary>
    /// 库位信息返回 DTO
    /// </summary>
    public class LocationDto
    {
        public long Id { get; set; }
        public string LocationCode { get; set; } = string.Empty;
        public long WarehouseId { get; set; }
        public string WarehouseCode { get; set; } = string.Empty;
        public long? ZoneId { get; set; }
        public string? ZoneCode { get; set; }
        public int AisleNo { get; set; }
        public int RowNo { get; set; }
        public int ColNo { get; set; }
        public int LayerNo { get; set; }
        public int Depth { get; set; }
        public int Side { get; set; }
        public string? LocationType { get; set; }
        public string Status { get; set; } = BizConstants.LocationStatus.EMPTY;
        public byte LockStatus { get; set; }
        public byte IsDisabled { get; set; }
        public string? DisableReason { get; set; }
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

    /// <summary>
    /// 库位状态变更请求 DTO
    /// </summary>
    public class LocationStatusChangeDto
    {
        /// <summary>
        /// 库位ID列表
        /// </summary>
        public List<long> LocationIds { get; set; } = new();
        /// <summary>
        /// 目标物理状态（EMPTY / OCCUPIED）
        /// </summary>
        public string Status { get; set; } = string.Empty;
        /// <summary>
        /// 原因说明
        /// </summary>
        public string? Reason { get; set; }
    }

    /// <summary>
    /// 库位地址编码生成请求 DTO
    /// </summary>
    public class LocationBatchGenerateDto
    {
        /// <summary>
        /// 仓库ID
        /// </summary>
        public long WarehouseId { get; set; }
        /// <summary>
        /// 库区ID
        /// </summary>
        public long? ZoneId { get; set; }
        /// <summary>
        /// 巷道号
        /// </summary>
        public int AisleNo { get; set; }
        /// <summary>
        /// 起始行号
        /// </summary>
        public int StartRow { get; set; }
        /// <summary>
        /// 结束行号
        /// </summary>
        public int EndRow { get; set; }
        /// <summary>
        /// 起始列号
        /// </summary>
        public int StartCol { get; set; }
        /// <summary>
        /// 结束列号
        /// </summary>
        public int EndCol { get; set; }
        /// <summary>
        /// 起始层号
        /// </summary>
        public int StartLayer { get; set; }
        /// <summary>
        /// 结束层号
        /// </summary>
        public int EndLayer { get; set; }
        /// <summary>
        /// 排（1-左排 2-右排）
        /// </summary>
        public int Side { get; set; } = 1;
        /// <summary>
        /// 深度（1-单深 2-双深）
        /// </summary>
        public int Depth { get; set; } = 1;
        /// <summary>
        /// 库位编码前缀（如 A01）
        /// </summary>
        public string? CodePrefix { get; set; }
    }
}
