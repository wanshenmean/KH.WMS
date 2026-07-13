using System.Text.Json.Serialization;

namespace KH.WMS.Modules.SystemModule.DTOs
{
    /// <summary>
    /// 菜单按钮 DTO（存储在菜单记录的 Buttons JSON 字段中）
    /// </summary>
    public class MenuButtonDto
    {
        /// <summary>
        /// 按钮编码（英文，如 btn_add）
        /// </summary>
        [JsonPropertyName("buttonCode")]
        public string ButtonCode { get; set; } = string.Empty;

        /// <summary>
        /// 按钮名称（中文，如 新增）
        /// </summary>
        [JsonPropertyName("buttonName")]
        public string ButtonName { get; set; } = string.Empty;

        /// <summary>
        /// 权限标识（如 sys:user:add），用于前端按钮级权限控制
        /// </summary>
        [JsonPropertyName("permKey")]
        public string PermKey { get; set; } = string.Empty;

        /// <summary>
        /// 图标
        /// </summary>
        [JsonPropertyName("icon")]
        public string? Icon { get; set; }

        /// <summary>
        /// 排序号
        /// </summary>
        [JsonPropertyName("sortNo")]
        public int SortNo { get; set; }

        /// <summary>
        /// 状态 1=启用 0=禁用
        /// </summary>
        [JsonPropertyName("status")]
        public byte Status { get; set; } = 1;

        /// <summary>
        /// 备注
        /// </summary>
        [JsonPropertyName("remark")]
        public string? Remark { get; set; }
    }
}
