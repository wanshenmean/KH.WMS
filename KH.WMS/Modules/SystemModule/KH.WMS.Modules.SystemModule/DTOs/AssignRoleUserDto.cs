namespace KH.WMS.Modules.SystemModule.DTOs
{
    /// <summary>
    /// 角色分配用户请求 DTO
    /// </summary>
    public class AssignRoleUserDto
    {
        public long RoleId { get; set; }
        public List<long> UserIds { get; set; } = new();
    }
}
