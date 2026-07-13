using System.Linq.Expressions;
using System.Reflection;
using KH.WMS.Core.Database.SqlSugar;
using KH.WMS.Core.Attributes;
using KH.WMS.Core.DependencyInjection.ServiceLifetimes;
using Microsoft.Extensions.Logging;
using SqlSugar;

namespace KH.WMS.Core.Database.Repositories;

/// <summary>
/// 仓储基类实现
/// </summary>
/// <typeparam name="T">实体类型</typeparam>
[RegisteredService, LogInterceptor(LogParameters = true, LogReturnValue = true, LogLevel = Microsoft.Extensions.Logging.LogLevel.Information)]
public class RepositoryBase<T, TKey> : IRepository<T, TKey>
    where T : class, new()
    where TKey : struct
{
    protected readonly ISqlSugarClient _db;
    protected readonly ILogger? _logger;

    public RepositoryBase(ISqlSugarClient db, ILogger? logger = null)
    {
        // 配置实体自动路由到配置库（SQLite）
        _db = typeof(T).GetCustomAttribute<ConfigDbAttribute>() != null
            ? ((SqlSugarClient)db).GetConnection(SqlSugarSetup.ConfigDb)
            : db;
        _logger = logger;
    }

    public async Task<T?> GetByIdAsync(TKey id)
    {
        return await _db.Queryable<T>().In(id).FirstAsync();
    }

    public async Task<List<T>> GetAllAsync()
    {
        return await _db.Queryable<T>().ToListAsync();
    }

    public async Task<List<T>> GetListAsync(System.Linq.Expressions.Expression<Func<T, bool>> expression)
    {
        return await _db.Queryable<T>().Where(expression).ToListAsync();
    }

    public async Task<T?> GetFirstOrDefaultAsync(System.Linq.Expressions.Expression<Func<T, bool>> expression)
    {
        return await _db.Queryable<T>().FirstAsync(expression);
    }

    public async Task<(List<T> Items, int Total)> GetPagedListAsync(int pageIndex, int pageSize, System.Linq.Expressions.Expression<Func<T, bool>>? expression = null)
    {
        var query = _db.Queryable<T>();

        if (expression != null)
        {
            query = query.Where(expression);
        }

        var total = await query.CountAsync();
        var items = await query.ToPageListAsync(pageIndex, pageSize);

        return (items, total);
    }

    public async Task<TKey> AddAsync(T entity)
    {
        return (await _db.Insertable(entity).ExecuteReturnPkListAsync<TKey>()).First();
    }

    public async Task<List<TKey>> AddAsync(List<T> entities)
    {
        return await _db.Insertable(entities).ExecuteReturnPkListAsync<TKey>();
    }

    public async Task<T> AddReturnEntityAsync(T entity)
    {
        return await _db.Insertable(entity).ExecuteReturnEntityAsync();
    }

    public async Task<List<T>> AddReturnEntityAsync(List<T> entities)
    {
        List<TKey> keys = await _db.Insertable(entities).ExecuteReturnPkListAsync<TKey>();
        return await _db.Queryable<T>().In(keys).ToListAsync();
    }

    public async Task<bool> UpdateAsync(T entity)
    {
        return await _db.Updateable(entity).ExecuteCommandAsync() > 0;
    }

    public async Task<bool> UpdateAsync(List<T> entities)
    {
        return await _db.Updateable(entities).ExecuteCommandAsync() > 0;
    }

    public async Task<bool> DeleteAsync(TKey id)
    {
        return await _db.Deleteable<T>().In(id).ExecuteCommandAsync() > 0;
    }

    public async Task<bool> DeleteAsync(List<TKey> ids)
    {
        return await _db.Deleteable<T>().In(ids).ExecuteCommandAsync() > 0;
    }

    public async Task<bool> DeleteAsync(System.Linq.Expressions.Expression<Func<T, bool>> expression)
    {
        return await _db.Deleteable<T>().Where(expression).ExecuteCommandAsync() > 0;
    }

    public async Task<bool> ExistsAsync(System.Linq.Expressions.Expression<Func<T, bool>> expression)
    {
        return await _db.Queryable<T>().Where(expression).AnyAsync();
    }

    public async Task<int> CountAsync(System.Linq.Expressions.Expression<Func<T, bool>>? expression = null)
    {
        var query = _db.Queryable<T>();

        if (expression != null)
        {
            query = query.Where(expression);
        }

        return await query.CountAsync();
    }

    public async Task<bool> AddWithNavAsync(T entity)
    {
        return await _db.InsertNav(entity).IncludesAllFirstLayer().ExecuteCommandAsync();
    }

    public async Task<bool> UpdateWithNavAsync(T entity)
    {
        return await _db.UpdateNav(entity).IncludesAllFirstLayer().ExecuteCommandAsync();
    }

    public async Task<bool> DeleteWithNavAsync(T entity)
    {
        return await _db.DeleteNav(entity).IncludesAllFirstLayer().ExecuteCommandAsync();
    }

    public async Task<T?> GetByIdWithNavAsync(TKey id)
    {
        return await _db.Queryable<T>()
            .IncludesAllFirstLayer()
            .In(id)
            .FirstAsync();
    }

    public async Task<List<T>> GetAllWithNavAsync()
    {
        return await _db.Queryable<T>()
            .IncludesAllFirstLayer()
            .ToListAsync();
    }

    public async Task<List<T>> GetListWithNavAsync(System.Linq.Expressions.Expression<Func<T, bool>> expression)
    {
        return await _db.Queryable<T>()
            .IncludesAllFirstLayer()
            .Where(expression)
            .ToListAsync();
    }

    public async Task<T?> GetFirstOrDefaultWithNavAsync(System.Linq.Expressions.Expression<Func<T, bool>> expression)
    {
        return await _db.Queryable<T>()
            .IncludesAllFirstLayer()
            .FirstAsync(expression);
    }

    // 查出所有行中包含物料ID=123的入库单（主表+从表一起返回）
    //var orders = await _repository.GetListByDetailAsync<InboundOrderLine>(
    //    x => x.OrderLines,
    //    d => d.MaterialId == 123
    //);

    // 查出第一个行中包含物料编码="M001"的入库单
    //var order = await _repository.GetFirstOrDefaultByDetailAsync<InboundOrderLine>(
    //    x => x.OrderLines,
    //    d => d.MaterialCode == "M001"
    //);

    public async Task<List<T>> GetListByDetailAsync<TDetail>(
        Expression<Func<T, IEnumerable<TDetail>>> navProperty,
        Expression<Func<TDetail, bool>> detailFilter)
        where TDetail : class, new()
    {
        var where = BuildAnyExpression(navProperty, detailFilter);
        return await _db.Queryable<T>()
            .IncludesAllFirstLayer()
            .Where(where)
            .ToListAsync();
    }

    public async Task<T?> GetFirstOrDefaultByDetailAsync<TDetail>(
        Expression<Func<T, IEnumerable<TDetail>>> navProperty,
        Expression<Func<TDetail, bool>> detailFilter)
        where TDetail : class, new()
    {
        var where = BuildAnyExpression(navProperty, detailFilter);
        return await _db.Queryable<T>()
            .IncludesAllFirstLayer()
            .FirstAsync(where);
    }

    /// <summary>
    /// 构建 x => x.NavProperty.Any(detailFilter) 表达式树
    /// </summary>
    private static Expression<Func<T, bool>> BuildAnyExpression<TDetail>(
        Expression<Func<T, IEnumerable<TDetail>>> navProperty,
        Expression<Func<TDetail, bool>> detailFilter)
        where TDetail : class, new()
    {
        var param = Expression.Parameter(typeof(T), "x");
        var navMember = Expression.Property(param, ((MemberExpression)navProperty.Body).Member.Name);

        var anyMethod = typeof(Enumerable).GetMethods()
            .First(m => m.Name == "Any" && m.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(TDetail));

        var anyCall = Expression.Call(anyMethod, navMember, detailFilter);
        return Expression.Lambda<Func<T, bool>>(anyCall, param);
    }

    /// <inheritdoc />
    public ISugarQueryable<T> AsQueryable()
    {
        return _db.Queryable<T>();
    }
}
