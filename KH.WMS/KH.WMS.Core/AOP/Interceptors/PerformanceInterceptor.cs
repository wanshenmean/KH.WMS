using System.Diagnostics;
using Castle.DynamicProxy;
using KH.WMS.Core.Logging;
using Microsoft.Extensions.Logging;

namespace KH.WMS.Core.AOP.Interceptors;

/// <summary>
/// 性能拦截器 - 统计方法执行时间
/// </summary>
public class PerformanceInterceptor : IInterceptor
{
    private readonly ILoggerService _logger;
    private readonly long _thresholdMs;

    public PerformanceInterceptor(ILoggerService logger, long thresholdMs = 1000)
    {
        _logger = logger;
        _thresholdMs = thresholdMs;
    }

    public void Intercept(IInvocation invocation)
    {
        var methodName = $"{invocation.TargetType.Name}.{invocation.Method.Name}";
        var stopwatch = Stopwatch.StartNew();

        try
        {
            invocation.Proceed();

            // 等待异步方法完成
            if (IsAsyncMethod(invocation.Method))
            {
                var task = invocation.ReturnValue as Task;
                task?.GetAwaiter().GetResult();
            }
        }
        finally
        {
            stopwatch.Stop();
            var elapsed = stopwatch.ElapsedMilliseconds;

            _logger.LogDebug("{Method} 执行时间: {Elapsed}ms", methodName, elapsed);

            if (elapsed > _thresholdMs)
            {
                _logger.LogWarning("性能警告: {Method} 执行时间: {Elapsed}ms (超过阈值: {Threshold}ms)",
                    methodName, elapsed, _thresholdMs);
            }
        }
    }

    private bool IsAsyncMethod(System.Reflection.MethodInfo method)
    {
        return method.ReturnType == typeof(Task) ||
               (method.ReturnType.IsGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>));
    }
}
