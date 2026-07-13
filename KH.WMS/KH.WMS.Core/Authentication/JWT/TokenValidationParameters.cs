using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace KH.WMS.Core.Authentication.JWT;

/// <summary>
/// Token 验证参数扩展
/// </summary>
public static class TokenValidationParametersExtensions
{
    /// <summary>
    /// 创建标准 Token 验证参数
    /// </summary>
    public static Microsoft.IdentityModel.Tokens.TokenValidationParameters CreateParameters(JwtTokenOptions options)
    {
        return new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = options.ValidateIssuer,
            ValidateAudience = options.ValidateAudience,
            ValidateLifetime = options.ValidateLifetime,
            ValidateIssuerSigningKey = true,
            ValidIssuer = options.Issuer,
            ValidAudience = options.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Secret)),
            ClockSkew = TimeSpan.FromSeconds(options.ClockSkewSeconds)
        };
    }
}
