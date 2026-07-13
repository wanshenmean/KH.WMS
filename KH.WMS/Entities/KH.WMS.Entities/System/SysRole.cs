using KH.WMS.Core.Models.Entities;
using SqlSugar;
using System.ComponentModel.DataAnnotations;

namespace KH.WMS.Entities.System
{
    /// <summary>
    /// 系统角色
    /// </summary>
    [SugarTable("sys_role")]
    [SugarIndex("uk_role_code", nameof(RoleCode), OrderByType.Asc, true)]
    public class SysRole : BaseEntity<long>, IEnableDisableEntity
    {

        /// <summary>
        /// 角色编码
        /// </summary>
        
        [SugarColumn(Length = 30, IsNullable = false, ColumnDescription = "角色编码")]
        public string RoleCode { get; set; } = string.Empty;

        /// <summary>
        /// 角色名称
        /// </summary>
        
        [SugarColumn(Length = 50, IsNullable = false, ColumnDescription = "角色名称")]
        public string RoleName { get; set; } = string.Empty;

        /// <summary>
        /// 父级ID
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "父级ID", DefaultValue = "0")]
        public long ParentId { get; set; } = 0;

        /// <summary>
        /// 排序号
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "排序号", DefaultValue = "0")]
        public int SortNo { get; set; } = 0;

        /// <summary>
        /// 状态
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "状态", DefaultValue = "1")]
        public byte Status { get; set; } = 1;

        /// <summary>
        /// 是否系统角色
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "是否系统角色", DefaultValue = "0")]
        public byte IsSystem { get; set; } = 0;

        /// <summary>
        /// 数据范围
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "数据范围", DefaultValue = "0")]
        public byte DataScope { get; set; } = 0;

        /// <summary>
        /// 备注
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnDescription = "备注")]
        public string? Remark { get; set; }
    }
}
