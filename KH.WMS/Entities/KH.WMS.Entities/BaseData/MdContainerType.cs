using KH.WMS.Core.Models.Entities;
using SqlSugar;
using System.ComponentModel.DataAnnotations;

namespace KH.WMS.Entities.BaseData
{
    [SugarTable("md_container_type")]
    [SugarIndex("uk_type_code", nameof(TypeCode), OrderByType.Asc)]
    /// <summary>
    /// 容器类型
    /// </summary>
    public class MdContainerType : BaseEntity<long>, IEnableDisableEntity
    {

        /// <summary>
        /// 类型编码
        /// </summary>
        [SugarColumn(Length = 20, IsNullable = false, ColumnDescription = "类型编码")]
        public string TypeCode { get; set; } = string.Empty;

        /// <summary>
        /// 类型名称
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = false, ColumnDescription = "类型名称")]
        public string TypeName { get; set; } = string.Empty;

        /// <summary>
        /// 状态
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "状态", DefaultValue = "1")]
        public byte Status { get; set; } = 1;
    }
}
