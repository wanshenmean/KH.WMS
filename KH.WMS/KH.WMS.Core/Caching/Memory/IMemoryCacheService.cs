using Microsoft.Extensions.Caching.Memory;

namespace KH.WMS.Core.Caching.Memory;

/// <summary>
/// 内存缓存服务接口
/// </summary>
public interface IMemoryCacheService : ICacheService
{
    /// <summary>
    /// 设置带滑动过期时间的缓存
    /// </summary>
    void SetWithSlidingExpiration<T>(string key, T value, TimeSpan slidingExpiration);

    /// <summary>
    /// 设置带绝对和滑动过期时间的缓存
    /// </summary>
    void SetWithHybridExpiration<T>(string key, T value, TimeSpan absoluteExpiration, TimeSpan slidingExpiration);

    /// <summary>
    /// 注册缓存过期回调
    /// </summary>
    void RegisterPostEvictionCallback(Action<object, object, EvictionReason, object> callback);
}
