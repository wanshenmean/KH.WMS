using KH.WMS.Core.Services;
using KH.WMS.Config.Abstractions;
using KH.WMS.Config.DTOs;

namespace KH.WMS.Config.Interfaces
{
    public interface ICfgGlobalConfigService : ICrudService<CfgGlobalConfig>
    {
        /// <summary>
        /// 按分组查询配置列表
        /// </summary>
        Task<List<ConfigGroupDto>> GetByGroupAsync();

        /// <summary>
        /// 获取指定分组的配置
        /// </summary>
        Task<List<ConfigItemDto>> GetByGroupCodeAsync(string groupCode);

        /// <summary>
        /// 获取指定配置值
        /// </summary>
        Task<string> GetConfigValueAsync(string group, string key);

        /// <summary>
        /// 获取指定配置值（带默认值）
        /// </summary>
        Task<string> GetConfigValueAsync(string group, string key, string defaultValue);

        /// <summary>
        /// 批量修改配置值
        /// </summary>
        Task BatchUpdateAsync(BatchUpdateConfigRequest request);

        /// <summary>
        /// 重置分组内所有配置为默认值
        /// </summary>
        Task ResetToDefaultAsync(string groupCode);

        /// <summary>
        /// 按作用域查询配置项列表（管理界面用）
        /// </summary>
        Task<List<ConfigItemDto>> GetByGroupAndScopeAsync(string groupCode, string scopeLevel, long? scopeId = null);

        /// <summary>
        /// 清除指定配置项的解析缓存
        /// </summary>
        void ClearConfigCache(List<CfgGlobalConfig>? changedItems = null);
    }
}
