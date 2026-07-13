using KH.WMS.Core.Api.Responses;
using KH.WMS.Core.Exceptions;
using KH.WMS.Core.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KH.WMS.Core.Filters.Exception;

/// <summary>
/// 自定义异常过滤器
/// </summary>
public class CustomExceptionFilter : IExceptionFilter, IAsyncExceptionFilter
{
    private readonly ILoggerService _logger;
    private readonly IHostEnvironment _environment;

    public CustomExceptionFilter(ILoggerService logger, IHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }

    public void OnException(ExceptionContext context)
    {
        _logger.LogError(context.Exception, "发生未处理异常: {Message}", context.Exception.Message);

        var result = CreateErrorResponse(context.Exception, context.HttpContext.TraceIdentifier);
        context.Result = result;
        context.ExceptionHandled = true;
    }

    public Task OnExceptionAsync(ExceptionContext context)
    {
        OnException(context);
        return Task.CompletedTask;
    }

    private IActionResult CreateErrorResponse(System.Exception exception, string traceId)
    {
        var response = new ApiResponse { TraceId = traceId };

        switch (exception)
        {
            case BusinessException businessException:
                response.Code = businessException.ErrorCode;
                response.Message = businessException.Message;
                response.Data = businessException.Details;
                return new ObjectResult(response) { StatusCode = 400 };

            case ValidationException validationException:
                response.Code = ErrorCodes.VALIDATION_ERROR;
                response.Message = "数据验证失败";
                response.Data = validationException.Errors;
                return new ObjectResult(response) { StatusCode = 400 };

            case NotFoundException notFoundException:
                response.Code = ErrorCodes.NOT_FOUND;
                response.Message = notFoundException.Message;
                return new ObjectResult(response) { StatusCode = 404 };

            case UnauthorizedAccessException:
                response.Code = ErrorCodes.UNAUTHORIZED;
                response.Message = "未授权访问";
                return new ObjectResult(response) { StatusCode = 401 };

            default:
                response.Code = ErrorCodes.SYSTEM_ERROR;
                response.Message = _environment.IsDevelopment()
                    ? exception.Message
                    : "系统发生错误，请稍后重试";

                if (_environment.IsDevelopment())
                {
                    response.Data = new
                    {
                        exception.GetType().Name,
                        exception.Message,
                        exception.StackTrace
                    };
                }

                return new ObjectResult(response) { StatusCode = 500 };
        }
    }
}

/// <summary>
/// 全局异常处理特性
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class HandleExceptionAttribute : Attribute, IFilterFactory
{
    public bool IsReusable => false;

    public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
    {
        var logger = serviceProvider.GetRequiredService<ILoggerService>();
        var environment = serviceProvider.GetRequiredService<IHostEnvironment>();
        return new CustomExceptionFilter(logger, environment);
    }
}
