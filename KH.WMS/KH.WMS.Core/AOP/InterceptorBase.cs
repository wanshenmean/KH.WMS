using System.Reflection;
using Castle.DynamicProxy;
using KH.WMS.Core.AOP.CallTrace;

namespace KH.WMS.Core.AOP;

/// <summary>
/// 拦截器基类
/// </summary>
public abstract class InterceptorBase : IInterceptor
{
    public virtual bool EnableAsync => true;

    public void Intercept(IInvocation invocation)
    {
        string methodName = $"{invocation.TargetType.Name}.{invocation.Method.Name}";
        MethodInfo method = invocation.Method;

        if (IsAsyncMethod(method))
        {
            InternalInterceptAsynchronous(invocation, methodName).GetAwaiter().GetResult();
        }
        else
        {
            // 使用 CallTraceContext.EnterScope() 来管理调用深度（仅同步方法）
            using (CallTraceContext.EnterScope())
            {
                try
                {
                    // 执行前逻辑
                    BeforeExecute(invocation, methodName);

                    // 执行方法
                    invocation.Proceed();

                    // 执行后逻辑
                    AfterExecute(invocation, methodName);
                }
                catch (Exception ex)
                {
                    // 异常处理
                    OnException(invocation, methodName, ex);
                    //throw;
                }
                finally
                {
                    // 清理逻辑
                    OnFinally(invocation, methodName);
                }
            }
        }
    }

    /// <summary>
    /// 获取方法调用层级缩进
    /// </summary>
    protected string GetIndent(int depth)
    {
        return $" [方法链层级:{depth}] ";

        // 每层级 2 个空格，最多 10 层
        //var indentCount = Math.Min(depth * 2, 20);
        //return new string(' ', indentCount);
    }


    protected bool IsAsyncMethod(System.Reflection.MethodInfo method)
    {
        return method.ReturnType == typeof(Task) ||
               (method.ReturnType.IsGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>));
    }


    private async Task InternalInterceptAsynchronous(IInvocation invocation, string methodName)
    {
        // 使用 CallTraceContext.EnterScope() 来管理调用深度
        using (CallTraceContext.EnterScope())
        {
            try
            {
                // 执行前逻辑
                await BeforeExecuteAsync(invocation, methodName);

                // 执行方法
                invocation.Proceed();
                if(invocation.ReturnValue.GetType() == typeof(Task))
                {
                    var task = (Task)invocation.ReturnValue;
                    await task;
                }
                

                // 执行后逻辑
                await AfterExecuteAsync(invocation, methodName);
            }
            catch (Exception ex)
            {
                // 异常处理
                await OnExceptionAsync(invocation, methodName, ex);
                throw;
            }
            finally
            {
                // 清理逻辑
                await OnFinallyAsync(invocation, methodName);
            }
        }
    }

    private async Task<TResult> InternalInterceptAsynchronous<TResult>(IInvocation invocation, string methodName)
    {
        // 使用 CallTraceContext.EnterScope() 来管理调用深度
        using (CallTraceContext.EnterScope())
        {
            try
            {
                // 执行前逻辑
                await BeforeExecuteAsync(invocation, methodName);

                // 执行方法
                invocation.Proceed();
                var task = (Task<TResult>)invocation.ReturnValue;
                var result = await task;

                // 执行后逻辑（带返回值）
                await AfterExecuteAsyncWithResult(invocation, methodName, result);

                return result;
            }
            catch (Exception ex)
            {
                // 异常处理
                await OnExceptionAsync(invocation, methodName, ex);
                throw;
            }
            finally
            {
                // 清理逻辑
                await OnFinallyAsync(invocation, methodName);
            }
        }
    }

    /// <summary>
    /// 执行前逻辑（同步）
    /// </summary>
    protected virtual void BeforeExecute(IInvocation invocation, string methodName)
    {
    }

    /// <summary>
    /// 执行后逻辑（同步）
    /// </summary>
    protected virtual void AfterExecute(IInvocation invocation, string methodName)
    {
    }

    /// <summary>
    /// 异常处理（同步）
    /// </summary>
    protected virtual void OnException(IInvocation invocation, string methodName, Exception ex)
    {
    }

    /// <summary>
    /// 清理逻辑（同步）
    /// </summary>
    protected virtual void OnFinally(IInvocation invocation, string methodName)
    {
    }

    /// <summary>
    /// 执行前逻辑（异步）
    /// </summary>
    protected virtual Task BeforeExecuteAsync(IInvocation invocation, string methodName)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// 执行后逻辑（异步）
    /// </summary>
    protected virtual Task AfterExecuteAsync(IInvocation invocation, string methodName)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// 执行后逻辑（异步，带返回值）
    /// </summary>
    protected virtual Task AfterExecuteAsyncWithResult<TResult>(IInvocation invocation, string methodName, TResult result)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// 异常处理（异步）
    /// </summary>
    protected virtual Task OnExceptionAsync(IInvocation invocation, string methodName, Exception ex)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// 清理逻辑（异步）
    /// </summary>
    protected virtual Task OnFinallyAsync(IInvocation invocation, string methodName)
    {
        return Task.CompletedTask;
    }


}
