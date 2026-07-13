using Microsoft.Extensions.Caching.Memory;

namespace KH.WMS.Core.Caching.Memory;

/// <summary>
/// 缓存策略配置
/// </summary>
public class CacheEntryOptions
{
    /// <summary>
    /// 默认缓存策略
    /// </summary>
    public static MemoryCacheEntryOptions Default => new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
    };

    /// <summary>
    /// 短期缓存（5分钟）
    /// </summary>
    public static MemoryCacheEntryOptions Short => new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
    };

    /// <summary>
    /// 中期缓存（30分钟）
    /// </summary>
    public static MemoryCacheEntryOptions Medium => new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
    };

    /// <summary>
    /// 长期缓存（2小时）
    /// </summary>
    public static MemoryCacheEntryOptions Long => new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(2)
    };

    /// <summary>
    /// 滑动过期缓存
    /// </summary>
    public static MemoryCacheEntryOptions Sliding => new()
    {
        SlidingExpiration = TimeSpan.FromMinutes(10)
    };

    /// <summary>
    /// 永不过期缓存（谨慎使用）
    /// </summary>
    public static MemoryCacheEntryOptions Never => new()
    {
        Priority = CacheItemPriority.NeverRemove
    };

    /// <summary>
    /// 创建自定义缓存策略
    /// </summary>
    public static MemoryCacheEntryOptions Create(TimeSpan? absoluteExpiration = null, TimeSpan? slidingExpiration = null)
    {
        var options = new MemoryCacheEntryOptions();

        if (absoluteExpiration.HasValue)
        {
            options.AbsoluteExpirationRelativeToNow = absoluteExpiration.Value;
        }

        if (slidingExpiration.HasValue)
        {
            options.SlidingExpiration = slidingExpiration.Value;
        }

        return options;
    }
}
