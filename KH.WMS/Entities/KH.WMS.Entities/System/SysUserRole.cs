using KH.WMS.Core.Models.Entities;
using SqlSugar;
using System.ComponentModel.DataAnnotations;

namespace KH.WMS.Entities.System
{
    /// <summary>
    /// 用户角色关联
    /// </summary>
    [SugarTable("sys_user_role")]
    [SugarIndex("uk_user_role", nameof(UserId), OrderByType.Asc, nameof(RoleId), OrderByType.Asc, true)]
    [SugarIndex("idx_role", nameof(RoleId), OrderByType.Asc)]
    public class SysUserRole : BaseEntity<long>
    {

        /// <summary>
        /// 用户ID
        /// </summary>
        
        [SugarColumn(IsNullable = false, ColumnDescription = "用户ID")]
        public long UserId { get; set; }

        /// <summary>
        /// 角色ID
        /// </summary>
        
        [SugarColumn(IsNullable = false, ColumnDescription = "角色ID")]
        public long RoleId { get; set; }
    }
}
