using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using KH.WMS.Core.Caching;
using KH.WMS.Core.Constants;
using KH.WMS.Core.DependencyInjection.ServiceLifetimes;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

namespace KH.WMS.Core.Authentication.JWT;

/// <summary>
/// JWT Token 服务实现
/// </summary>
[RegisteredService(Lifetime = Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton, WithoutInterceptor = true)]
public class JwtTokenService : IJwtTokenService
{
    private readonly JwtTokenOptions _options;
    private readonly ICacheService _cacheService;

    /// <summary>
    /// RefreshToken 随机字节数（256 位），决定刷新令牌的熵长度。
    /// </summary>
    private const int RefreshTokenByteSize = 32;

    public JwtTokenService(IOptions<JwtTokenOptions> options, ICacheService cacheService)
    {
        _options = options.Value;
        _cacheService = cacheService;
    }

    public string GenerateAccessToken(long userId, string username, long roleId)
    {
        var expireMinutes = GetTokenExpireMinutes();

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // 与校验端保持一致的声明：jti=userId、role、unique_name
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Jti, userId.ToString()),
            new("role", roleId.ToString()),
            new("unique_name", username),
        };

        // 关闭出站 claim 类型映射，避免 "role"/"unique_name" 被改写为长 URI，
        // 保证 payload 中的 key 与 GetRolesFromToken/GetUsernameFromToken 的读取一致。
        var handler = new JwtSecurityTokenHandler();
        handler.OutboundClaimTypeMap.Clear();

        var token = handler.CreateToken(new SecurityTokenDescriptor
        {
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            Subject = new ClaimsIdentity(claims),
            NotBefore = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddMinutes(expireMinutes),
            SigningCredentials = creds,
        });

        return handler.WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[RefreshTokenByteSize];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public bool ValidateToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_options.Secret);

        try
        {
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = _options.ValidateIssuer,
                ValidateAudience = _options.ValidateAudience,
                ValidateLifetime = _options.ValidateLifetime,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _options.Issuer,
                ValidAudience = _options.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ClockSkew = TimeSpan.FromSeconds(_options.ClockSkewSeconds)
            }, out _);

            return true;
        }
        catch
        {
            return false;
        }
    }

    public long? GetUserIdFromToken(string token)
    {
        try
        {
            return Convert.ToInt32(new JwtSecurityTokenHandler().ReadJwtToken(token).Id);
        }
        catch { return null; }
    }

    public string? GetUsernameFromToken(string token)
    {
        var payload = DecodePayload(token);
        if (payload == null) return null;
        return payload.TryGetValue("unique_name", out var value) ? value?.ToString() : null;
    }

    public IEnumerable<string>? GetRolesFromToken(string token)
    {
        var payload = DecodePayload(token);
        if (payload == null) return null;
        if (payload.TryGetValue("role", out var value))
            return new[] { value?.ToString() };
        return null;
    }

    public (string accessToken, string refreshToken)? RefreshToken(string accessToken, string refreshToken)
    {
        if (!ValidateToken(accessToken))
            return null;

        var userId = GetUserIdFromToken(accessToken);
        var username = GetUsernameFromToken(accessToken);

        if (userId == null || username == null)
            return null;

        var newAccessToken = GenerateAccessToken(userId.Value, username, 0);
        var newRefreshToken = GenerateRefreshToken();
        return (newAccessToken, newRefreshToken);
    }

    public bool IsTokenExpired(string token)
    {
        var remaining = GetTokenRemainingSeconds(token);
        return remaining == null || remaining <= 0;
    }

    public bool ShouldRefreshToken(string token)
    {
        var remaining = GetTokenRemainingSeconds(token);
        if (remaining == null) return true;
        // 剩余时间不足过期时间的 1/3 时认为需要刷新
        var threshold = GetTokenExpireMinutes() / 3.0 * 60;
        return remaining < (long)threshold;
    }

    public long? GetTokenRemainingSeconds(string token)
    {
        var payload = DecodePayload(token);
        if (payload == null) return null;
        if (!payload.TryGetValue("exp", out var expObj)) return null;
        //if (expObj is Newtonsoft.Json.Linq.JValue jv && jv.Type == Newtonsoft.Json.Linq.JTokenType.Integer)
        {
            var exp = Convert.ToInt64(expObj);
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            return Math.Max(0, exp - now);
        }
        //return null;
    }

    /// <summary>
    /// 从缓存读取令牌过期时间，读取失败则使用 appsettings.json 中的默认值
    /// </summary>
    private int GetTokenExpireMinutes()
    {
        var value = _cacheService.Get<string>(CacheConstants.SysParam.GetKey(SysParamConstants.TOKEN_EXPIRE_MIN));
        if (!string.IsNullOrEmpty(value) && int.TryParse(value, out var minutes) && minutes > 0)
            return minutes;
        return _options.AccessTokenExpirationMinutes;
    }

    /// <summary>
    /// 手动解码 Token 的 Payload 部分
    /// </summary>
    private Dictionary<string, object>? DecodePayload(string token)
    {
        try
        {
            var parts = token.Split('.');
            if (parts.Length != 3) return null;
            var json = Encoding.UTF8.GetString(Base64UrlDecode(parts[1]));
            return JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
        }
        catch
        {
            return null;
        }
    }

    private static byte[] Base64UrlDecode(string input)
    {
        input = input.Replace('-', '+').Replace('_', '/');
        switch (input.Length % 4)
        {
            case 2: input += "=="; break;
            case 3: input += "="; break;
        }
        return Convert.FromBase64String(input);
    }
}
