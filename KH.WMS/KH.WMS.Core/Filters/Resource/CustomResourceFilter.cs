using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace KH.WMS.Core.Filters.Resource;

/// <summary>
/// 自定义资源过滤器
/// </summary>
public class CustomResourceFilter : IResourceFilter, IAsyncResourceFilter
{
    private readonly ILogger<CustomResourceFilter> _logger;
    private static readonly Dictionary<string, object> _cache = new();

    public CustomResourceFilter(ILogger<CustomResourceFilter> logger)
    {
        _logger = logger;
    }

    public void OnResourceExecuting(ResourceExecutingContext context)
    {
        var cacheKey = context.HttpContext.Request.Path.ToString();

        if (_cache.TryGetValue(cacheKey, out var cachedResult))
        {
            _logger.LogInformation("缓存命中: {Key}", cacheKey);
            context.Result = new ObjectResult(cachedResult);
        }
    }

    public void OnResourceExecuted(ResourceExecutedContext context)
    {
        if (context.Result is ObjectResult result)
        {
            var cacheKey = context.HttpContext.Request.Path.ToString();
            _cache[cacheKey] = result.Value;
            _logger.LogInformation("缓存已设置: {Key}", cacheKey);
        }
    }

    public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
    {
        // 执行前
        OnResourceExecuting(context);

        if (context.Result != null)
        {
            return; // 缓存命中
        }

        // 执行
        await next();

        // 执行后
        //OnResourceExecuted(context.Result);
    }
}

/// <summary>
/// 资源缓存特性
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class CacheResourceAttribute : Attribute, IFilterFactory
{
    public bool IsReusable => false;

    public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
    {
        var logger = serviceProvider.GetRequiredService<ILogger<CustomResourceFilter>>();
        return new CustomResourceFilter(logger);
    }
}
