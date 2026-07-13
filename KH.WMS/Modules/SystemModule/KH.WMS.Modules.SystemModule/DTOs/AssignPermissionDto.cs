namespace KH.WMS.Modules.SystemModule.DTOs
{
    /// <summary>
    /// 分配角色权限请求 DTO
    /// </summary>
    public class AssignPermissionDto
    {
        public long RoleId { get; set; }

        /// <summary>
        /// 菜单权限项列表
        /// </summary>
        public List<MenuPermissionItem> Menus { get; set; } = new();
    }

    /// <summary>
    /// 菜单权限项（含按钮粒度控制）
    /// </summary>
    public class MenuPermissionItem
    {
        /// <summary>
        /// 菜单/目录ID
        /// </summary>
        public long PermissionId { get; set; }

        /// <summary>
        /// 允许的按钮 permKey 列表
        /// null 或空表示该菜单所有按钮都授权
        /// </summary>
        public List<string>? AllowedButtons { get; set; }
    }
}
