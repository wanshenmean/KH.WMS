using KH.WMS.Core.Caching;
using Microsoft.Extensions.Caching.Memory;

namespace KH.WMS.Core.Security.RateLimiting;

/// <summary>
/// 滑动窗口限流器
/// </summary>
public class SlidingWindowRateLimiter : IRateLimitService
{
    private readonly ICacheService _cache;

    public SlidingWindowRateLimiter(ICacheService cache)
    {
        _cache = cache;
    }

    public Task<bool> IsAllowedAsync(string key, int limit, TimeSpan window)
    {
        var counter = _cache.GetOrCreate(key, () =>
        {
            return new SlidingWindowCounter();
        });

        lock (counter)
        {
            var now = DateTime.UtcNow;

            // 清理过期记录
            counter.Requests = counter.Requests
                .Where(r => r > now.Subtract(window))
                .ToList();

            // 检查是否超过限制
            if (counter.Requests.Count >= limit)
            {
                return Task.FromResult(false);
            }

            // 添加当前请求
            counter.Requests.Add(now);
            return Task.FromResult(true);
        }
    }

    public Task<int> GetCurrentCountAsync(string key)
    {
        if (_cache.TryGet(key, out SlidingWindowCounter? counter) && counter != null)
        {
            lock (counter)
            {
                return Task.FromResult(counter.Requests.Count);
            }
        }

        return Task.FromResult(0);
    }

    public Task ResetAsync(string key)
    {
        _cache.Remove(key);
        return Task.CompletedTask;
    }

    public Task<int> GetRemainingQuotaAsync(string key, int limit)
    {
        var current = GetCurrentCountAsync(key).Result;
        var remaining = Math.Max(0, limit - current);
        return Task.FromResult(remaining);
    }
}

/// <summary>
/// 滑动窗口计数器
/// </summary>
public class SlidingWindowCounter
{
    public List<DateTime> Requests { get; set; } = new();
}

/// <summary>
/// 固定窗口限流器
/// </summary>
public class FixedWindowRateLimiter : IRateLimitService
{
    private readonly ICacheService _cache;

    public FixedWindowRateLimiter(ICacheService cache)
    {
        _cache = cache;
    }

    public Task<bool> IsAllowedAsync(string key, int limit, TimeSpan window)
    {
        var windowKey = $"{key}_{DateTime.UtcNow.Ticks / window.Ticks}";
        var counter = _cache.GetOrCreate(windowKey, () =>
        {
            return new FixedWindowCounter { Count = 0 };
        });

        lock (counter)
        {
            if (counter.Count >= limit)
            {
                return Task.FromResult(false);
            }

            counter.Count++;
            return Task.FromResult(true);
        }
    }

    public Task<int> GetCurrentCountAsync(string key)
    {
        // 固定窗口需要知道当前窗口的key
        // 这里简化处理，返回0
        return Task.FromResult(0);
    }

    public Task ResetAsync(string key)
    {
        _cache.Remove(key);
        return Task.CompletedTask;
    }

    public Task<int> GetRemainingQuotaAsync(string key, int limit)
    {
        // 固定窗口需要知道当前窗口的key
        return Task.FromResult(limit);
    }
}

/// <summary>
/// 固定窗口计数器
/// </summary>
public class FixedWindowCounter
{
    public int Count { get; set; }
}

/// <summary>
/// 令牌桶限流器
/// </summary>
public class TokenBucketRateLimiter : IRateLimitService
{
    private readonly ICacheService _cache;

    public TokenBucketRateLimiter(ICacheService cache)
    {
        _cache = cache;
    }

    public Task<bool> IsAllowedAsync(string key, int limit, TimeSpan window)
    {
        var bucket = _cache.GetOrCreate(key, () =>
        {
            return new TokenBucket
            {
                Tokens = limit,
                LastRefill = DateTime.UtcNow,
                Capacity = limit,
                RefillRate = limit / (double)window.TotalSeconds
            };
        });

        lock (bucket)
        {
            var now = DateTime.UtcNow;
            var elapsed = (now - bucket.LastRefill).TotalSeconds;

            // 补充令牌
            var tokensToAdd = (int)(elapsed * bucket.RefillRate);
            bucket.Tokens = Math.Min(bucket.Capacity, bucket.Tokens + tokensToAdd);
            bucket.LastRefill = now;

            // 检查是否有令牌
            if (bucket.Tokens > 0)
            {
                bucket.Tokens--;
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }
    }

    public Task<int> GetCurrentCountAsync(string key)
    {
        if (_cache.TryGet(key, out TokenBucket? bucket) && bucket != null)
        {
            lock (bucket)
            {
                return Task.FromResult(bucket.Tokens);
            }
        }

        return Task.FromResult(0);
    }

    public Task ResetAsync(string key)
    {
        _cache.Remove(key);
        return Task.CompletedTask;
    }

    public Task<int> GetRemainingQuotaAsync(string key, int limit)
    {
        return GetCurrentCountAsync(key);
    }
}

/// <summary>
/// 令牌桶
/// </summary>
public class TokenBucket
{
    public int Tokens { get; set; }
    public DateTime LastRefill { get; set; }
    public int Capacity { get; set; }
    public double RefillRate { get; set; }
}

/// <summary>
/// 限流配置选项
/// </summary>
public class RateLimitOptions
{
    /// <summary>
    /// 时间窗口内允许的请求数
    /// </summary>
    public int RequestLimit { get; set; } = 100;

    /// <summary>
    /// 时间窗口（秒）
    /// </summary>
    public int WindowSeconds { get; set; } = 60;

    /// <summary>
    /// 限流策略
    /// </summary>
    public RateLimitStrategy Strategy { get; set; } = RateLimitStrategy.SlidingWindow;
}

/// <summary>
/// 限流策略
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
