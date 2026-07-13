using KH.WMS.Core.Models.Entities;
using SqlSugar;
using System.ComponentModel.DataAnnotations;

namespace KH.WMS.Entities.System
{
    /// <summary>
    /// 系统用户
    /// </summary>
    [SugarTable("sys_user")]
    [SugarIndex("uk_user_name", nameof(UserName), OrderByType.Asc, true)]
    public class SysUser : BaseEntity<long>, IEnableDisableEntity
    {
        /// <summary>
        /// 用户名
        /// </summary>
        
        [SugarColumn(Length = 50, IsNullable = false, ColumnDescription = "用户名")]
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// 密码
        /// </summary>
        [SugarColumn(Length = 200, IsNullable = false, ColumnDescription = "密码")]
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// 真实姓名
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnDescription = "真实姓名")]
        public string? RealName { get; set; }

        /// <summary>
        /// 头像
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnDescription = "头像")]
        public string? Avatar { get; set; }

        /// <summary>
        /// 部门ID
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "部门ID")]
        public long? DepartmentId { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "状态", DefaultValue = "1")]
        public byte Status { get; set; } = 1;

        /// <summary>
        /// 是否系统用户
        /// </summary>
        [SugarColumn(IsNullable = false, ColumnDescription = "是否系统用户", DefaultValue = "0")]
        public byte IsSystem { get; set; } = 0;

        /// <summary>
        /// 登录IP
        /// </summary>
        [SugarColumn(Length = 50, IsNullable = true, ColumnDescription = "登录IP")]
        public string? LoginIp { get; set; }

        /// <summary>
        /// 登录时间
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "登录时间")]
        public DateTime? LoginTime { get; set; }

        /// <summary>
        /// 密码更新时间
        /// </summary>
        [SugarColumn(IsNullable = true, ColumnDescription = "密码更新时间")]
        public DateTime? PasswordUpdateTime { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [SugarColumn(Length = 500, IsNullable = true, ColumnDescription = "备注")]
        public string? Remark { get; set; }
    }
}
