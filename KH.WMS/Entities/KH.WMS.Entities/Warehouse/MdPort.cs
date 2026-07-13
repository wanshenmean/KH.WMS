using SqlSugar;
using System.ComponentModel.DataAnnotations;
using KH.WMS.Core.Models.Entities;

namespace KH.WMS.Entities.Warehouse
{
    /// <summary>
    /// 站台（入库口/出库口，位于输送线上的出入库位置）
    /// </summary>
    [SugarTable("md_port")]
    [SugarIndex("uk_warehouse_port", nameof(WarehouseId), OrderByType.Asc, nameof(PortCode), OrderByType.Asc)]
    [SugarIndex("idx_type", nameof(PortType), OrderByType.Asc)]
    [SugarIndex("idx_conveyor", nameof(ConveyorLineId), OrderByType.Asc)]
    [SugarIndex("idx_zone", nameof(ZoneId), OrderByType.Asc)]
    public class MdPort : BaseEntity<long>, IEnableDisableEntity
    {
        /// <summary>
        /// 站台编码
        /// </summary>
        
        [SugarColumn(Length = 20, IsNullable = false, ColumnDescription = "站台编码")]
        public string PortCode { get; set; } = string.Empty;

        /// <summary>
        /// 站台名称
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnDescription = "站台名称")]
        public string? PortName { get; set; }

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
        [SugarColumn(IsNullable = true, ColumnDescription = "所属输送线ID")]
        public long? ConveyorLineId { get; set; }

        /// <summary>
        /// 站台类型（INBOUND-入库口 / OUTBOUND-出库口 / MIXED-混合口）
        /// </summary>
        
        [SugarColumn(Length = 20, IsNullable = false, ColumnDescription = "站台类型")]
        public string PortType { get; set; } = string.Empty;

        /// <summary>
        /// 关联设备编码
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnDescription = "关联设备编码")]
        public string? EquipmentCode { get; set; }

        /// <summary>
        /// 状态（1启用 0禁用）
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



//第一步：配置站台类型（CfgPortType）
//    TypeCode: PICKING
//    TypeName: 分拣工位
//    AllowInbound: 0
//    AllowOutbound: 0
//    AllowPicking: 1

//  第二步：创建站台实例（MdPort）
//    PortCode: PK - 01
//    PortName: 1号分拣工位
//    PortType: PICKING
//    ConveyorLineId: 关联到某条输送线
//    WarehouseId / ZoneId: 归属仓库和库区
//    EquipmentCode: 关联扫码枪等设备