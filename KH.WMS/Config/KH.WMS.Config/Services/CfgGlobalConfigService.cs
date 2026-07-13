using KH.WMS.Core.Constants;
using KH.WMS.Core.Database.Repositories;
using KH.WMS.Core.Database.UnitOfWorks;
using KH.WMS.Core.DependencyInjection.ServiceLifetimes;
using KH.WMS.Core.Services;
using KH.WMS.Config.Abstractions;
using KH.WMS.Core.Constants;
using KH.WMS.Config.DTOs;
using KH.WMS.Config.Interfaces;
using Newtonsoft.Json;
using SqlSugar;
using ICacheService = KH.WMS.Core.Caching.ICacheService;

namespace KH.WMS.Config.Services
{
    [RegisteredService(ServiceType = typeof(ICfgGlobalConfigService))]
    public class CfgGlobalConfigService(
        IRepository<CfgGlobalConfig, long> repository,
        ISqlSugarClient db,
        IUnitOfWork unitOfWork,
        IDetailSaveService detailSaveService,
        ICacheService cacheService) : CrudService<CfgGlobalConfig>(repository, unitOfWork, detailSaveService), ICfgGlobalConfigService
    {
        private readonly ISqlSugarClient _db = db;
        private readonly ICacheService _cacheService = cacheService;
        private const string CONFIG_CACHE_PREFIX = CacheConstants.Config.PREFIX;
        private static readonly Dictionary<string, string> GroupNames = new()
        {
            { "INBOUND", "入库配置" },
            { "OUTBOUND", "出库配置" },
            { "INVENTORY", "库存配置" },
            { "CONTAINER", "容器/托盘配置" }
        };

        /// <summary>
        /// 按分组查询配置列表
        /// </summary>
        public async Task<List<ConfigGroupDto>> GetByGroupAsync()
        {
            var allConfigs = await _repository.GetAllAsync();
            var groups = allConfigs
                .GroupBy(c => c.ConfigGroup)
                .OrderBy(g => g.Key)
                .Select(g => new ConfigGroupDto
                {
                    GroupCode = g.Key,
                    GroupName = GroupNames.GetValueOrDefault(g.Key, g.Key),
                    Items = g.OrderBy(c => c.SortNo).Select(ToDto).ToList()
                }).ToList();
            return groups;
        }

        /// <summary>
        /// 获取指定分组的配置
        /// </summary>
        public async Task<List<ConfigItemDto>> GetByGroupCodeAsync(string groupCode)
        {
            var configs = await _repository.GetListAsync(c => c.ConfigGroup == groupCode);
            return configs.OrderBy(c => c.SortNo).Select(ToDto).ToList();
        }

        /// <summary>
        /// 获取指定配置值（仅查询 GLOBAL 级别）
        /// </summary>
        public async Task<string> GetConfigValueAsync(string group, string key)
        {
            var config = (await _repository.GetListAsync(c =>
                    c.ConfigGroup == group
                    && c.ConfigKey == key
                    && c.ScopeLevel == ConfigScopeLevels.GLOBAL
                    && c.Status == BoolFlag.YES))
                .OrderByDescending(c => c.Priority)
                .FirstOrDefault();
            return config?.ConfigValue ?? string.Empty;
        }

        /// <summary>
        /// 获取指定配置值（带默认值，仅查询 GLOBAL 级别）
        /// </summary>
        public async Task<string> GetConfigValueAsync(string group, string key, string defaultValue)
        {
            var value = await GetConfigValueAsync(group, key);
            return string.IsNullOrEmpty(value) ? defaultValue : value;
        }

        /// <summary>
        /// 批量修改配置值
        /// </summary>
        public async Task BatchUpdateAsync(BatchUpdateConfigRequest request)
        {
            if (request.Items == null || request.Items.Count == 0) return;

            var ids = request.Items.Select(i => i.Id).ToList();
            var changedConfigs = await _repository.GetListAsync(c => ids.Contains(c.Id));

            // 多条配置更新需在同一事务，避免部分失败导致配置不一致
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                foreach (var item in request.Items)
                {
                    await _db.Updateable<CfgGlobalConfig>()
                        .SetColumns(c => c.ConfigValue == item.ConfigValue)
                        .Where(c => c.Id == item.Id)
                        .ExecuteCommandAsync();
                }
                await _unitOfWork.CommitAsync();
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }

            ClearConfigCache(changedConfigs);
        }

        /// <summary>
        /// 重置分组内所有配置为默认值
        /// </summary>
        public async Task ResetToDefaultAsync(string groupCode)
        {
            var changedConfigs = await _repository.GetListAsync(c => c.ConfigGroup == groupCode && c.DefaultValue != null);

            await _db.Updateable<CfgGlobalConfig>()
                .SetColumns(c => c.ConfigValue == c.DefaultValue)
                .Where(c => c.ConfigGroup == groupCode && c.DefaultValue != null)
                .ExecuteCommandAsync();

            ClearConfigCache(changedConfigs);
        }

        /// <summary>
        /// 按作用域查询配置项列表（管理界面用）
        /// </summary>
        public async Task<List<ConfigItemDto>> GetByGroupAndScopeAsync(string groupCode, string scopeLevel, long? scopeId = null)
        {
            var configs = scopeId.HasValue
                ? await _repository.GetListAsync(c => c.ConfigGroup == groupCode && c.ScopeLevel == scopeLevel && c.ScopeId == scopeId.Value)
                : await _repository.GetListAsync(c => c.ConfigGroup == groupCode && c.ScopeLevel == scopeLevel && c.ScopeId == null);

            return configs.OrderBy(c => c.SortNo).Select(ToDto).ToList();
        }

        /// <summary>
        /// 清除指定配置项的解析缓存
        /// </summary>
        public void ClearConfigCache(List<CfgGlobalConfig>? changedItems = null)
        {
            if (changedItems == null || changedItems.Count == 0) return;

            // 配置解析缓存按查询参数(warehouseId/zoneId/docTypeCode)组合生成多种 key，
            // 改一个配置会使所有作用域组合的缓存失效，故按前缀整体清除该 (group:key) 的所有变体。
            foreach (var item in changedItems)
            {
                var prefix = $"{CONFIG_CACHE_PREFIX}{item.ConfigGroup}:{item.ConfigKey}:";
                _cacheService.RemoveByPrefix(prefix);
            }
        }

        private static ConfigItemDto ToDto(CfgGlobalConfig c)
        {
            List<string>? optionList = null;
            if (c.ValueType == "ENUM" && !string.IsNullOrEmpty(c.Options))
            {
                try
                {
                    optionList = JsonConvert.DeserializeObject<List<string>>(c.Options);
                }
                catch { }
            }

            return new ConfigItemDto
            {
                Id = c.Id,
                ConfigGroup = c.ConfigGroup,
                ConfigKey = c.ConfigKey,
                ConfigName = c.ConfigName,
                ConfigValue = c.ConfigValue,
                DefaultValue = c.DefaultValue,
                ValueType = c.ValueType,
                Options = c.Options,
                OptionList = optionList,
                Description = c.Description,
                SortNo = c.SortNo,
                Status = c.Status,
                ScopeLevel = c.ScopeLevel,
                ScopeId = c.ScopeId,
                Priority = c.Priority,
            };
        }
    }
}
