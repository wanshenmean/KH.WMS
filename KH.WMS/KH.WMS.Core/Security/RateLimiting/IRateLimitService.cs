namespace KH.WMS.Core.Security.RateLimiting;

/// <summary>
/// 限流服务接口
/// </summary>
public interface IRateLimitService
{
    /// <summary>
    /// 检查是否允许请求
    /// </summary>
    Task<bool> IsAllowedAsync(string key, int limit, TimeSpan window);

    /// <summary>
    /// 获取当前计数
    /// </summary>
    Task<int> GetCurrentCountAsync(string key);

    /// <summary>
    /// 重置计数
    /// </summary>
    Task ResetAsync(string key);

    /// <summary>
    /// 获取剩余配额
    /// </summary>
    Task<int> GetRemainingQuotaAsync(string key, int limit);
}
