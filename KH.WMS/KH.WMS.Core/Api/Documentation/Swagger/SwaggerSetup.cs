using KH.WMS.Core.Constants;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
namespace KH.WMS.Core.Api.Documentation.Swagger;

/// <summary>
/// Swagger 配置
/// </summary>
public static class SwaggerSetup
{
    /// <summary>
    /// 添加 Swagger 服务
    /// </summary>
    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services, IConfiguration configuration)
    {
        var swaggerOptions = configuration.GetSection("Swagger").Get<SwaggerOptions>();
        swaggerOptions ??= new SwaggerOptions();

        services.AddSwaggerGen(options =>
        {
            var info = new OpenApiInfo
            {
                Title = swaggerOptions.Title,
                Version = swaggerOptions.Version,
                Description = swaggerOptions.Description
            };

            // 只有在 URL 不为空时才设置 Contact
            if (!string.IsNullOrWhiteSpace(swaggerOptions.ContactName) ||
                !string.IsNullOrWhiteSpace(swaggerOptions.ContactEmail) ||
                !string.IsNullOrWhiteSpace(swaggerOptions.ContactUrl))
            {
                info.Contact = new OpenApiContact
                {
                    Name = swaggerOptions.ContactName,
                    Email = swaggerOptions.ContactEmail
                };
                if (!string.IsNullOrWhiteSpace(swaggerOptions.ContactUrl))
                {
                    info.Contact.Url = new Uri(swaggerOptions.ContactUrl);
                }
            }

            // 只有在 URL 不为空时才设置 License
            if (!string.IsNullOrWhiteSpace(swaggerOptions.LicenseName) ||
                !string.IsNullOrWhiteSpace(swaggerOptions.LicenseUrl))
            {
                info.License = new OpenApiLicense
                {
                    Name = swaggerOptions.LicenseName
                };
                if (!string.IsNullOrWhiteSpace(swaggerOptions.LicenseUrl))
                {
                    info.License.Url = new Uri(swaggerOptions.LicenseUrl);
                }
            }

            options.SwaggerDoc(swaggerOptions.Version, info);

            // 添加 JWT 认证
            if (swaggerOptions.EnableJwt)
            {
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT 授权令牌，请在下方输入框中输入 token（无需输入 Bearer 前缀）",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT"
                });

                options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("Bearer", document)] = []
                });
            }

            // 添加 XML 注释
            var xmlFile = $"{System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath);
            }

            // 自定义操作过滤器（可选）
            // options.OperationFilter<SwaggerDefaultValues>();
            // options.DocumentFilter<SwaggerHeaderFilter>();
        });

        return services;
    }

    /// <summary>
    /// 使用 Swagger 中间件
    /// </summary>
    public static IApplicationBuilder UseSwaggerDocumentation(this IApplicationBuilder app, IConfiguration configuration)
    {
        var swaggerOptions = configuration.GetSection(AppSettingsConstants.Swagger).Get<SwaggerOptions>();
        swaggerOptions ??= new SwaggerOptions();

        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint($"/swagger/{swaggerOptions.Version}/swagger.json", $"{swaggerOptions.Title} {swaggerOptions.Version}");
            options.RoutePrefix = swaggerOptions.RoutePrefix;
            options.DocumentTitle = swaggerOptions.Title;
            options.DefaultModelsExpandDepth(-1); // 隐藏模型
        });

        return app;
    }
}

/// <summary>
/// Swagger 配置选项
/// </summary>
public class SwaggerOptions
{
    /// <summary>
    /// 标题
    /// </summary>
    public string Title { get; set; } = "API Documentation";

    /// <summary>
    /// 版本
    /// </summary>
    public string Version { get; set; } = "v1";

    /// <summary>
    /// 描述
    /// </summary>
    public string Description { get; set; } = "API Documentation";

    /// <summary>
    /// 联系人名称
    /// </summary>
    public string ContactName { get; set; } = "";

    /// <summary>
    /// 联系人邮箱
    /// </summary>
    public string ContactEmail { get; set; } = "";

    /// <summary>
    /// 联系人URL
    /// </summary>
    public string ContactUrl { get; set; } = "";

    /// <summary>
    /// 许可证名称
    /// </summary>
    public string LicenseName { get; set; } = "";

    /// <summary>
    /// 许可证URL
    /// </summary>
    public string LicenseUrl { get; set; } = "";

    /// <summary>
    /// 路由前缀
    /// </summary>
    public string RoutePrefix { get; set; } = "swagger";

    /// <summary>
    /// 是否启用 JWT 认证
    /// </summary>
    public bool EnableJwt { get; set; } = true;
}
