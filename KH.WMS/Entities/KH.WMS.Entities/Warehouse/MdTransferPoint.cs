using SqlSugar;
using System.ComponentModel.DataAnnotations;
using KH.WMS.Core.Models.Entities;
using KH.WMS.Entities.Constants;

namespace KH.WMS.Entities.Warehouse
{
    /// <summary>
    /// 接驳口（输送线与堆垛机的交接点）
    /// 入库时：输送线搬运终点，堆垛机从这里取货放到货位
    /// 出库时：输送线搬运起点，堆垛机从货位取货放到这里
    /// </summary>
    [SugarTable("md_transfer_point")]
    [SugarIndex("uk_warehouse_point", nameof(WarehouseId), OrderByType.Asc, nameof(PointCode), OrderByType.Asc)]
    [SugarIndex("idx_conveyor", nameof(ConveyorLineId), OrderByType.Asc)]
    [SugarIndex("idx_aisle", nameof(AisleId), OrderByType.Asc)]
    [SugarIndex("idx_status", nameof(Status), OrderByType.Asc)]
    public class MdTransferPoint : BaseEntity<long>, IEnableDisableEntity
    {
        /// <summary>
        /// 接驳口编码
        /// </summary>
        
        [SugarColumn(Length = 20, IsNullable = false, ColumnDescription = "接驳口编码")]
        public string PointCode { get; set; } = string.Empty;

        /// <summary>
        /// 接驳口名称
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnDescription = "接驳口名称")]
        public string? PointName { get; set; }

        /// <summary>
        /// 仓库ID
        /// </summary>
        
        [SugarColumn(IsNullable = false, ColumnDescription = "仓库ID")]
        public long WarehouseId { get; set; }

        /// <summary>
        /// 库区ID
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "库区ID")]
        public long? ZoneId { get; set; }

        /// <summary>
        /// 所属输送线ID
        /// </summary>
        
        [SugarColumn(IsNullable = false, ColumnDescription = "所属输送线ID")]
        public long ConveyorLineId { get; set; }

        /// <summary>
        /// 服务巷道ID（该接驳口服务于哪条巷道）
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "服务巷道ID")]
        public long? AisleId { get; set; }

        /// <summary>
        /// 接驳口类型（INBOUND-入库接驳 / OUTBOUND-出库接驳 / MIXED-混合）
        /// </summary>
        [SugarColumn(Length = 20, IsNullable = false, ColumnDescription = "接驳口类型", DefaultValue = BizConstants.TransferPointType.MIXED)]
        public string PointType { get; set; } = BizConstants.TransferPointType.MIXED;

        /// <summary>
        /// 状态（1启用 0禁用 2维护中）
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "状态", DefaultValue = "1")]
        public byte Status { get; set; } = 1;

        /// <summary>
        /// 备注
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true, ColumnDescription = "备注")]
        public string? Remark { get; set; }
    }
}
