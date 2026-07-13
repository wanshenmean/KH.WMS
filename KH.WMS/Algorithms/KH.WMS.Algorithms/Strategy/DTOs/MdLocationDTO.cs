using KH.WMS.Algorithms.Strategy.Constants;
using SqlSugar;

namespace KH.WMS.Algorithms.Strategy.DTOs
{
    /// <summary>
    /// 库位DTO
    /// 映射 md_location 表
    /// </summary>
    [SugarTable("md_location")]
    public class MdLocationDTO
    {
        public long Id { get; set; }

        /// <summary>
        /// 库位编码
        /// </summary>
        public string LocationCode { get; set; } = string.Empty;

        /// <summary>
        /// 仓库ID
        /// </summary>
        public long WarehouseId { get; set; }

        /// <summary>
        /// 仓库编码（冗余字段，查询时直接使用，无需关联仓库表获取）
        /// </summary>
        public string WarehouseCode { get; set; } = string.Empty;

        /// <summary>
        /// 库区ID
        /// </summary>
        public long? ZoneId { get; set; }

        /// <summary>
        /// 库区编码（冗余字段，查询时直接使用，无需关联库区表获取）
        /// </summary>
        public string? ZoneCode { get; set; }

        /// <summary>
        /// 巷道号（堆垛机所在巷道）
        /// </summary>
        public int AisleNo { get; set; }

        /// <summary>
        /// 行号
        /// </summary>
        public int RowNo { get; set; }

        /// <summary>
        /// 列号
        /// </summary>
        public int ColNo { get; set; }

        /// <summary>
        /// 层号
        /// </summary>
        public int LayerNo { get; set; }

        /// <summary>
        /// 排（巷道左右两侧，1-左排 2-右排）
        /// </summary>
        public int Side { get; set; } = 1;

        /// <summary>
        /// 深度（同一库位双深位时的前/后排，1-前排 2-后排）
        /// </summary>
        public int Depth { get; set; } = 1;

        /// <summary>
        /// 库位类型
        /// </summary>
        public string? LocationType { get; set; }

        /// <summary>
        /// ABC分类（A/B/C，可为空；为空时回退到所属库区AbcClass）
        /// </summary>
        public string? AbcClass { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public string Status { get; set; } = AlgoConstants.LocationStatus.EMPTY;

        /// <summary>
        /// 锁定状态
        /// </summary>
        public byte LockStatus { get; set; } = 0;

        /// <summary>
        /// 是否禁用
        /// </summary>
        public byte IsDisabled { get; set; } = 0;

        /// <summary>
        /// 禁用原因
        /// </summary>
        public string? DisableReason { get; set; }

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
