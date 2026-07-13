namespace KH.WMS.Core.Attributes;

/// <summary>
/// 缓存特性 - 标记方法结果需要缓存
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
public class CacheAttribute : Attribute
{
    /// <summary>
    /// 缓存时长（秒）
    /// </summary>
    public int Duration { get; set; } = 60 * 5;

    /// <summary>
    /// 缓存键前缀
    /// </summary>
    public string? KeyPrefix { get; set; }

    /// <summary>
    /// 是否启用缓存
    /// </summary>
    public bool Enable { get; set; } = true;

    /// <summary>
    /// 缓存键中是否包含用户信息
    /// </summary>
    public bool IncludeUser { get; set; } = false;

    public CacheAttribute()
    {
    }

    public CacheAttribute(int duration)
    {
        Duration = duration;
    }

    public CacheAttribute(int duration, string keyPrefix)
    {
        Duration = duration;
        KeyPrefix = keyPrefix;
    }
}
