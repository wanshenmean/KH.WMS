namespace KH.WMS.Modules.SystemModule.DTOs
{
    /// <summary>
    /// 用户信息返回 DTO
    /// </summary>
    public class UserDto
    {
        public long Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string? RealName { get; set; }
        public string? Avatar { get; set; }
        public long? DepartmentId { get; set; }
        public byte Status { get; set; }
        public byte IsSystem { get; set; }
        public string? LoginIp { get; set; }
        public DateTime? LoginTime { get; set; }
        public string? Remark { get; set; }
        /// <summary>
        /// 用户角色ID列表
        /// </summary>
        public List<long> RoleIds { get; set; } = new();
        /// <summary>
        /// 用户角色名称列表
        /// </summary>
        public List<string> RoleNames { get; set; } = new();
    }
}
