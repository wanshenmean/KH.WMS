using System.Collections.Concurrent;
using System.Linq;
using KH.WMS.Core.Logging;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace KH.WMS.Core.Caching.Memory;

/// <summary>
/// 内存缓存服务实现
/// </summary>
public class MemoryCacheService : IMemoryCacheService
{
    private readonly IMemoryCache _cache;
    private readonly ILoggerService _logger;
    private readonly ConcurrentDictionary<string, byte> _keys = new();

    public MemoryCacheService(IMemoryCache cache, ILoggerService logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public T? Get<T>(string key)
    {
        _cache.TryGetValue(key, out var value);
        return value is T result ? result : default;
    }

    public bool TryGet<T>(string key, out T? value)
    {
        var result = _cache.TryGetValue(key, out var cachedValue);
        value = cachedValue is T typedValue ? typedValue : default;
        return result;
    }

    public void Set<T>(string key, T value, TimeSpan? expiration = null)
    {
        var options = new MemoryCacheEntryOptions();

        if (expiration.HasValue)
        {
            options.SetAbsoluteExpiration(expiration.Value);
        }

        _cache.Set(key, value, options);
        _keys.TryAdd(key, 0);
        _logger.LogDebug("缓存已设置: {Key}, 过期时间: {Expiration}s", key, expiration?.TotalSeconds ?? 0);
    }

    public bool TrySet<T>(string key, T value, TimeSpan? expiration = null)
    {
        if (Exists(key))
        {
            return false;
        }

        Set(key, value, expiration);
        return true;
    }

    public bool Remove(string key)
    {
        _cache.Remove(key);
        _keys.TryRemove(key, out _);
        _logger.LogDebug("缓存已删除: {Key}", key);
        return true;
    }

    public void RemoveMultiple(IEnumerable<string> keys)
    {
        foreach (var key in keys)
        {
            Remove(key);
        }
    }

    /// <summary>
    /// 按前缀批量删除缓存：删除所有以 prefix 开头的键。
    /// IMemoryCache 不支持键枚举，故用本地 _keys 注册表追踪所有缓存键来实现前缀匹配。
    /// </summary>
    public void RemoveByPrefix(string prefix)
    {
        if (string.IsNullOrEmpty(prefix)) return;

        var matched = _keys.Keys.Where(k => k.StartsWith(prefix, StringComparison.Ordinal)).ToList();
        foreach (var key in matched)
        {
            _cache.Remove(key);
            _keys.TryRemove(key, out _);
        }
        _logger.LogDebug("按前缀删除缓存: {Prefix}, 删除 {Count} 项", prefix, matched.Count);
    }

    public bool Exists(string key)
    {
        return _cache.TryGetValue(key, out _);
    }

    public bool SetOrCreate<T>(string key, T value, TimeSpan? expiration = null)
    {
        if (Exists(key))
        {
            Set(key, value, expiration);
            return true;
        }
        Set(key, value, expiration);
        return true;
    }

    public T? GetOrCreate<T>(string key, Func<T> factory, TimeSpan? expiration = null)
    {
        return _cache.GetOrCreate(key, entry =>
        {
            if (expiration.HasValue)
            {
                entry.SetAbsoluteExpiration(expiration.Value);
            }
            return factory();
        });
    }

    public async Task<T?> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
    {
        return await _cache.GetOrCreateAsync(key, async entry =>
        {
            if (expiration.HasValue)
            {
                entry.SetAbsoluteExpiration(expiration.Value);
            }
            return await factory();
        });
    }

    public bool Refresh(string key, TimeSpan? expiration = null)
    {
        if (!TryGet<object>(key, out var value))
        {
            return false;
        }

        if (expiration.HasValue)
        {
            _cache.Set(key, value, expiration.Value);
        }

        return true;
    }

    public void SetWithSlidingExpiration<T>(string key, T value, TimeSpan slidingExpiration)
    {
        var options = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(slidingExpiration);

        _cache.Set(key, value, options);
    }

    public void SetWithHybridExpiration<T>(string key, T value, TimeSpan absoluteExpiration, TimeSpan slidingExpiration)
    {
        var options = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(absoluteExpiration)
            .SetSlidingExpiration(slidingExpiration);

        _cache.Set(key, value, options);
    }

    public void RegisterPostEvictionCallback(Action<object, object, EvictionReason, object> callback)
    {
        // 此方法需要在创建缓存项时注册回调
        _logger.LogDebug("注册缓存过期回调");
    }
}
