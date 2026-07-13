using KH.WMS.Core.DependencyInjection.ServiceLifetimes;
using Microsoft.Extensions.DependencyInjection;

namespace KH.WMS.Core.Authentication.JWT;

/// <summary>
/// JWT 配置选项
/// </summary>
[SelfRegisteredService(Lifetime = ServiceLifetime.Singleton)]
public class JwtTokenOptions
{
    /// <summary>
    /// 密钥。必须在 appsettings.json 的 "Jwt:Secret" 中配置至少 32 位的强密钥；
    /// 留空时启动会直接失败（见 AuthenticationSetup），避免使用公开的硬编码默认值。
    /// </summary>
    public string Secret { get; set; } = string.Empty;

    /// <summary>
    /// 发行者
    /// </summary>
    public string Issuer { get; set; } = "KH.WMS";

    /// <summary>
    /// 受众
    /// </summary>
    public string Audience { get; set; } = "KH.WMS";

    /// <summary>
    /// 访问令牌过期时间（分钟）
    /// </summary>
    public int AccessTokenExpirationMinutes { get; set; } = 30;

    /// <summary>
    /// 刷新令牌过期时间（天）
    /// </summary>
    public int RefreshTokenExpirationDays { get; set; } = 7;

    /// <summary>
    /// 时钟偏移（秒）
    /// </summary>
    public int ClockSkewSeconds { get; set; } = 5;

    /// <summary>
    /// 验证参数
    /// </summary>
    public bool ValidateIssuer { get; set; } = true;

    public bool ValidateAudience { get; set; } = true;

    public bool ValidateLifetime { get; set; } = true;
}
