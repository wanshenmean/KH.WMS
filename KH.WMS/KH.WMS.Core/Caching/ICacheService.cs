namespace KH.WMS.Core.Caching;

/// <summary>
/// 缓存服务接口
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// 获取缓存值
    /// </summary>
    T? Get<T>(string key);

    /// <summary>
    /// 尝试获取缓存值
    /// </summary>
    /// <returns>是否成功获取</returns>
    bool TryGet<T>(string key, out T? value);

    /// <summary>
    /// 设置缓存值
    /// </summary>
    void Set<T>(string key, T value, TimeSpan? expiration = null);

    /// <summary>
    /// 尝试设置缓存值（仅当缓存不存在时设置）
    /// </summary>
    /// <returns>是否成功设置</returns>
    bool TrySet<T>(string key, T value, TimeSpan? expiration = null);

    bool SetOrCreate<T>(string key, T value, TimeSpan? expiration = null);

    /// <summary>
    /// 删除缓存
    /// </summary>
    bool Remove(string key);

    /// <summary>
    /// 批量删除缓存
    /// </summary>
    void RemoveMultiple(IEnumerable<string> keys);

    /// <summary>
    /// 按前缀批量删除缓存（删除所有以 prefix 开头的键）
    /// </summary>
    void RemoveByPrefix(string prefix);

    /// <summary>
    /// 检查缓存是否存在
    /// </summary>
    bool Exists(string key);

    /// <summary>
    /// 获取或设置缓存
    /// </summary>
    T? GetOrCreate<T>(string key, Func<T> factory, TimeSpan? expiration = null);

    /// <summary>
    /// 异步获取或设置缓存
    /// </summary>
    Task<T?> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null);

    /// <summary>
    /// 刷新缓存（延长过期时间）
    /// </summary>
    bool Refresh(string key, TimeSpan? expiration = null);
}
