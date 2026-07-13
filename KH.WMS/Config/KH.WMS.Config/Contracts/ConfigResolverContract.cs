using KH.WMS.Config.Abstractions;
using KH.WMS.Core.Caching;
using KH.WMS.Core.Constants;
using KH.WMS.Core.Database.Repositories;
using KH.WMS.Core.DependencyInjection.ServiceLifetimes;
using Microsoft.Extensions.DependencyInjection;
using ICacheService = KH.WMS.Core.Caching.ICacheService;

namespace KH.WMS.Config.Contracts;

/// <summary>
/// 全局配置解析器契约实现
/// 提供分层配置解析能力，支持 GLOBAL / WAREHOUSE / ZONE / DOC_TYPE 四级作用域
/// 通过 IConfigScopeResolver 解析作用域标识，不直接依赖具体业务实体
/// </summary>
[RegisteredService(Lifetime = ServiceLifetime.Scoped, ServiceType = typeof(IConfigResolverContract))]
public class ConfigResolverContract(
    IConfigScopeResolver scopeResolver,
    IRepository<CfgGlobalConfig, long> globalConfigRepository,
    ICacheService cache) : IConfigResolverContract
{
    private const string CACHE_PREFIX = CacheConstants.Config.PREFIX;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(1);

    /// <inheritdoc />
    public async Task<string> ResolveConfigValueAsync(string group, string key, ConfigScopeContext? scope = null)
    {
        scope ??= ConfigScopeContext.Empty;

        var cacheKey = $"{CACHE_PREFIX}{group}:{key}:w{scope.WarehouseId ?? (object?)""}:z{scope.ZoneId ?? (object?)""}:d{scope.DocTypeCode ?? ""}";
        if (cache.TryGet<string>(cacheKey, out var cached))
            return cached!;

        string? result = null;

        // 1. DOC_TYPE 级别（通过 IConfigScopeResolver 解析 docTypeCode → ID）
        if (!string.IsNullOrEmpty(scope.DocTypeCode))
        {
            var docTypeId = await scopeResolver.ResolveScopeIdAsync(ConfigScopeLevels.DOC_TYPE, scope.DocTypeCode);
            if (docTypeId.HasValue)
                result = await QueryConfigValueAsync(group, key, ConfigScopeLevels.DOC_TYPE, docTypeId.Value);
        }

        // 2. ZONE 级别
        if (string.IsNullOrEmpty(result) && scope.ZoneId.HasValue)
            result = await QueryConfigValueAsync(group, key, ConfigScopeLevels.ZONE, scope.ZoneId.Value);

        // 3. WAREHOUSE 级别
        if (string.IsNullOrEmpty(result) && scope.WarehouseId.HasValue)
            result = await QueryConfigValueAsync(group, key, ConfigScopeLevels.WAREHOUSE, scope.WarehouseId.Value);

        // 4. GLOBAL 级别（无作用域参数时也走此路径）
        result ??= await QueryConfigValueAsync(group, key, ConfigScopeLevels.GLOBAL, null);

        cache.Set(cacheKey, result ?? string.Empty, CacheDuration);
        return result ?? string.Empty;
    }

    /// <inheritdoc />
    public async Task<bool> ResolveConfigBoolAsync(string group, string key, ConfigScopeContext? scope = null)
    {
        var value = await ResolveConfigValueAsync(group, key, scope);
        return string.Equals(value, "true", StringComparison.OrdinalIgnoreCase)
            || value == "1";
    }

    /// <inheritdoc />
    public async Task WarmUpAsync()
    {
        var globalConfigs = await globalConfigRepository.GetListAsync(
            c => c.ScopeLevel == ConfigScopeLevels.GLOBAL && c.Status == BoolFlag.YES);

        foreach (var config in globalConfigs)
        {
            var cacheKey = $"{CACHE_PREFIX}{config.ConfigGroup}:{config.ConfigKey}:w::z::d:";
            cache.Set(cacheKey, config.ConfigValue, CacheDuration);
        }
    }

    private async Task<string?> QueryConfigValueAsync(string group, string key, string scopeLevel, long? scopeId)
    {
        var configs = await globalConfigRepository.GetListAsync(c =>
            c.ConfigGroup == group
            && c.ConfigKey == key
            && c.ScopeLevel == scopeLevel
            && c.Status == BoolFlag.YES
            && (scopeId.HasValue ? c.ScopeId == scopeId : c.ScopeId == null));

        return configs.OrderByDescending(c => c.Priority).FirstOrDefault()?.ConfigValue;
    }
}
