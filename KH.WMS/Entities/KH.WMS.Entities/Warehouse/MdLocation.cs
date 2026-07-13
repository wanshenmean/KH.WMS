using SqlSugar;
using System.ComponentModel.DataAnnotations;
using KH.WMS.Core.Attributes;
using KH.WMS.Core.Models.Entities;
using KH.WMS.Entities.Constants;

namespace KH.WMS.Entities.Warehouse
{
    /// <summary>
    /// 库位（高位货架库位）
    /// </summary>
    [SugarTable("md_location")]
    [StatusFieldName(nameof(IsDisabled))]
    [SugarIndex("uk_location_code", nameof(LocationCode), OrderByType.Asc)]
    [SugarIndex("idx_warehouse", nameof(WarehouseId), OrderByType.Asc)]
    [SugarIndex("idx_zone", nameof(ZoneId), OrderByType.Asc)]
    [SugarIndex("idx_address", nameof(AisleNo), OrderByType.Asc, nameof(Side), OrderByType.Asc, nameof(RowNo), OrderByType.Asc, nameof(ColNo), OrderByType.Asc, nameof(LayerNo), OrderByType.Asc, nameof(Depth), OrderByType.Asc)]
    [SugarIndex("idx_status", nameof(Status), OrderByType.Asc)]
    [SugarIndex("idx_lock_status", nameof(LockStatus), OrderByType.Asc)]
    [SugarIndex("idx_disabled", nameof(IsDisabled), OrderByType.Asc)]
    public class MdLocation : BaseEntity<long>, IEnableDisableEntity
    {
        /// <summary>
        /// 库位编码
        /// </summary>
        [SugarColumn(Length = 30, IsNullable = false, ColumnDescription = "库位编码")]
        public string LocationCode { get; set; } = string.Empty;

        /// <summary>
        /// 仓库ID
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "仓库ID")]
        public long WarehouseId { get; set; }

        /// <summary>
        /// 仓库编码（冗余字段，查询时直接使用，无需关联仓库表获取）
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "仓库编码")]
        public string WarehouseCode { get; set; } = string.Empty;

        /// <summary>
        /// 库区ID
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "库区ID")]
        public long? ZoneId { get; set; }

        /// <summary>
        /// 库区编码（冗余字段，查询时直接使用，无需关联库区表获取）
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "库区编码")]
        public string? ZoneCode { get; set; }

        /// <summary>
        /// 巷道号（堆垛机所在巷道）
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "巷道号")]
        public int AisleNo { get; set; }

        /// <summary>
        /// 行号
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "行号")]
        public int RowNo { get; set; }

        /// <summary>
        /// 列号
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "列号")]
        public int ColNo { get; set; }

        /// <summary>
        /// 层号
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "层号")]
        public int LayerNo { get; set; }

        /// <summary>
        /// 排（巷道左右两侧，1-左排 2-右排）
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "排", DefaultValue = "1")]
        public int Side { get; set; } = 1;

        /// <summary>
        /// 深度（同一库位双深位时的前/后排，1-前排 2-后排）
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "深度", DefaultValue = "1")]
        public int Depth { get; set; } = 1;

        /// <summary>
        /// 库位类型
        /// </summary>
        [SugarColumn(Length = 20, IsNullable = true, ColumnDescription = "库位类型")]
        public string? LocationType { get; set; }

        /// <summary>
        /// ABC分类（A-高频/B-中频/C-低频），用于ABC分类货位分配策略匹配物料周转分类。
        /// 为空表示未设置，回退到所属库区的 AbcClass。
        /// </summary>
        [SugarColumn(ColumnDataType = "VARCHAR(2)", IsNullable = true, ColumnDescription = "ABC分类")]
        public string? AbcClass { get; set; }

        /// <summary>
        /// 物理状态（EMPTY-空闲 / OCCUPIED-占用）
        /// </summary>
        [SugarColumn(Length = 20, IsNullable = false, ColumnDescription = "状态", DefaultValue = "EMPTY")]
        public string Status { get; set; } = BizConstants.LocationStatus.EMPTY;

        /// <summary>
        /// 锁定状态（0-未锁定 / 1-入库预占 / 2-出库锁定 / 3-盘点冻结）
        /// 由系统根据任务类型自动设置和释放，人工不直接操作此字段。
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "锁定状态", DefaultValue = "0")]
        public byte LockStatus { get; set; } = BizConstants.LocationLockStatus.NONE;

        /// <summary>
        /// 是否禁用（0-启用 / 1-禁用）
        /// 由人工管理，用于设备维护、异常报废等长期停用场景。
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "是否禁用", DefaultValue = "0")]
        public byte IsDisabled { get; set; } = BizConstants.BoolFlag.NO;

        /// <summary>
        /// 禁用原因
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true, ColumnDescription = "禁用原因")]
        public string? DisableReason { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true, ColumnDescription = "备注")]
        public string? Remark { get; set; }

        // === 领域方法 ===

        /// <summary>
        /// 根据地址参数生成库位编码
        /// </summary>
        public void GenerateLocationCode()
        {
            LocationCode = $"{WarehouseCode}-{AisleNo}-{RowNo}-{ColNo}-{LayerNo}-{Side}-{Depth}";
        }
    }
}
