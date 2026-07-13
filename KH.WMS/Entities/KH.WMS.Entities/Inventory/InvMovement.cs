using SqlSugar;
using System.ComponentModel.DataAnnotations;
using KH.WMS.Core.Models.Entities;

namespace KH.WMS.Entities.Inventory
{
    /// <summary>
    /// 库存变动记录（库存流水）
    /// 记录所有库存变动信息，包括入库、出库、转移、调整、盘点等
    /// </summary>
    [SugarTable("inv_movement")]
    [SugarIndex("idx_warehouse_material", nameof(WarehouseId), OrderByType.Asc, nameof(MaterialId), OrderByType.Asc)]
    [SugarIndex("idx_location", nameof(LocationId), OrderByType.Asc)]
    [SugarIndex("idx_batch", nameof(BatchNo), OrderByType.Asc)]
    [SugarIndex("idx_container", nameof(ContainerCode), OrderByType.Asc)]
    [SugarIndex("idx_type_direction", nameof(MovementType), OrderByType.Asc, nameof(Direction), OrderByType.Asc)]
    [SugarIndex("idx_doc", nameof(DocType), OrderByType.Asc, nameof(DocNo), OrderByType.Asc)]
    [SugarIndex("idx_time", nameof(MovementTime), OrderByType.Asc)]
    public class InvMovement : BaseEntity<long>
    {
        /// <summary>
        /// 仓库ID
        /// </summary>
        
        [SugarColumn(IsNullable = false, ColumnDescription = "仓库ID")]
        public long WarehouseId { get; set; }

        /// <summary>
        /// 物料ID
        /// </summary>
        
        [SugarColumn(IsNullable = false, ColumnDescription = "物料ID")]
        public long MaterialId { get; set; }

        /// <summary>
        /// 货位ID
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "货位ID")]
        public long? LocationId { get; set; }

        /// <summary>
        /// 批次号
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnDescription = "批次号")]
        public string? BatchNo { get; set; }

        /// <summary>
        /// 序列号
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnDescription = "序列号")]
        public string? SerialNo { get; set; }

        /// <summary>
        /// 容器编号
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnDescription = "容器编号")]
        public string? ContainerCode { get; set; }

        /// <summary>
        /// 变动类型（INBOUND-入库 / OUTBOUND-出库 / TRANSFER-转移 / ADJUST-调整 / STOCKTAKE-盘点）
        /// </summary>
        
        [SugarColumn(Length = 20, IsNullable = false, ColumnDescription = "变动类型")]
        public string MovementType { get; set; } = string.Empty;

        /// <summary>
        /// 变动方向（INCREASE-增加 / DECREASE-减少）
        /// </summary>
        
        [SugarColumn(Length = 20, IsNullable = false, ColumnDescription = "变动方向")]
        public string Direction { get; set; } = string.Empty;

        /// <summary>
        /// 变动数量
        /// </summary>
        
        [SugarColumn(DecimalDigits = 3, Length = 12, IsNullable = false, ColumnDescription = "变动数量")]
        public decimal MovementQty { get; set; }

        /// <summary>
        /// 单位
        /// </summary>
        [SugarColumn(Length = 20, IsNullable = true, ColumnDescription = "单位")]
        public string? Unit { get; set; }

        /// <summary>
        /// 变动前数量
        /// </summary>
        [SugarColumn(DecimalDigits = 3, Length = 12, IsNullable = true, ColumnDescription = "变动前数量")]
        public decimal? QtyBefore { get; set; }

        /// <summary>
        /// 变动后数量
        /// </summary>
        [SugarColumn(DecimalDigits = 3, Length = 12, IsNullable = true, ColumnDescription = "变动后数量")]
        public decimal? QtyAfter { get; set; }

        /// <summary>
        /// 关联单据类型
        /// </summary>
        [SugarColumn(Length = 30, IsNullable = true, ColumnDescription = "关联单据类型")]
        public string? DocType { get; set; }

        /// <summary>
        /// 关联单据编号
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnDescription = "关联单据编号")]
        public string? DocNo { get; set; }

        /// <summary>
        /// 变动时间
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "变动时间")]
        public DateTime MovementTime { get; set; }

        /// <summary>
        /// 操作人ID
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "操作人ID")]
        public long? OperatorId { get; set; }

        /// <summary>
        /// 操作人姓名
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnDescription = "操作人姓名")]
        public string? OperatorName { get; set; }

        /// <summary>
        /// 扩展字段（JSON格式，CfgExtField配置驱动）
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDataType = "TEXT", ColumnDescription = "扩展字段")]
        public string? ExtData { get; set; }
    }
}
