using System.Reflection;
using Autofac;
using Castle.DynamicProxy;
using KH.WMS.Core.Models;
using KH.WMS.Core.Validation;

namespace KH.WMS.Core.AOP.Interceptors;

/// <summary>
/// 配置校验拦截器
/// 拦截标注了 [ConfigValidation] 的业务方法，在方法执行前运行声明的校验器链
/// 任一校验失败则短路返回 ServiceResult.Fail，不执行原方法
/// </summary>
public class ConfigValidationInterceptor : IInterceptor
{
    private readonly ILifetimeScope _lifetimeScope;

    public ConfigValidationInterceptor(ILifetimeScope lifetimeScope)
    {
        _lifetimeScope = lifetimeScope;
    }

    public void Intercept(IInvocation invocation)
    {
        var attrs = invocation.MethodInvocationTarget
            .GetCustomAttributes<ConfigValidationAttribute>(true);

        if (!attrs.Any())
        {
            invocation.Proceed();
            return;
        }

        var returnType = invocation.Method.ReturnType;
        var isGenericServiceResult = returnType.IsGenericType &&
            returnType.GetGenericTypeDefinition() == typeof(Task<>);
        var isServiceResult = returnType == typeof(Task);

        // 仅对 Task<ServiceResult> 和 Task<ServiceResult<T>> 生效
        if (!isServiceResult && !isGenericServiceResult)
        {
            invocation.Proceed();
            return;
        }

        var resultType = isGenericServiceResult
            ? returnType.GetGenericArguments()[0]
            : typeof(ServiceResult);

        // 直接从当前 LifetimeScope 解析依赖（拦截器已在请求作用域中运行，无需创建子 scope）
        var validators = _lifetimeScope.Resolve<IEnumerable<IValidator>>();
        var validatorDict = validators.ToDictionary(v => v.Code);

        // 构建服务字典供 Validator 使用
        var services = new Dictionary<string, object>();

        // 通过类型名称动态解析 ConfigService，避免 Core 项目对 Contracts 的直接引用
        var configServiceType = Type.GetType("KH.WMS.Config.Abstractions.IConfigResolverContract, KH.WMS.Config");
        if (configServiceType != null)
        {
            var configService = _lifetimeScope.ResolveOptional(configServiceType);
            if (configService != null)
                services["ConfigService"] = configService;
        }

        // 同步执行所有校验（校验内部需要 await，此处用 GetAwaiter().GetResult）
        foreach (var attr in attrs)
        {
            if (!validatorDict.TryGetValue(attr.ValidatorCode, out var validator))
                continue;

            var errorMessage = validator.ValidateAsync(invocation.Arguments, services).GetAwaiter().GetResult();
            if (errorMessage != null)
            {
                // 校验失败，短路返回 ServiceResult.Fail
                var failMethod = typeof(ServiceResult)
                    .GetMethod(nameof(ServiceResult.Fail), [typeof(string)])!;
                var failResult = failMethod.Invoke(null, [errorMessage]);

                invocation.ReturnValue = CreateTaskResult(resultType, failResult);
                return;
            }
        }

        // 所有校验通过，执行原方法
        invocation.Proceed();
    }

    /// <summary>
    /// 构造 Task<ServiceResult> 或 Task<ServiceResult<T>> 的完成态
    /// </summary>
    private static object CreateTaskResult(Type resultType, object? result)
    {
        var fromResultMethod = typeof(Task)
            .GetMethod(nameof(Task.FromResult))!
            .MakeGenericMethod(resultType);
        return fromResultMethod.Invoke(null, [result])!;
    }
}
