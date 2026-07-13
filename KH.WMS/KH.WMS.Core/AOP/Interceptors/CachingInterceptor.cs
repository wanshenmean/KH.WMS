using System.Reflection;
using Castle.DynamicProxy;
using KH.WMS.Core.Api.Responses;
using KH.WMS.Core.Attributes;
using KH.WMS.Core.Caching;
using KH.WMS.Core.License.Results;
using KH.WMS.Core.Logging;
using Microsoft.Extensions.Logging;

namespace KH.WMS.Core.AOP.Interceptors;

/// <summary>
/// 缓存拦截器 - 自动缓存方法返回结果
/// </summary>
public class CachingInterceptor : IInterceptor
{
    private readonly ILoggerService _logger;
    private readonly ICacheService _cache;
    public CachingInterceptor(ILoggerService logger, ICacheService cache)
    {
        _logger = logger;
        _cache = cache;
    }

    public void Intercept(IInvocation invocation)
    {
        //_logger.LogInfoToFile("CachingInterceptor", "进入缓存拦截器: {Method}", invocation.Method.Name);

        var cacheAttr = GetCacheAttribute(invocation);
        if (cacheAttr == null || !cacheAttr.Enable)
        {
            invocation.Proceed();
            return;
        }

        var cacheKey = GenerateCacheKey(invocation, cacheAttr);

        // 检查缓存
        if (_cache.TryGet<object>(cacheKey, out var cachedValue))
        {
            _logger.LogDebug("缓存命中: {Key}", cacheKey);

            if (typeof(Task).IsAssignableFrom(invocation.Method.ReturnType))
            {
                if (invocation.Method.ReturnType.IsGenericType)
                {
                    // Task<T> 类型 - 使用反射创建正确的泛型 Task
                    var resultType = invocation.Method.ReturnType.GetGenericArguments()[0];
                    var fromResultMethod = typeof(Task)
                        .GetMethod(nameof(Task.FromResult))!
                        .MakeGenericMethod(resultType);
                    invocation.ReturnValue = fromResultMethod.Invoke(null, new[] { cachedValue });
                }
                else
                {
                    // Task 类型（无返回值）
                    invocation.ReturnValue = Task.CompletedTask;
                }
            }
            else
            {
                invocation.ReturnValue = cachedValue;
            }
            return;
        }

        _logger.LogDebug("缓存未命中: {Key}", cacheKey);

        // 执行方法
        invocation.Proceed();

        // 异步方法处理
        if (IsAsyncMethod(invocation.Method))
        {
            InterceptAsync(invocation, cacheKey, cacheAttr).GetAwaiter().GetResult();
        }
        else
        {
            // 同步方法，直接缓存返回值
            if (invocation.ReturnValue != null)
            {
                var result = invocation.ReturnValue;

                if (result is ApiResponse response)
                {
                    if (response.Data != null)
                    {
                        _cache.Set(cacheKey, invocation.ReturnValue, TimeSpan.FromSeconds(cacheAttr.Duration));
                        _logger.LogDebug("已缓存: {Key}, 过期时间: {Duration}s", cacheKey, cacheAttr.Duration);
                    }
                }
            }
        }
    }

    private async Task InterceptAsync(IInvocation invocation, string cacheKey, CacheAttribute cacheAttr)
    {
        var task = invocation.ReturnValue as Task;

        if (task == null)
            return;

        await task;

        // 获取 Task<TResult> 的返回值
        var resultProperty = invocation.ReturnValue?.GetType().GetProperty("Result");
        var result = resultProperty?.GetValue(invocation.ReturnValue);

        if (result != null)
        {
            if (result is ApiResponse response)
            {
                if (response.Data != null)
                {
                    _cache.Set(cacheKey, result, TimeSpan.FromSeconds(cacheAttr.Duration));
                    _logger.LogDebug("已缓存: {Key}, 过期时间: {Duration}s", cacheKey, cacheAttr.Duration);
                }
            }
        }
    }

    private CacheAttribute? GetCacheAttribute(IInvocation invocation)
    {

        var attr = invocation.MethodInvocationTarget.GetCustomAttributes(typeof(CacheAttribute), true)
                               .FirstOrDefault() as CacheAttribute;
        if (attr != null)
            return attr;

        // 检查方法上的特性
        var methodAttr = invocation.Method.GetCustomAttributes(typeof(CacheAttribute), false).FirstOrDefault() as CacheAttribute;
        if (methodAttr != null)
            return methodAttr;

        // 检查类上的特性
        var classAttr = invocation.TargetType.GetCustomAttributes(typeof(CacheAttribute), true).FirstOrDefault() as CacheAttribute;
        return classAttr;
    }

    private string GenerateCacheKey(IInvocation invocation, CacheAttribute cacheAttr)
    {
        var prefix = string.IsNullOrEmpty(cacheAttr.KeyPrefix)
            ? $"{invocation.TargetType.Name}.{invocation.Method.Name}"
            : cacheAttr.KeyPrefix;

        var argsKey = string.Join("_", invocation.Arguments.Select(a => a?.ToString() ?? "null"));
        return $"{prefix}:{argsKey}";
    }

    private bool IsAsyncMethod(System.Reflection.MethodInfo method)
    {
        return method.ReturnType == typeof(Task) ||
               (method.ReturnType.IsGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>));
    }
}
