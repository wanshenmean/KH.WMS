using KH.WMS.Core.Api.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace KH.WMS.Core.Filters.Result;

/// <summary>
/// TraceId 结果过滤器，自动为 ApiResponse 注入 TraceId
/// </summary>
public class TraceIdResultFilter : IResultFilter
{
    public void OnResultExecuting(ResultExecutingContext context)
    {
        if (context.Result is ObjectResult { Value: ApiResponse response })
        {
            response.TraceId ??= context.HttpContext.TraceIdentifier;
        }
    }

    public void OnResultExecuted(ResultExecutedContext context) { }
}
