using System;
using System.Diagnostics;
using System.Reflection;
using System.Security.Claims;
using Castle.DynamicProxy;
using KH.WMS.Core.AOP.CallTrace;
using KH.WMS.Core.Attributes;
using KH.WMS.Core.Logging;
using KH.WMS.Core.UserProvide;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StackExchange.Profiling;

namespace KH.WMS.Core.AOP.Interceptors;

/// <summary>
/// 日志拦截器 - 记录方法调用日志
/// </summary>
public class LoggingInterceptor(ILoggerService logger, IHttpContextAccessor httpContextAccessor, IUserContext userContext) : InterceptorBase
{
    private readonly ILoggerService _logger = logger;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly IUserContext _userContext = userContext;
    private LogInterceptorAttribute? _attribute;

    /// <summary>
    /// 获取当前用户ID，非HTTP上下文返回"匿名"
    /// </summary>
    private string GetUserId()
    {
        return _userContext.UserId?.ToString() ?? "匿名";
    }

    /// <summary>
    /// 获取当前用户名，非HTTP上下文返回"匿名用户"
    /// </summary>
    private string GetUserName()
    {
        return _userContext.UserName ?? "匿名用户";
    }

    protected override void BeforeExecute(IInvocation invocation, string methodName)
    {
        string traceId = CallTraceContext.Current.TraceId;
        string indent = GetIndent(CallTraceContext.Current.Depth);
        string userId = GetUserId();
        string userName = GetUserName();

        if (CallTraceContext.Current.Depth == 1)
        {
            // 从方法上获取特性 - 先尝试 MethodInvocationTarget，再尝试 Method
            _attribute = invocation.MethodInvocationTarget.GetCustomAttributes(typeof(LogInterceptorAttribute), true)
                               .FirstOrDefault() as LogInterceptorAttribute;

            if (_attribute == null)
            {
                _attribute = invocation.Method.GetCustomAttributes(typeof(LogInterceptorAttribute), true)
                                   .FirstOrDefault() as LogInterceptorAttribute;
            }

            // 如果方法上没有特性，尝试从类上获取
            if (_attribute == null)
            {
                _attribute = invocation.TargetType.GetCustomAttributes(typeof(LogInterceptorAttribute), true)
                                   .FirstOrDefault() as LogInterceptorAttribute;
            }

            if (_attribute == null)
                return;

            CallTraceContext.Current.IsRecord = true;
        }

        if (CallTraceContext.Current.IsRecord)
        {
            // 如果启用参数记录，记录详细参数（Verbose 级别 - 完整参数详情）
            var logParameters = _attribute?.LogParameters ?? true;
            if (logParameters)
            {
                var parameters = FormatParameters(invocation);
                _logger.LogDebug("[方法进入] {Indent}[{TraceId}] -> {MethodName} | User: {UserId} ({UserName})  |  参数: {Parameters}",
               indent, traceId, methodName, userId, userName, parameters);

                MiniProfiler.Current.Step($"[方法进入] {indent}[{traceId}] -> {methodName} | User: {userId} ({userName})  |  参数: {parameters}");

                ErrorLogScope.Append($"[{DateTime.Now:HH:mm:ss.fff}] → {methodName} | 参数: {parameters}");
            }
            else
            {
                _logger.LogDebug("[方法进入] {Indent}[{TraceId}] -> {MethodName} | User: {UserId} ({UserName})",
               indent, traceId, methodName, userId, userName);

                MiniProfiler.Current.Step($"[方法进入] {indent}[{traceId}] -> {methodName} | User: {userId} ({userName})");

                ErrorLogScope.Append($"[{DateTime.Now:HH:mm:ss.fff}] → {methodName}");
            }
        }

    }

    protected override void AfterExecute(IInvocation invocation, string methodName)
    {
        string traceId = CallTraceContext.Current.TraceId;
        string indent = GetIndent(CallTraceContext.Current.Depth);

        if (CallTraceContext.Current.Depth == 1)
        {
            if (_attribute == null)
                return;
        }
        if (CallTraceContext.Current.IsRecord)
        {
            // 如果启用返回值记录，记录返回值（Verbose 级别）
            var logReturnValue = _attribute?.LogReturnValue ?? true;
            if (logReturnValue && invocation.ReturnValue != null)
            {
                object returnVal = invocation.ReturnValue;
                if (IsAsyncMethod(invocation.Method))
                {
                    var task = invocation.ReturnValue as Task;

                    if (task == null)
                        return;

                    task?.GetAwaiter().GetResult();

                    // 获取 Task<TResult> 的返回值
                    var resultProperty = invocation.ReturnValue?.GetType().GetProperty("Result");
                    var result = resultProperty?.GetValue(invocation.ReturnValue);
                    returnVal = result!;
                }
                else
                {
                    returnVal = invocation.ReturnValue;
                }
                var returnValue = FormatReturnValue(returnVal);
                _logger.LogDebug("[方法退出] {Indent}[{TraceId}] <- {MethodName}  |  返回值: {ReturnValue}", indent, traceId, methodName, returnValue);

                MiniProfiler.Current.Step($"[方法退出] {indent}[{traceId}] <- {methodName}  |  返回值: {returnValue}");

                ErrorLogScope.Append($"[{DateTime.Now:HH:mm:ss.fff}] ← {methodName} | 返回: {returnValue}");
            }
            else
            {
                _logger.LogDebug("[方法退出] {Indent}[{TraceId}] <- {MethodName}", indent, traceId, methodName);

                MiniProfiler.Current.Step($"[方法退出] {indent}[{traceId}] <- {methodName}");
            }
        }
    }

    protected override void OnException(IInvocation invocation, string methodName, Exception ex)
    {
        _logger.LogError(ex, "[方法异常] {MethodName} | 异常信息: {Message}",
            methodName, ex.Message);
    }

    protected override Task BeforeExecuteAsync(IInvocation invocation, string methodName)
    {
        BeforeExecute(invocation, methodName);
        return base.BeforeExecuteAsync(invocation, methodName);
    }

    protected override Task AfterExecuteAsync(IInvocation invocation, string methodName)
    {
        AfterExecute(invocation, methodName);
        return base.AfterExecuteAsync(invocation, methodName);
    }


    private string FormatParameters(IInvocation invocation)
    {
        if (invocation.Arguments == null || invocation.Arguments.Length == 0)
            return "无参数";

        var parameters = new List<string>();
        for (int i = 0; i < invocation.Arguments.Length; i++)
        {
            var arg = invocation.Arguments[i];
            var formattedValue = FormatValue(arg, i);
            parameters.Add($"P{i}: {formattedValue}");
        }

        return string.Join(", ", parameters);
    }

    private string FormatReturnValue(object? returnValue)
    {
        return FormatValue(returnValue, null);
    }

    /// <summary>
    /// 格式化值（处理基本类型、对象、集合等）
    /// </summary>
    private string FormatValue(object? value, int? paramIndex)
    {
        if (value == null)
            return "null";

        var type = value.GetType();

        // 处理字符串
        if (type == typeof(string))
        {
            var str = (string)value;
            return $"\"{str}\"";
        }

        // 处理基本值类型（int, bool, double, DateTime 等）
        if (type.IsPrimitive || type.IsEnum || type == typeof(DateTime) || type == typeof(DateTimeOffset) || type == typeof(decimal) || type == typeof(Guid))
        {
            return value.ToString() ?? "";
        }

        try
        {
            return JsonConvert.SerializeObject(value);
        }
        catch
        {
            return "Json序列化失败：" + value.ToString() ?? "";
        }
    }
}
