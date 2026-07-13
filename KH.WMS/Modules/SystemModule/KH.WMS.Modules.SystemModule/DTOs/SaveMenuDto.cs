

using KH.WMS.Entities.Constants;

namespace KH.WMS.Modules.SystemModule.DTOs
{
    /// <summary>
    /// 保存菜单请求 DTO
    /// </summary>
    public class SaveMenuDto
    {
        public long? Id { get; set; }
        public string PermissionCode { get; set; } = string.Empty;
        public string PermissionName { get; set; } = string.Empty;
        public long ParentId { get; set; }
        public byte MenuType { get; set; }
        public string? Path { get; set; }
        public string? Component { get; set; }
        public string? Icon { get; set; }
        public int SortNo { get; set; }
        public byte IsVisible { get; set; } = BizConstants.BoolFlag.YES;
        public byte Status { get; set; } = BizConstants.BoolFlag.YES;
        public byte IsExternal { get; set; }
        public byte IsCache { get; set; }

        /// <summary>
        /// 按钮权限列表
        /// </summary>
        public List<MenuButtonDto>? Buttons { get; set; }

        public string? Remark { get; set; }
    }
}
