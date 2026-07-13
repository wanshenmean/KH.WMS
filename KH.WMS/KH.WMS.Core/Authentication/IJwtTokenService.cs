namespace KH.WMS.Core.Authentication;

/// <summary>
/// JWT Token 服务接口
/// </summary>
public interface IJwtTokenService
{
    string GenerateAccessToken(long userId, string username, long roleId);

    /// <summary>
    /// 生成访问令牌
    /// </summary>
    //string GenerateAccessToken(long userId, string username, IEnumerable<string> roles, IEnumerable<string>? permissions = null);

    /// <summary>
    /// 生成刷新令牌
    /// </summary>
    string GenerateRefreshToken();

    /// <summary>
    /// 验证令牌
    /// </summary>
    bool ValidateToken(string token);

    /// <summary>
    /// 从令牌中获取用户ID
    /// </summary>
    long? GetUserIdFromToken(string token);

    /// <summary>
    /// 从令牌中获取用户名
    /// </summary>
    string? GetUsernameFromToken(string token);

    /// <summary>
    /// 从令牌中获取角色
    /// </summary>
    IEnumerable<string>? GetRolesFromToken(string token);

    /// <summary>
    /// 刷新令牌
    /// </summary>
    (string accessToken, string refreshToken)? RefreshToken(string accessToken, string refreshToken);

    /// <summary>
    /// 判断 Token 是否已过期
    /// </summary>
    bool IsTokenExpired(string token);

    /// <summary>
    /// 判断 Token 是否即将过期（剩余时间不足 RefreshThresholdMinutes 时返回 true）
    /// </summary>
    bool ShouldRefreshToken(string token);

    /// <summary>
    /// 获取 Token 剩余有效时间（秒），已过期返回 0，无法解析返回 null
    /// </summary>
    long? GetTokenRemainingSeconds(string token);
}
