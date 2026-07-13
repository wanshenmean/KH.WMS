using KH.WMS.Core.DependencyInjection.ServiceLifetimes;
using KH.WMS.Core.Logging.WMSError;
using KH.WMS.Core.Validation;
using KH.WMS.Modules.InboundModule.DTOs;
using static KH.WMS.Core.Logging.WMSError.WMSErrorCodes;

namespace KH.WMS.Modules.InboundModule.Validation;

/// <summary>
/// 组盘数据非空校验器
/// 校验 binds 参数不能为空
/// </summary>
[RegisteredService(WithoutInterceptor = true, ServiceType = typeof(IValidator))]
public class BindDataNotEmptyValidator : IValidator
{
    public string Code => ValidatorCodes.BIND_DATA_NOT_EMPTY;

    public Task<string?> ValidateAsync(object?[] args, Dictionary<string, object>? services = null)
    {
        var binds = args.OfType<List<ContainerBindDto>>().FirstOrDefault();
        if (binds == null || binds.Count == 0)
            return Task.FromResult<string?>(WMSErrorMessages.GetMessage(CONTAINER_BIND_DATA_EMPTY));
        return Task.FromResult<string?>(null);
    }
}
