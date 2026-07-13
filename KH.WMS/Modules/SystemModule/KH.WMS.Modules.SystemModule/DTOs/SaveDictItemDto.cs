using KH.WMS.Entities.Constants;

namespace KH.WMS.Modules.SystemModule.DTOs
{
    /// <summary>
    /// 保存字典项请求 DTO
    /// </summary>
    public class SaveDictItemDto
    {
        public long? Id { get; set; }
        public long DictTypeId { get; set; }
        public string ItemValue { get; set; } = string.Empty;
        public string ItemLabel { get; set; } = string.Empty;
        public string? TagColor { get; set; }
        public int SortOrder { get; set; }
        public byte IsActive { get; set; } = BizConstants.BoolFlag.YES;
        public string? Remark { get; set; }
    }
}
