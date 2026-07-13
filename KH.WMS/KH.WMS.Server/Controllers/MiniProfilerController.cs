using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Profiling;
using StackExchange.Profiling.Storage;

namespace KH.WMS.Server.Controllers;

/// <summary>
/// MiniProfiler 查询控制器
/// </summary>
[Route("api/[controller]")]
[ApiController]
[AllowAnonymous]
public class MiniProfilerController : ControllerBase
{
    private readonly IAsyncStorage _storage;

    public MiniProfilerController(IAsyncStorage storage)
    {
        _storage = storage;
    }

    /// <summary>
    /// 获取最近的性能分析记录列表
    /// </summary>
    /// <param name="maxResults">最大返回数量，默认 20</param>
    /// <param name="start">开始时间（可选）</param>
    /// <param name="finish">结束时间（可选）</param>
    /// <returns>性能分析记录 ID 列表</returns>
    [HttpGet("list")]
    public async Task<IActionResult> GetList(
        [FromQuery] int maxResults = 20,
        [FromQuery] DateTime? start = null,
        [FromQuery] DateTime? finish = null)
    {
        var ids = await _storage.ListAsync(maxResults, start, finish, ListResultsOrder.Descending);
        return Ok(new
        {
            count = ids.Count(),
            ids
        });
    }

    /// <summary>
    /// 获取指定 ID 的性能分析详情
    /// </summary>
    /// <param name="id">性能分析记录 ID</param>
    /// <returns>性能分析详情</returns>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var profiler = await _storage.LoadAsync(id);

        if (profiler == null)
        {
            return NotFound(new { message = $"未找到 ID 为 {id} 的性能分析记录" });
        }

        return Ok(new
        {
            id = profiler.Id,
            name = profiler.Name,
            started = profiler.Started,
            durationMilliseconds = profiler.DurationMilliseconds,
            user = profiler.User,
            machineName = profiler.MachineName,
            customLinks = profiler.CustomLinks
        });
    }

    /// <summary>
    /// 获取指定用户未查看的性能分析记录
    /// </summary>
    /// <param name="user">用户名</param>
    /// <returns>未查看的记录 ID 列表</returns>
    [HttpGet("unviewed")]
    public async Task<IActionResult> GetUnviewed([FromQuery] string? user = null)
    {
        // 如果没有指定用户，使用当前用户（如果已认证）
        user ??= User.Identity?.Name ?? "anonymous";

        var ids = await _storage.GetUnviewedIdsAsync(user);
        return Ok(new
        {
            user,
            count = ids.Count,
            ids
        });
    }

    /// <summary>
    /// 标记记录为已查看
    /// </summary>
    /// <param name="id">性能分析记录 ID</param>
    /// <param name="user">用户名（可选，默认使用当前用户）</param>
    [HttpPost("{id:guid}/view")]
    public async Task<IActionResult> MarkAsViewed(Guid id, [FromQuery] string? user = null)
    {
        user ??= User.Identity?.Name ?? "anonymous";
        await _storage.SetViewedAsync(user, id);
        return Ok(new { message = $"已标记记录 {id} 为已查看" });
    }

    /// <summary>
    /// 获取性能统计摘要
    /// </summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var ids = await _storage.ListAsync(100);
        var profilers = new List<StackExchange.Profiling.MiniProfiler>();

        foreach (var id in ids)
        {
            var profiler = await _storage.LoadAsync(id);
            if (profiler != null)
            {
                profilers.Add(profiler);
            }
        }

        if (profilers.Count == 0)
        {
            return Ok(new { message = "暂无性能分析记录" });
        }

        return Ok(new
        {
            totalRequests = profilers.Count,
            avgDuration = profilers.Average(p => p.DurationMilliseconds),
            maxDuration = profilers.Max(p => p.DurationMilliseconds),
            minDuration = profilers.Min(p => p.DurationMilliseconds),
            recent = profilers.Take(5).Select(p => new
            {
                p.Id,
                p.Name,
                p.Started,
                p.DurationMilliseconds,
                user = p.User ?? "anonymous"
            })
        });
    }
}
