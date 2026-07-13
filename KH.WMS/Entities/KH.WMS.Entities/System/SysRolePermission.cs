using KH.WMS.Core.Models.Entities;
using SqlSugar;
using System.ComponentModel.DataAnnotations;

namespace KH.WMS.Entities.System
{
    /// <summary>
    /// 角色权限关联
    /// </summary>
    [SugarTable("sys_role_permission")]
    [SugarIndex("uk_role_permission", nameof(RoleId), OrderByType.Asc, nameof(PermissionId), OrderByType.Asc, true)]
    [SugarIndex("idx_permission", nameof(PermissionId), OrderByType.Asc)]
    public class SysRolePermission : BaseEntity<long>
    {

        /// <summary>
        /// 角色ID
        /// </summary>
        
        [SugarColumn(IsNullable = false, ColumnDescription = "角色ID")]
        public long RoleId { get; set; }

        /// <summary>
        /// 权限ID
        /// </summary>
        
        [SugarColumn(IsNullable = false, ColumnDescription = "权限ID")]
        public long PermissionId { get; set; }

        /// <summary>
        /// 允许的按钮权限（JSON 数组，存储 permKey 列表）
        /// null 或空表示该菜单所有按钮都授权，有值则仅授权列表中的按钮
        /// </summary>
        [SugarColumn(ColumnDataType = "nvarchar(max)", IsNullable = true,
            ColumnDescription = "允许的按钮权限JSON")]
        public string? AllowedButtons { get; set; }
    }
}
