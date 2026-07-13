using KH.WMS.Core.Constants;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;

namespace KH.WMS.Core.Api.Documentation.Swagger;

/// <summary>
/// Swagger 请求头过滤器
/// </summary>
public class SwaggerHeaderFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        foreach (var path in swaggerDoc.Paths.Values)
        {
            foreach (var operation in path.Operations.Values)
            {
                if (operation.Parameters == null)
                    operation.Parameters = new List<IOpenApiParameter>();

                // 添加通用请求头
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = HeaderConstants.Tracing.X_CORRELATION_ID,
                    In = ParameterLocation.Header,
                    Description = "关联ID - 用于追踪请求",
                    Required = false,
                    Schema = new OpenApiSchema { Type = JsonSchemaType.String }
                });

                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = HeaderConstants.Tracing.X_REQUEST_ID,
                    In = ParameterLocation.Header,
                    Description = "请求ID - 用于追踪请求",
                    Required = false,
                    Schema = new OpenApiSchema { Type = JsonSchemaType.String }
                });

                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = HeaderConstants.Client.X_CLIENT_VERSION,
                    In = ParameterLocation.Header,
                    Description = "客户端版本",
                    Required = false,
                    Schema = new OpenApiSchema { Type = JsonSchemaType.String }
                });

                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = HeaderConstants.Client.X_DEVICE_ID,
                    In = ParameterLocation.Header,
                    Description = "设备ID",
                    Required = false,
                    Schema = new OpenApiSchema { Type = JsonSchemaType.String }
                });
            }
        }
    }
}
