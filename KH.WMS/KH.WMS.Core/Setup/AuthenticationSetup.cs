using KH.WMS.Core.Authentication.JWT;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace KH.WMS.Core.Setup;

/// <summary>
/// 认证配置
/// </summary>
public static class AuthenticationSetup
{
    /// <summary>
    /// 配置 JWT 认证
    /// </summary>
    public static IServiceCollection AddAuthenticationSetup(this IServiceCollection services, IConfiguration configuration)
    {
        // 绑定 Jwt 配置到 JwtTokenOptions（JwtTokenService 等通过 IOptions<JwtTokenOptions> 读取）
        services.Configure<JwtTokenOptions>(configuration.GetSection("Jwt"));

        var jwtOptions = configuration.GetSection("Jwt").Get<JwtTokenOptions>() ?? new JwtTokenOptions();

        // 安全前置校验：必须配置足够强度的签名密钥，否则拒绝启动（杜绝使用公开的硬编码默认密钥）
        if (string.IsNullOrWhiteSpace(jwtOptions.Secret) || jwtOptions.Secret.Length < 32)
        {
            throw new InvalidOperationException(
                "JWT Secret 未配置或不安全：请在 appsettings.json 的 'Jwt:Secret' 中配置至少 32 位的强密钥，生产环境务必覆盖默认值。");
        }

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret));

        // 标准 JwtBearer 配置：直接在 AddJwtBearer 委托内设置 TokenValidationParameters，
        // 确保中间件对每个携带 Bearer token 的请求都按签发密钥做 HS256 验签，
        // 验签通过即把 HttpContext.User 置为已认证（IsAuthenticated=true）。
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.MapInboundClaims = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = jwtOptions.ValidateIssuer,
                ValidateAudience = jwtOptions.ValidateAudience,
                ValidateLifetime = jwtOptions.ValidateLifetime,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtOptions.Issuer,
                ValidAudience = jwtOptions.Audience,
                IssuerSigningKey = signingKey,
                ClockSkew = TimeSpan.FromSeconds(jwtOptions.ClockSkewSeconds)
            };

            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetService<ILoggerFactory>()?.CreateLogger("JwtBearer");
                    logger?.LogError(context.Exception, "JWT 认证失败: {Message}", context.Exception?.Message);
                    return Task.CompletedTask;
                },
                OnChallenge = context =>
                {
                    context.HandleResponse();
                    context.Response.Clear();
                    context.Response.ContentType = "application/json";
                    context.Response.StatusCode = 401;
                    context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(new { message = "授权未通过", status = false, code = 401 }));
                    return Task.CompletedTask;
                }
            };
        });

        return services;
    }

    /// <summary>
    /// 配置授权策略
    /// </summary>
    public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            // 这里可以添加自定义策略
            // options.AddPolicy("AdminOnly", policy => policy.RequireRole("admin"));
        });

        return services;
    }
}
