using KH.WMS.Algorithms.Strategy.Constants;
using SqlSugar;

namespace KH.WMS.Algorithms.Strategy.DTOs
{
    /// <summary>
    /// 库存头DTO
    /// 映射 inv_inventory_header 表
    /// </summary>
    [SugarTable("inv_inventory_header")]
    public class InvInventoryHeaderDTO
    {
        public long Id { get; set; }

        /// <summary>
        /// 容器编号
        /// </summary>
        public string ContainerCode { get; set; } = string.Empty;

        /// <summary>
        /// 仓库ID
        /// </summary>
        public long WarehouseId { get; set; }

        /// <summary>
        /// 仓库编码（冗余字段，方便查询和展示）
        /// </summary>
        public string? WarehouseCode { get; set; } = string.Empty;

        /// <summary>
        /// 当前所在货位ID
        /// </summary>
        public long? LocationId { get; set; }

        /// <summary>
        /// 当前所在货位编码（冗余字段，方便查询和展示）
        /// </summary>
        public string? LocationCode { get; set; } = string.Empty;

        /// <summary>
        /// 库存状态（AVAILABLE-可用 / LOCKED-锁定 / QC-质检中 / FROZEN-冻结）
        /// </summary>
        public string InventoryStatus { get; set; } = AlgoConstants.InventoryStatus.AVAILABLE;

        /// <summary>
        /// 明细数量（该容器有几种物料）
        /// </summary>
        public int DetailCount { get; set; }

        /// <summary>
        /// 入库时间
        /// </summary>
        public DateTime InboundTime { get; set; }

        /// <summary>
        /// 最后入库时间
        /// </summary>
        public DateTime? LastInboundTime { get; set; }

        /// <summary>
        /// 最后盘点时间
        /// </summary>
        public DateTime? LastStocktakeTime { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string? Remark { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        public string? CreatedBy { get; set; }

        /// <summary>
        /// 创建人名称
        /// </summary>
        public string? CreatedByName { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedTime { get; set; }

        /// <summary>
        /// 最后修改人
        /// </summary>
        public string? LastModifiedBy { get; set; }

        /// <summary>
        /// 最后修改人名称
        /// </summary>
        public string? LastModifiedByName { get; set; }

        /// <summary>
        /// 最后修改时间
        /// </summary>
        public DateTime? LastModifiedTime { get; set; }
    }
}
