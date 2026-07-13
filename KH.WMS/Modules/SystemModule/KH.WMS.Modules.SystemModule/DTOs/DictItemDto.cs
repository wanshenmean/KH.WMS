namespace KH.WMS.Modules.SystemModule.DTOs
{
    /// <summary>
    /// 统一字典项 DTO（兼容静态字典和SQL动态字典）
    /// </summary>
    public class DictItemDto
    {
        public string ItemValue { get; set; } = string.Empty;
        public string ItemLabel { get; set; } = string.Empty;
        public string? TagColor { get; set; }
        public int SortOrder { get; set; }
    }
}
