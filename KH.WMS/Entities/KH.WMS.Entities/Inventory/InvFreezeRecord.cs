using KH.WMS.Core.Models.Entities;
using KH.WMS.Entities.Constants;
using SqlSugar;
using System.ComponentModel.DataAnnotations;

namespace KH.WMS.Entities.Inventory
{
    /// <summary>
    /// 库存冻结记录
    /// 用于质量问题、审计、盘点等场景下隔离库存
    /// </summary>
    [SugarTable("inv_freeze_record")]
    [SugarIndex("uk_freeze_no", nameof(FreezeNo), OrderByType.Asc)]
    [SugarIndex("idx_warehouse", nameof(WarehouseId), OrderByType.Asc)]
    [SugarIndex("idx_material", nameof(MaterialId), OrderByType.Asc)]
    [SugarIndex("idx_status", nameof(Status), OrderByType.Asc)]
    public class InvFreezeRecord : BaseEntity<long>
    {
        /// <summary>
        /// 冻结单号
        /// </summary>
        
        [SugarColumn(Length = 50, IsNullable = false, ColumnDescription = "冻结单号")]
        public string FreezeNo { get; set; } = string.Empty;

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
        /// 物料编码
        /// </summary>
        
        [SugarColumn(Length = 50, IsNullable = false, ColumnDescription = "物料编码")]
        public string MaterialCode { get; set; } = string.Empty;

        /// <summary>
        /// 物料名称
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = true, ColumnDescription = "物料名称")]
        public string? MaterialName { get; set; }

        /// <summary>
        /// 库位ID
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "库位ID")]
        public long? LocationId { get; set; }

        /// <summary>
        /// 库位编码
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnDescription = "库位编码")]
        public string? LocationCode { get; set; }

        /// <summary>
        /// 批次号
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnDescription = "批次号")]
        public string? BatchNo { get; set; }

        /// <summary>
        /// 容器编号
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnDescription = "容器编号")]
        public string? ContainerCode { get; set; }

        /// <summary>
        /// 冻结数量
        /// </summary>
        
        [SugarColumn(DecimalDigits = 3, Length = 12, IsNullable = false, ColumnDescription = "冻结数量")]
        public decimal FreezeQty { get; set; }

        /// <summary>
        /// 冻结原因
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnDescription = "冻结原因")]
        public string? FreezeReason { get; set; }

        /// <summary>
        /// 状态（FROZEN-冻结中 / UNFROZEN-已解冻）
        /// </summary>
        [SugarColumn(Length = 20, IsNullable = false, ColumnDescription = "状态", DefaultValue = BizConstants.FreezeRecordStatus.FROZEN)]
        public string Status { get; set; } = BizConstants.FreezeRecordStatus.FROZEN;

        /// <summary>
        /// 冻结时间
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "冻结时间")]
        public DateTime FreezeTime { get; set; }

        /// <summary>
        /// 解冻时间
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "解冻时间")]
        public DateTime? UnfreezeTime { get; set; }

        /// <summary>
        /// 解冻人
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnDescription = "解冻人")]
        public string? UnfreezeBy { get; set; }

        /// <summary>
        /// 解冻备注
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnDescription = "解冻备注")]
        public string? UnfreezeRemark { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnDescription = "备注")]
        public string? Remark { get; set; }

        /// <summary>
        /// 扩展字段（JSON格式，CfgDocumentField配置驱动）
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDataType = "TEXT", ColumnDescription = "扩展字段")]
        public string? ExtData { get; set; }
    }
}
