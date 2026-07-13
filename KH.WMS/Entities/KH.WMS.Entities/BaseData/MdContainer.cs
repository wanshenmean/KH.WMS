using KH.WMS.Core.Models.Entities;
using KH.WMS.Entities.Constants;
using SqlSugar;
using System.ComponentModel.DataAnnotations;

namespace KH.WMS.Entities.BaseData
{
    [SugarTable("md_container")]
    [SugarIndex("uk_container_no", nameof(ContainerNo), OrderByType.Asc)]
    [SugarIndex("idx_type", nameof(ContainerTypeId), OrderByType.Asc)]
    [SugarIndex("idx_location", nameof(CurrentLocationId), OrderByType.Asc)]
    [SugarIndex("idx_warehouse", nameof(WarehouseId), OrderByType.Asc)]
    /// <summary>
    /// 容器
    /// </summary>
    public class MdContainer : BaseEntity<long>
    {

        /// <summary>
        /// 容器编号
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = false, ColumnDescription = "容器编号")]
        public string ContainerNo { get; set; } = string.Empty;

        /// <summary>
        /// 容器类型ID
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "容器类型ID")]
        public long ContainerTypeId { get; set; }

        /// <summary>
        /// 仓库ID
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "仓库ID")]
        public long? WarehouseId { get; set; }

        /// <summary>
        /// 当前库位ID
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "当前库位ID")]
        public long? CurrentLocationId { get; set; }

        /// <summary>
        /// 状态（EMPTY/IN_USE/LOCKED/MOVING）
        /// </summary>
        [SugarColumn(Length = 20, IsNullable = false, ColumnDescription = "状态", DefaultValue = "EMPTY")]
        public string Status { get; set; } = BizConstants.ContainerStatus.EMPTY;
    }
}
