using KH.WMS.Config.Abstractions;
using KH.WMS.Core.DependencyInjection.ServiceLifetimes;
using KH.WMS.Core.Logging.WMSError;
using KH.WMS.Core.Validation;
using KH.WMS.Modules.InboundModule.DTOs;
using static KH.WMS.Core.Logging.WMSError.WMSErrorCodes;

namespace KH.WMS.Modules.InboundModule.Validation;

/// <summary>
/// 有效期必填校验器
/// 当 EXPIRY_MANAGEMENT=true 时，校验每组盘数据的生产日期或有效期不能为空
/// </summary>
[RegisteredService(WithoutInterceptor = true, ServiceType = typeof(IValidator))]
public class ExpiryDateRequiredValidator : IValidator
{
    public string Code => ValidatorCodes.EXPIRY_DATE_REQUIRED;

    public async Task<string?> ValidateAsync(object?[] args, Dictionary<string, object>? services = null)
    {
        var configService = GetConfigService(services);
        if (configService == null) return null;

        if (!await configService.ResolveConfigBoolAsync("CONTAINER", "EXPIRY_MANAGEMENT"))
            return null;

        var binds = args.OfType<List<ContainerBindDto>>().FirstOrDefault();
        if (binds == null) return null;

        foreach (var bind in binds)
        {
            if (!bind.ProductionDate.HasValue && !bind.ExpiryDate.HasValue)
                return WMSErrorMessages.GetMessage(EXPIRY_DATE_REQUIRED);
        }
        return null;
    }

    private static IConfigResolverContract? GetConfigService(Dictionary<string, object>? services)
    {
        if (services == null) return null;
        return services.TryGetValue("ConfigService", out var svc) ? svc as IConfigResolverContract : null;
    }
}
