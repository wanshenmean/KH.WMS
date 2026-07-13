using SqlSugar;

namespace KH.WMS.Algorithms.Strategy.DTOs
{
    /// <summary>
    /// 库存明细DTO
    /// 映射 inv_inventory_detail 表
    /// </summary>
    [SugarTable("inv_inventory_detail")]
    public class InvInventoryDetailDTO
    {
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
