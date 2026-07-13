using KH.WMS.Config.Abstractions;
using KH.WMS.Core.DependencyInjection.ServiceLifetimes;
using KH.WMS.Core.Logging.WMSError;
using KH.WMS.Core.Validation;
using KH.WMS.Entities.Inbound;
using KH.WMS.Modules.InboundModule.DTOs;
using SqlSugar;
using static KH.WMS.Core.Logging.WMSError.WMSErrorCodes;

namespace KH.WMS.Modules.InboundModule.Validation;

/// <summary>
/// 混批校验器
/// 当 ALLOW_MIXED_BATCH=false 时，同容器不能包含不同批次
/// </summary>
[RegisteredService(WithoutInterceptor = true, ServiceType = typeof(IValidator))]
public class MixedBatchValidator : IValidator
{
    private readonly ISqlSugarClient _db;

    public MixedBatchValidator(ISqlSugarClient db)
    {
        _db = db;
    }

    public string Code => ValidatorCodes.MIXED_BATCH;

    public async Task<string?> ValidateAsync(object?[] args, Dictionary<string, object>? services = null)
    {
        var configService = GetConfigService(services);
        if (configService == null) return null;

        if (await configService.ResolveConfigBoolAsync("CONTAINER", "ALLOW_MIXED_BATCH"))
            return null;

        var binds = args.OfType<List<ContainerBindDto>>().FirstOrDefault();
        if (binds == null) return null;

        // 查询订单行获取默认批次号
        var lineIds = binds.Select(b => b.InboundOrderLineId).Distinct().ToList();
        var lines = await _db.Queryable<InboundOrderLine>()
            .Where(l => lineIds.Contains(l.Id))
            .Select(l => new { l.Id, l.BatchNo })
            .ToListAsync();
        var lineDict = lines.ToDictionary(l => l.Id, l => l.BatchNo);

        foreach (var group in binds.GroupBy(b => b.ContainerCode))
        {
            var batchNos = group
                .Select(b => b.BatchNo ?? lineDict[b.InboundOrderLineId])
                .Where(b => !string.IsNullOrEmpty(b))
                .Distinct()
                .ToList();

            if (batchNos.Count > 1)
                return WMSErrorMessages.GetMessage(CONTAINER_MIXED_BATCH, group.Key);
        }
        return null;
    }

    private static IConfigResolverContract? GetConfigService(Dictionary<string, object>? services)
    {
        if (services == null) return null;
        return services.TryGetValue("ConfigService", out var svc) ? svc as IConfigResolverContract : null;
    }
}
