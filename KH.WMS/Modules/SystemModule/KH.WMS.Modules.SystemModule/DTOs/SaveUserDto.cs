using KH.WMS.Entities.Constants;

namespace KH.WMS.Modules.SystemModule.DTOs
{
    /// <summary>
    /// 保存用户请求 DTO
    /// </summary>
    public class SaveUserDto
    {
        public long? Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string? Password { get; set; }
        public string? RealName { get; set; }
        public string? Avatar { get; set; }
        public long? DepartmentId { get; set; }
        public byte Status { get; set; } = BizConstants.BoolFlag.YES;
        public string? Remark { get; set; }
    }
}
