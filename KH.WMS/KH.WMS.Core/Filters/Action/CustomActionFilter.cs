using System.Diagnostics;
using KH.WMS.Core.Logging;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace KH.WMS.Core.Filters.Action;

/// <summary>
/// 自定义动作过滤器
/// </summary>
public class CustomActionFilter : IActionFilter, IAsyncActionFilter
{
    private readonly ILoggerService _logger;

    public CustomActionFilter(ILoggerService logger)
    {
        _logger = logger;
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger.LogInfo("开始执行: {Controller}/{Action}",
            context.RouteData.Values["controller"],
            context.RouteData.Values["action"]);
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        _logger.LogInfo("执行完成: {Controller}/{Action}",
            context.RouteData.Values["controller"],
            context.RouteData.Values["action"]);
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var stopwatch = Stopwatch.StartNew();

        // 执行前
        OnActionExecuting(context);

        // 执行动作
        var executedContext = await next();

        // 执行后
        stopwatch.Stop();

        if (executedContext.Exception == null)
        {
            _logger.LogInfo("执行成功: {Controller}/{Action}, 耗时: {Elapsed}ms",
                context.RouteData.Values["controller"],
                context.RouteData.Values["action"],
                stopwatch.ElapsedMilliseconds);
        }
        else
        {
            _logger.LogError(executedContext.Exception, "执行失败: {Controller}/{Action}, 耗时: {Elapsed}ms",
                context.RouteData.Values["controller"],
                context.RouteData.Values["action"],
                stopwatch.ElapsedMilliseconds);
        }

        OnActionExecuted(executedContext);
    }
}

/// <summary>
/// 动作日志特性
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class LogActionAttribute : Attribute, IFilterFactory
{
    public bool IsReusable => false;

    public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
    {
        var logger = serviceProvider.GetRequiredService<ILoggerService>();
        return new CustomActionFilter(logger);
    }
}
