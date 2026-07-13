using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using KH.WMS.Algorithms.Strategy.Constants;

namespace KH.WMS.Algorithms.Strategy.DTOs
{
    public class InventoryInfoDTO
    {
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
        /// 当前所在库区编码（用于出库分配偏好按分区排序）
        /// </summary>
        public string? ZoneCode { get; set; }

        /// <summary>
        /// 库存状态（AVAILABLE-可用 / LOCKED-锁定 / QC-质检中 / FROZEN-冻结）
        /// </summary>
        public string InventoryStatus { get; set; } = AlgoConstants.InventoryStatus.AVAILABLE;

        public long Id { get; set; }

        /// <summary>
        /// 库存头ID
        /// </summary>
        public long HeaderId { get; set; }

        /// <summary>
        /// 物料ID
        /// </summary>
        public long MaterialId { get; set; }

        /// <summary>
        /// 物料编码（冗余字段，方便查询和展示）
        /// </summary>
        public string MaterialCode { get; set; } = string.Empty;

        /// <summary>
        /// 批次号
        /// </summary>
        public string? BatchNo { get; set; }

        /// <summary>
        /// 序列号（单品管理时使用）
        /// </summary>
        public string? SerialNo { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        public decimal Qty { get; set; }

        /// <summary>
        /// 单位
        /// </summary>
        public string? Unit { get; set; }

        /// <summary>
        /// 锁定数量
        /// </summary>
        public decimal LockedQty { get; set; }

        /// <summary>
        /// 生产日期
        /// </summary>
        public DateOnly? ProductionDate { get; set; }

        /// <summary>
        /// 过期日期
        /// </summary>
        public DateOnly? ExpiryDate { get; set; }

        /// <summary>
        /// 入库单号
        /// </summary>
        public string? InboundDocNo { get; set; }

        /// <summary>
        /// 供应商ID
        /// </summary>
        public long? SupplierId { get; set; }

        /// <summary>
        /// 入库时间
        /// </summary>
        public DateTime? InboundTime { get; set; }

    }
}
