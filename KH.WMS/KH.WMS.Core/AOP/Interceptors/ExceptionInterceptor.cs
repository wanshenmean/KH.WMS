using Castle.DynamicProxy;
using KH.WMS.Core.Exceptions;
using KH.WMS.Core.Logging;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace KH.WMS.Core.AOP.Interceptors;

/// <summary>
/// 异常拦截器 - 统一异常处理
/// </summary>
public class ExceptionInterceptor : IInterceptor
{
    private readonly ILoggerService _logger;

    public ExceptionInterceptor(ILoggerService logger)
    {
        _logger = logger;
    }

    public void Intercept(IInvocation invocation)
    {
        var methodName = GetMethodName(invocation);

        try
        {
            invocation.Proceed();
        }
        catch (BusinessException ex)
        {
            _logger.LogError(ex, "业务异常: {Method}", methodName);
            throw;
        }
        catch (ValidationException ex)
        {
            _logger.LogError(ex, "验证异常: {Method}", methodName);
            throw;
        }
        catch (NotFoundException ex)
        {
            _logger.LogError(ex, "未找到异常: {Method}", methodName);
            throw;
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError(ex, "未授权访问: {Method}", methodName);
            throw;
        }
        catch(SqlException ex)
        {
            _logger.LogError(ex, "数据库异常: {Method}", methodName);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "系统异常: {Method}", methodName);
            throw;
        }

    }

    private string GetMethodName(IInvocation invocation)
    {
        return $"{invocation.TargetType.Name}.{invocation.Method.Name}";
    }

}
