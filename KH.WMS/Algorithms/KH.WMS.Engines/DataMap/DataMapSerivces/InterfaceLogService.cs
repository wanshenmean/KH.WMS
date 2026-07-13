using KH.WMS.Core.Database.Repositories;
using KH.WMS.Core.DependencyInjection.ServiceLifetimes;
using Microsoft.Extensions.DependencyInjection;
using SqlSugar;

namespace KH.WMS.Engines.DataMap.DataMapInterface;

/// <summary>
/// 接口调用日志服务实现
/// </summary>
[RegisteredService(Lifetime = ServiceLifetime.Scoped)]
public class InterfaceLogService : IInterfaceLogService
{
    private readonly IRepository<IntInterfaceLog, long> _repository;
    private readonly ISqlSugarClient _db;
    public InterfaceLogService(IRepository<IntInterfaceLog, long> repository, ISqlSugarClient db)
    {
        _repository = repository;
        _db = db;

    }

    public async Task<long> LogAsync(IntInterfaceLog log)
    {
        log.CreatedTime = DateTime.Now;

        return await _repository.AddAsync(log);
    }

    public async Task<List<IntInterfaceLog>> GetListAsync(string? interfaceCode = null, string? status = null, int pageIndex = 1, int pageSize = 20)
    {
        var query = _db.Queryable<IntInterfaceLog>();

        if (!string.IsNullOrEmpty(interfaceCode))
        {
            query = query.Where(x => x.InterfaceCode == interfaceCode);
        }

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(x => x.Status == status);
        }

        return await query
            .OrderByDescending(x => x.CreatedTime)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<List<IntInterfaceLog>> GetByTraceIdAsync(string traceId)
    {
        return await _db.Queryable<IntInterfaceLog>()
            .Where(x => x.TraceId == traceId)
            .OrderByDescending(x => x.CreatedTime)
            .ToListAsync();
    }

    public async Task<IntInterfaceLog?> GetByIdAsync(long id)
    {
        return await _db.Queryable<IntInterfaceLog>()
            .Where(x => x.Id == id )
            .FirstAsync();
    }

    public async Task<object> GetStatisticsAsync(string? interfaceCode = null, DateTime? startTime = null, DateTime? endTime = null)
    {
        var query = _db.Queryable<IntInterfaceLog>();

        if (!string.IsNullOrEmpty(interfaceCode))
        {
            query = query.Where(x => x.InterfaceCode == interfaceCode);
        }

        if (startTime.HasValue)
        {
            query = query.Where(x => x.CreatedTime >= startTime.Value);
        }

        if (endTime.HasValue)
        {
            query = query.Where(x => x.CreatedTime <= endTime.Value);
        }

        var totalCount = await query.CountAsync();
        var successCount = await query.Where(x => x.Status == DataMapConstants.CallLogStatus.SUCCESS).CountAsync();
        var failCount = await query.Where(x => x.Status == DataMapConstants.CallLogStatus.FAIL).CountAsync();
        var timeoutCount = await query.Where(x => x.Status == DataMapConstants.CallLogStatus.TIMEOUT).CountAsync();

        var avgExecutionTime = await query
            .Where(x => x.ExecuteMilliseconds.HasValue)
            .AvgAsync(x => x.ExecuteMilliseconds);

        // 按接口统计
        var byInterface = await query
            .GroupBy(x => x.InterfaceCode)
            .Select(x => new
            {
                x.InterfaceCode,
                TotalCount = SqlFunc.AggregateCount(x.Id),
                SuccessCount = SqlFunc.AggregateSum(SqlFunc.IF(x.Status == DataMapConstants.CallLogStatus.SUCCESS).Return(1).End(0)),
                FailCount = SqlFunc.AggregateSum(SqlFunc.IF(x.Status == DataMapConstants.CallLogStatus.FAIL).Return(1).End(0)),
                AvgExecutionTime = SqlFunc.AggregateAvg(x.ExecuteMilliseconds)
            })
            .ToListAsync();

        return new
        {
            TotalCount = totalCount,
            SuccessCount = successCount,
            FailCount = failCount,
            TimeoutCount = timeoutCount,
            SuccessRate = totalCount > 0 ? Math.Round((double)successCount / totalCount * 100, 2) : 0,
            AvgExecutionTime = avgExecutionTime.HasValue ? Math.Round((double)avgExecutionTime.Value, 2) : 0,
            ByInterface = byInterface
        };
    }

    public async Task<bool> CleanExpiredLogsAsync(int daysToKeep = 90)
    {
        var expireDate = DateTime.Now.AddDays(-daysToKeep);

        var result = await _db.Deleteable<IntInterfaceLog>()
            .Where(x => x.CreatedTime < expireDate)
            .ExecuteCommandAsync();

        return result > 0;
    }
}
