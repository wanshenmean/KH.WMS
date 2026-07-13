using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.AspNetCore.Mvc.ApiExplorer;

namespace KH.WMS.Core.Api.Documentation.Swagger;

/// <summary>
/// Swagger 默认值操作过滤器
/// </summary>
public class SwaggerDefaultValues : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var apiDescription = context.ApiDescription;

        // 设置操作 ID
        var actionId = apiDescription.ActionDescriptor.AttributeRouteInfo?.Name ?? apiDescription.RelativePath;
        operation.OperationId = actionId?.Replace("/", "_");

        // 添加路径信息到描述
        if (apiDescription.RelativePath != null)
        {
            operation.Description ??= "";
            operation.Description += $"<br/><b>路径:</b> {apiDescription.RelativePath}";
        }
    }
}
