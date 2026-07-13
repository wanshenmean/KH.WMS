namespace KH.WMS.Modules.SystemModule.DTOs
{
    /// <summary>
    /// 用户分配角色请求 DTO
    /// </summary>
    public class AssignUserRoleDto
    {
        public long UserId { get; set; }
        public List<long> RoleIds { get; set; } = new();
    }
}
