namespace KH.WMS.Core.Attributes;

/// <summary>
/// 限流特性 - 标记接口或方法的限流规则
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
public class RateLimitAttribute : Attribute
{
    /// <summary>
    /// 时间窗口内允许的最大请求数
    /// </summary>
    public int RequestLimit { get; set; } = 100;

    /// <summary>
    /// 时间窗口（秒）
    /// </summary>
    public int TimeWindowSeconds { get; set; } = 60;

    /// <summary>
    /// 限流键前缀
    /// </summary>
    public string? KeyPrefix { get; set; }

    /// <summary>
    /// 是否按IP限流
    /// </summary>
    public bool ByIp { get; set; } = true;

    /// <summary>
    /// 是否按用户限流
    /// </summary>
    public bool ByUser { get; set; } = false;

    /// <summary>
    /// 限流策略：固定窗口、滑动窗口、令牌桶
    /// </summary>
    public RateLimitStrategy Strategy { get; set; } = RateLimitStrategy.SlidingWindow;

    public RateLimitAttribute()
    {
    }

    public RateLimitAttribute(int requestLimit, int timeWindowSeconds)
    {
        RequestLimit = requestLimit;
        TimeWindowSeconds = timeWindowSeconds;
    }
}

/// <summary>
/// 限流策略枚举
/// </summary>
public enum RateLimitStrategy
{
    /// <summary>
    /// 固定窗口
    /// </summary>
    FixedWindow,

    /// <summary>
    /// 滑动窗口
    /// </summary>
    SlidingWindow,

    /// <summary>
    /// 令牌桶
    /// </summary>
    TokenBucket
}
