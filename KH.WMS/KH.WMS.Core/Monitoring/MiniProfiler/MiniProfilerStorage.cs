using StackExchange.Profiling;
using StackExchange.Profiling.Storage;
using System.Collections.Concurrent;

namespace KH.WMS.Core.Monitoring.MiniProfiler;

/// <summary>
/// MiniProfiler 内存存储
/// </summary>
public class MiniProfilerMemoryStorage : IAsyncStorage
{
    private readonly ConcurrentDictionary<Guid, StackExchange.Profiling.MiniProfiler> _profilers = new();
    private readonly ConcurrentDictionary<string, HashSet<Guid>> _unviewedProfilers = new();
    private readonly ConcurrentDictionary<string, HashSet<Guid>> _viewedProfilers = new();
    private readonly object _cleanupLock = new();
    private DateTime _lastCleanup = DateTime.UtcNow;

    public MiniProfilerMemoryStorage(TimeSpan? duration = null)
    {
        _duration = duration ?? TimeSpan.FromMinutes(5);
    }

    private readonly TimeSpan _duration;

    /// <summary>
    /// 获取指定时间范围内的 profiler 列表
    /// </summary>
    public IEnumerable<Guid> List(int maxResults, DateTime? start = null, DateTime? finish = null, ListResultsOrder orderBy = ListResultsOrder.Descending)
    {
        var query = _profilers.Values.AsEnumerable();

        // 时间过滤
        if (start.HasValue)
        {
            query = query.Where(p => p.Started >= start.Value);
        }
        if (finish.HasValue)
        {
            query = query.Where(p => p.Started <= finish.Value);
        }

        // 排序
        query = orderBy == ListResultsOrder.Descending
            ? query.OrderByDescending(p => p.Started)
            : query.OrderBy(p => p.Started);

        return query.Take(maxResults).Select(p => p.Id);
    }

    /// <summary>
    /// 加载指定 ID 的 profiler
    /// </summary>
    public StackExchange.Profiling.MiniProfiler? Load(Guid id)
    {
        _profilers.TryGetValue(id, out var profiler);
        return profiler;
    }

    /// <summary>
    /// 保存 profiler
    /// </summary>
    public void Save(StackExchange.Profiling.MiniProfiler profiler)
    {
        if (profiler.Id != Guid.Empty)
        {
            _profilers.AddOrUpdate(profiler.Id, profiler, (k, v) => profiler);

            // 如果有用户信息，标记为未查看
            if (!string.IsNullOrEmpty(profiler.User))
            {
                var userUnviewed = _unviewedProfilers.GetOrAdd(profiler.User, _ => new HashSet<Guid>());
                lock (userUnviewed)
                {
                    userUnviewed.Add(profiler.Id);
                }
            }

            // 每 100 次保存或每分钟清理一次（避免频繁清理）
            if (_profilers.Count > 100/* || DateTime.UtcNow - _lastCleanup > TimeSpan.FromMinutes(1)*/)
            {
                CleanUpIfNeeded();
            }
        }
    }

    /// <summary>
    /// 标记为未查看
    /// </summary>
    public void SetUnviewed(string user, Guid id)
    {
        if (string.IsNullOrEmpty(user)) return;

        var userUnviewed = _unviewedProfilers.GetOrAdd(user, _ => new HashSet<Guid>());
        lock (userUnviewed)
        {
            userUnviewed.Add(id);
        }

        // 从已查看列表中移除
        if (_viewedProfilers.TryGetValue(user, out var viewed))
        {
            lock (viewed)
            {
                viewed.Remove(id);
            }
        }
    }

    /// <summary>
    /// 标记为已查看
    /// </summary>
    public void SetViewed(string user, Guid id)
    {
        if (string.IsNullOrEmpty(user)) return;

        var userViewed = _viewedProfilers.GetOrAdd(user, _ => new HashSet<Guid>());
        lock (userViewed)
        {
            userViewed.Add(id);
        }

        // 从未查看列表中移除
        if (_unviewedProfilers.TryGetValue(user, out var unviewed))
        {
            lock (unviewed)
            {
                unviewed.Remove(id);
            }
        }
    }

    /// <summary>
    /// 获取未查看的 profiler ID 列表
    /// </summary>
    public IEnumerable<Guid> GetUnviewedIds(string user)
    {
        if (string.IsNullOrEmpty(user) || !_unviewedProfilers.TryGetValue(user, out var unviewed))
        {
            return Enumerable.Empty<Guid>();
        }

        // 过滤掉已过期的 profiler
        var cutoff = DateTime.UtcNow.Subtract(_duration);
        List<Guid> result;
        lock (unviewed)
        {
            result = unviewed.Where(id =>
                _profilers.TryGetValue(id, out var profiler) &&
                profiler.Started >= cutoff
            ).ToList();
        }
        return result;
    }

    /// <summary>
    /// 获取指定时间范围内的 profiler 列表（旧方法兼容）
    /// </summary>
    public IEnumerable<Guid> List(int maxResults, DateTime? start = null, DateTime? finish = null, int? resultsPerPage = null)
    {
        return List(maxResults, start, finish, ListResultsOrder.Descending);
    }

    /// <summary>
    /// 获取未查看的 profiler ID 列表（IStorage 接口）
    /// </summary>
    List<Guid> IAsyncStorage.GetUnviewedIds(string? user)
    {
        return GetUnviewedIds(user ?? string.Empty).ToList();
    }

    /// <summary>
    /// 异步获取 profiler 列表
    /// </summary>
    public Task<IEnumerable<Guid>> ListAsync(int maxResults, DateTime? start = null, DateTime? finish = null, ListResultsOrder orderBy = ListResultsOrder.Descending)
    {
        return Task.FromResult(List(maxResults, start, finish, orderBy));
    }

    /// <summary>
    /// 异步保存 profiler
    /// </summary>
    public Task SaveAsync(StackExchange.Profiling.MiniProfiler profiler)
    {
        Save(profiler);
        return Task.CompletedTask;
    }

    /// <summary>
    /// 异步加载 profiler
    /// </summary>
    public Task<StackExchange.Profiling.MiniProfiler?> LoadAsync(Guid id)
    {
        return Task.FromResult(Load(id));
    }

    /// <summary>
    /// 异步标记为未查看
    /// </summary>
    public Task SetUnviewedAsync(string? user, Guid id)
    {
        SetUnviewed(user ?? string.Empty, id);
        return Task.CompletedTask;
    }

    /// <summary>
    /// 异步标记为已查看
    /// </summary>
    public Task SetViewedAsync(string? user, Guid id)
    {
        SetViewed(user ?? string.Empty, id);
        return Task.CompletedTask;
    }

    /// <summary>
    /// 异步获取未查看的 profiler ID 列表
    /// </summary>
    public Task<List<Guid>> GetUnviewedIdsAsync(string? user)
    {
        return Task.FromResult(GetUnviewedIds(user ?? string.Empty).ToList());
    }

    /// <summary>
    /// 按需清理过期数据
    /// </summary>
    private void CleanUpIfNeeded()
    {
        // 使用锁避免并发清理
        if (Monitor.TryEnter(_cleanupLock))
        {
            try
            {
                // 再次检查，避免重复清理
                if (DateTime.UtcNow - _lastCleanup < TimeSpan.FromSeconds(30))
                {
                    return;
                }

                var cutoff = DateTime.UtcNow.Subtract(_duration);

                // 清理过期的 profiler - 使用 ToList 避免枚举时修改
                var expiredKeys = _profilers.Where(kvp => kvp.Value.Started < cutoff)
                                           .Select(kvp => kvp.Key)
                                           .ToList();
                foreach (var key in expiredKeys)
                {
                    _profilers.TryRemove(key, out _);
                }

                // 清理过期的未查看记录
                foreach (var pair in _unviewedProfilers)
                {
                    lock (pair.Value)
                    {
                        var expiredIds = pair.Value.Where(id =>
                            !_profilers.TryGetValue(id, out var profiler) ||
                            profiler.Started < cutoff
                        ).ToList();

                        foreach (var id in expiredIds)
                        {
                            pair.Value.Remove(id);
                        }
                    }
                }

                // 清理过期的已查看记录
                foreach (var pair in _viewedProfilers)
                {
                    lock (pair.Value)
                    {
                        var expiredIds = pair.Value.Where(id =>
                            !_profilers.TryGetValue(id, out var profiler) ||
                            profiler.Started < cutoff
                        ).ToList();

                        foreach (var id in expiredIds)
                        {
                            pair.Value.Remove(id);
                        }
                    }
                }

                _lastCleanup = DateTime.UtcNow;
            }
            finally
            {
                Monitor.Exit(_cleanupLock);
            }
        }
    }
}
