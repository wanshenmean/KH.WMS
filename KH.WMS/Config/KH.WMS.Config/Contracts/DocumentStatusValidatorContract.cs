using System.Text.Json;
using KH.WMS.Config.Abstractions;
using KH.WMS.Core.Caching;
using KH.WMS.Core.Constants;
using KH.WMS.Core.Database.Repositories;
using KH.WMS.Core.DependencyInjection.ServiceLifetimes;
using KH.WMS.Config.Abstractions;
using KH.WMS.Core.Constants;
using Microsoft.Extensions.DependencyInjection;
using ICacheService = KH.WMS.Core.Caching.ICacheService;

namespace KH.WMS.Config.Contracts;

/// <summary>
/// 单据状态验证器契约实现
/// 查询 cfg_document_type / cfg_document_status 表
/// 通过 CfgDocumentStatus.NextStatuses JSON 数组校验流转合法性
/// </summary>
[RegisteredService(Lifetime = ServiceLifetime.Scoped, ServiceType = typeof(IDocumentStatusValidatorContract))]
public class DocumentStatusValidatorContract(
    IRepository<CfgDocumentType, long> docTypeRepository,
    IRepository<CfgDocumentStatus, long> docStatusRepository,
    ICacheService cache) : IDocumentStatusValidatorContract
{
    private readonly IRepository<CfgDocumentType, long> _docTypeRepository = docTypeRepository;
    private readonly IRepository<CfgDocumentStatus, long> _docStatusRepository = docStatusRepository;
    private readonly ICacheService _cache = cache;

    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(30);

    /// <inheritdoc />
    public async Task ValidateTransitionAsync(string docTypeCode, string fromStatus, string toStatus)
    {
        if (fromStatus == toStatus)
            return;

        var statusConfig = await GetStatusConfigAsync(docTypeCode, fromStatus);
        if (statusConfig == null)
            throw new InvalidOperationException($"未找到单据 [{docTypeCode}] 的状态配置: {fromStatus}");

        var nextList = ParseNextStatuses(statusConfig.NextStatuses);
        if (!nextList.Contains(toStatus))
            throw new InvalidOperationException(
                $"单据 [{docTypeCode}] 不允许从状态 [{fromStatus}] 转换到 [{toStatus}]，允许的目标状态: [{string.Join(", ", nextList)}]");
    }

    /// <inheritdoc />
    public async Task<bool> CanTransitionAsync(string docTypeCode, string fromStatus, string toStatus)
    {
        try
        {
            await ValidateTransitionAsync(docTypeCode, fromStatus, toStatus);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<CfgDocumentStatus?> GetStatusConfigAsync(string docTypeCode, string status)
    {
        var docType = await GetDocTypeByCodeAsync(docTypeCode);
        if (docType == null) return null;

        var cacheKey = $"{CacheConstants.Data.PREFIX}DocStatus:{docTypeCode}:{status}";
        return await _cache.GetOrCreateAsync(cacheKey, async () =>
        {
            return await _docStatusRepository.GetFirstOrDefaultAsync(
                s => s.DocTypeId == docType.Id && s.StatusCode == status && s.IsActive == BoolFlag.YES);
        }, CacheDuration);
    }

    /// <inheritdoc />
    public async Task<List<string>> GetAllowedTransitionsAsync(string docTypeCode, string fromStatus)
    {
        var statusConfig = await GetStatusConfigAsync(docTypeCode, fromStatus);
        if (statusConfig == null) return new List<string>();
        return ParseNextStatuses(statusConfig.NextStatuses);
    }

    /// <inheritdoc />
    public async Task ValidateAllowEditAsync(string docTypeCode, string currentStatus)
    {
        var statusConfig = await GetStatusConfigAsync(docTypeCode, currentStatus);
        if (statusConfig == null)
            throw new InvalidOperationException($"未找到单据 {docTypeCode} 的状态配置: {currentStatus}");
        if (statusConfig.AllowEdit != BoolFlag.YES)
            throw new InvalidOperationException($"单据当前状态 [{statusConfig.StatusName}] 不允许编辑");
    }

    /// <inheritdoc />
    public async Task ValidateAllowDeleteAsync(string docTypeCode, string currentStatus)
    {
        var statusConfig = await GetStatusConfigAsync(docTypeCode, currentStatus);
        if (statusConfig == null)
            throw new InvalidOperationException($"未找到单据 {docTypeCode} 的状态配置: {currentStatus}");
        if (statusConfig.AllowDelete != BoolFlag.YES)
            throw new InvalidOperationException($"单据当前状态 [{statusConfig.StatusName}] 不允许删除");
    }

    /// <inheritdoc />
    public async Task<string?> GetInitialStatusAsync(string docTypeCode)
    {
        var docType = await GetDocTypeByCodeAsync(docTypeCode);
        if (docType == null) return null;

        var initial = await _docStatusRepository.GetFirstOrDefaultAsync(
            s => s.DocTypeId == docType.Id && s.IsInitial == BoolFlag.YES && s.IsActive == BoolFlag.YES);
        return initial?.StatusCode;
    }

    private async Task<CfgDocumentType?> GetDocTypeByCodeAsync(string typeCode)
    {
        var cacheKey = $"{CacheConstants.Data.PREFIX}DocType:Code:{typeCode}";
        return await _cache.GetOrCreateAsync(cacheKey, async () =>
        {
            return await _docTypeRepository.GetFirstOrDefaultAsync(
                t => t.TypeCode == typeCode && t.IsActive == BoolFlag.YES);
        }, CacheDuration);
    }

    private static List<string> ParseNextStatuses(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return new List<string>();
        try
        {
            return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }
}
