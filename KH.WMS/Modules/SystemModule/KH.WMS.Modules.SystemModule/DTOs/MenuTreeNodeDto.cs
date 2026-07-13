

namespace KH.WMS.Modules.SystemModule.DTOs
{
    /// <summary>
    /// 菜单树节点 DTO
    /// </summary>
    public class MenuTreeNodeDto
    {
        public long Id { get; set; }
        public string PermissionCode { get; set; } = string.Empty;
        public string PermissionName { get; set; } = string.Empty;
        public long ParentId { get; set; }
        public byte MenuType { get; set; }
        public string? Path { get; set; }
        public string? Component { get; set; }
        public string? Icon { get; set; }
        public int SortNo { get; set; }
        public byte IsVisible { get; set; }
        public byte Status { get; set; }
        public byte IsExternal { get; set; }
        public byte IsCache { get; set; }
        public string? Remark { get; set; }

        /// <summary>
        /// 按钮权限列表（仅菜单类型有值）
        /// </summary>
        public List<MenuButtonDto>? Buttons { get; set; }

        /// <summary>
        /// 子节点
        /// </summary>
        public List<MenuTreeNodeDto>? Children { get; set; }
    }
}
