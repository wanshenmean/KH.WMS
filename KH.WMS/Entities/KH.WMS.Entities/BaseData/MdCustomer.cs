using SqlSugar;
using System.ComponentModel.DataAnnotations;
using KH.WMS.Core.Models.Entities;

namespace KH.WMS.Entities.BaseData
{
    /// <summary>
    /// 客户
    /// </summary>
    [SugarTable("md_customer")]
    [SugarIndex("uk_customer_code", nameof(CustomerCode), OrderByType.Asc)]
    public class MdCustomer : BaseEntity<long>, IEnableDisableEntity
    {

        /// <summary>
        /// 客户编码
        /// </summary>
        [SugarColumn(Length = 30, IsNullable = false, ColumnDescription = "客户编码")]
        public string CustomerCode { get; set; } = string.Empty;

        /// <summary>
        /// 客户名称
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = false, ColumnDescription = "客户名称")]
        public string CustomerName { get; set; } = string.Empty;

        /// <summary>
        /// 联系人
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnDescription = "联系人")]
        public string? Contact { get; set; }

        /// <summary>
        /// 联系电话
        /// </summary>
        [SugarColumn(Length = 20, IsNullable = true, ColumnDescription = "联系电话")]
        public string? ContactPhone { get; set; }

        /// <summary>
        /// 地址
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnDescription = "地址")]
        public string? Address { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "状态", DefaultValue = "1")]
        public byte Status { get; set; } = 1;

        /// <summary>
        /// 扩展字段（JSON格式，CfgExtField配置驱动）
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDataType = "TEXT", ColumnDescription = "扩展字段")]
        public string? ExtData { get; set; }
    }
}
