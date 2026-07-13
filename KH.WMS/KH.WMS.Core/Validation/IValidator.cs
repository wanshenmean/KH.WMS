namespace KH.WMS.Core.Validation;

/// <summary>
/// 统一校验器接口
/// 所有校验器（配置驱动、参数校验等）实现此接口
/// 通过 [ConfigValidation] 属性标注到业务方法上，由 ConfigValidationInterceptor 自动调度
/// </summary>
public interface IValidator
{
    /// <summary>
    /// 校验器唯一标识，对应 [ConfigValidation("code")] 属性中的 code
    /// </summary>
    string Code { get; }

    /// <summary>
    /// 执行校验
    /// </summary>
    /// <param name="args">被拦截方法的参数数组</param>
    /// <param name="services">可选的依赖服务字典，由拦截器注入（如 "ConfigService" → ICfgGlobalConfigService 实例）</param>
    /// <returns>null 表示通过，返回错误消息字符串表示失败</returns>
    Task<string?> ValidateAsync(object?[] args, Dictionary<string, object>? services = null);
}
