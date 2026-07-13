using KH.WMS.Config.Abstractions;
using KH.WMS.Core.Caching;
using KH.WMS.Core.Constants;
using KH.WMS.Core.Database.Repositories;
using KH.WMS.Core.DependencyInjection.ServiceLifetimes;
using Microsoft.Extensions.DependencyInjection;
using ICacheService = KH.WMS.Core.Caching.ICacheService;

namespace KH.WMS.Config.Contracts;

/// <summary>
/// 配置作用域解析器默认实现
/// 负责将作用域标识（如单据类型编码）解析为作用域ID
/// 查询 cfg_document_type 等配置表
/// </summary>
[RegisteredService(Lifetime = ServiceLifetime.Scoped, ServiceType = typeof(IConfigScopeResolver))]
public class DefaultConfigScopeResolver(
    IRepository<CfgDocumentType, long> docTypeRepository,
    ICacheService cache) : IConfigScopeResolver
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(30);

    /// <inheritdoc />
    public async Task<long?> ResolveScopeIdAsync(string scopeLevel, string scopeIdentifier)
    {
        if (string.IsNullOrEmpty(scopeIdentifier))
            return null;

        return scopeLevel switch
        {
            ConfigScopeLevels.DOC_TYPE => await ResolveDocTypeIdAsync(scopeIdentifier),
            // WAREHOUSE / ZONE 的 scopeId 由调用方直接传入（已是 long），无需解析
            _ => null,
        };
    }

    private async Task<long?> ResolveDocTypeIdAsync(string docTypeCode)
    {
        var cacheKey = $"{CacheConstants.Data.PREFIX}ConfigScope:DocType:{docTypeCode}";
        return await cache.GetOrCreateAsync(cacheKey, async () =>
        {
            var docType = await docTypeRepository.GetFirstOrDefaultAsync(
                t => t.TypeCode == docTypeCode && t.IsActive == BoolFlag.YES);
            return docType?.Id;
        }, CacheDuration);
    }
}
