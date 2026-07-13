using System.Text;
using KH.WMS.Core.Constants;
using KH.WMS.Core.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace KH.WMS.Core.Authentication.JWT;

/// <summary>
/// JWT Bearer 认证扩展
/// </summary>
public static class JwtBearerExtensions
{
    /// <summary>
    /// 添加 JWT Bearer 认证
    /// </summary>
    public static IServiceCollection AddJwtBearerAuthentication(this IServiceCollection services, JwtTokenOptions jwtOptions)
    {
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            // 关闭 Claim 类型映射，保持原始 JWT 标准名称（exp、iss、aud 等）
            // 否则 exp 会被映射为 http://schemas.microsoft.com/... 导致按 JwtRegisteredClaimNames.Exp 查找时找不到
            options.MapInboundClaims = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                SaveSigninToken = true,
                ValidateIssuer = jwtOptions.ValidateIssuer,
                ValidateAudience = jwtOptions.ValidateAudience,
                ValidateLifetime = jwtOptions.ValidateLifetime,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtOptions.Issuer,
                ValidAudience = jwtOptions.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret)),
                ClockSkew = TimeSpan.FromSeconds(jwtOptions.ClockSkewSeconds)
            };

            // 事件处理
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    var token = context.Token;
                    if (string.IsNullOrEmpty(token))
                    {
                        var authHeader = context.Request.Headers[HeaderConstants.Authentication.AUTHORIZATION].ToString();
                        if (authHeader.StartsWith(HeaderConstants.Authentication.BEARER_PREFIX, StringComparison.OrdinalIgnoreCase))
                        {
                            context.Token = authHeader[HeaderConstants.Authentication.BEARER_PREFIX.Length..].Trim();
                        }
                    }
                    return Task.CompletedTask;
                },
                OnAuthenticationFailed = context =>
                {
                    var logger = context.HttpContext.RequestServices.GetService<ILoggerFactory>()?.CreateLogger("JwtBearer");
                    logger?.LogError(context.Exception, "JWT 认证失败: {Message}", context.Exception?.Message);
                    return Task.CompletedTask;
                },
                OnTokenValidated = context =>
                {
                    return Task.CompletedTask;
                },
                OnChallenge = context =>
                {
                    context.HandleResponse();
                    context.Response.Clear();
                    context.Response.ContentType = "application/json";
                    context.Response.StatusCode = 401;
                    context.Response.WriteAsync(new { message = "授权未通过", status = false, code = 401 }.Serialize());
                    return Task.CompletedTask;
                }
            };
        });

        return services;
    }
}
