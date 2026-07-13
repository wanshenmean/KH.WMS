using KH.WMS.Core.Constants;
using KH.WMS.Config.Abstractions;

namespace KH.WMS.Config.DTOs
{
    /// <summary>
    /// 批量修改配置值请求
    /// </summary>
    public class BatchUpdateConfigRequest
    {
        /// <summary>
        /// 配置修改项列表
        /// </summary>
        public List<ConfigUpdateItem> Items { get; set; } = new List<ConfigUpdateItem>();
    }

    /// <summary>
    /// 单个配置修改项
    /// </summary>
    public class ConfigUpdateItem
    {
        /// <summary>
        /// 配置ID
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 配置键
        /// </summary>
        public string ConfigKey { get; set; } = string.Empty;

        /// <summary>
        /// 新的配置值
        /// </summary>
        public string ConfigValue { get; set; } = string.Empty;
    }

    /// <summary>
    /// 按分组查询的配置响应
    /// </summary>
    public class ConfigGroupDto
    {
        /// <summary>
        /// 分组编码
        /// </summary>
        public string GroupCode { get; set; } = string.Empty;

        /// <summary>
        /// 分组名称
        /// </summary>
        public string GroupName { get; set; } = string.Empty;

        /// <summary>
        /// 配置项列表
        /// </summary>
        public List<ConfigItemDto> Items { get; set; } = new List<ConfigItemDto>();
    }

    /// <summary>
    /// 配置项详情
    /// </summary>
    public class ConfigItemDto
    {
        public long Id { get; set; }

        /// <summary>
        /// 配置分组
        /// </summary>
        public string ConfigGroup { get; set; } = string.Empty;

        /// <summary>
        /// 配置键
        /// </summary>
        public string ConfigKey { get; set; } = string.Empty;

        /// <summary>
        /// 配置名称
        /// </summary>
        public string ConfigName { get; set; } = string.Empty;

        /// <summary>
        /// 配置值
        /// </summary>
        public string ConfigValue { get; set; } = string.Empty;

        /// <summary>
        /// 默认值
        /// </summary>
        public string DefaultValue { get; set; }

        /// <summary>
        /// 值类型（BOOLEAN / NUMBER / STRING / ENUM / JSON）
        /// </summary>
        public string ValueType { get; set; } = string.Empty;

        /// <summary>
        /// 可选值（ENUM类型的候选值列表）
        /// </summary>
        public List<string>? OptionList { get; set; }

        /// <summary>
        /// 原始可选值JSON
        /// </summary>
        public string? Options { get; set; }

        /// <summary>
        /// 描述说明
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 排序号
        /// </summary>
        public int SortNo { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public byte Status { get; set; }

        /// <summary>
        /// 作用域级别（GLOBAL / WAREHOUSE / ZONE / DOC_TYPE）
        /// </summary>
        public string ScopeLevel { get; set; } = ConfigScopeLevels.GLOBAL;

        /// <summary>
        /// 作用域ID（仓库ID/库区ID/单据类型ID），GLOBAL 级别时为 null
        /// </summary>
        public long? ScopeId { get; set; }

        /// <summary>
        /// 优先级，数值越大优先级越高
        /// </summary>
        public int Priority { get; set; }
    }
}
