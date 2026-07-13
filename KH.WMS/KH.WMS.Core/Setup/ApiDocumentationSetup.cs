using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KH.WMS.Core.Setup;

/// <summary>
/// API 文档配置
/// </summary>
public static class ApiDocumentationSetup
{
    /// <summary>
    /// 配置 Swagger
    /// </summary>
    public static IServiceCollection AddApiDocumentationSetup(this IServiceCollection services, IConfiguration configuration)
    {
        return KH.WMS.Core.Api.Documentation.Swagger.SwaggerSetup.AddSwaggerDocumentation(services, configuration);
    }

    /// <summary>
    /// 使用 Swagger
    /// </summary>
    public static IApplicationBuilder UseSwaggerDocumentation(this IApplicationBuilder app)
    {
        // 从服务获取配置
        var serviceProvider = app.ApplicationServices;
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        return KH.WMS.Core.Api.Documentation.Swagger.SwaggerSetup.UseSwaggerDocumentation(app, configuration);
    }
}
