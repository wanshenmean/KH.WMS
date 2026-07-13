using KH.WMS.Entities.Constants;

namespace KH.WMS.Modules.SystemModule.DTOs
{
    /// <summary>
    /// 保存角色请求 DTO
    /// </summary>
    public class SaveRoleDto
    {
        public long? Id { get; set; }
        public string RoleCode { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public long ParentId { get; set; }
        public int SortNo { get; set; }
        public byte Status { get; set; } = BizConstants.BoolFlag.YES;
        public byte DataScope { get; set; }
        public string? Remark { get; set; }
    }
}
