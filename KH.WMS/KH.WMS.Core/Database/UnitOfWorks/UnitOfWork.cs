
using Microsoft.Extensions.Logging;
using KH.WMS.Core.Database.Repositories;
using KH.WMS.Core.DependencyInjection.ServiceLifetimes;

namespace KH.WMS.Core.Database.UnitOfWorks;

/// <summary>
/// 工作单元实现（面向接口，支持事务嵌套）
/// </summary>
[RegisteredService(Lifetime = Microsoft.Extensions.DependencyInjection.ServiceLifetime.Scoped, WithoutInterceptor = true)]
public class UnitOfWork : IUnitOfWork
{
    private readonly IDbContext _dbContext;
    private readonly ILogger<UnitOfWork> _logger;
    private bool _disposed;

    public UnitOfWork(IDbContext dbContext, ILogger<UnitOfWork> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// 获取数据库上下文
    /// </summary>
    public IDbContext DbContext => _dbContext;

    /// <summary>
    /// 是否在事务中
    /// </summary>
    public bool HasActiveTransaction => _dbContext.HasActiveTransaction;

    /// <summary>
    /// 获取事务深度（嵌套层数）
    /// </summary>
    public int TransactionDepth => _dbContext.TransactionDepth;

    /// <summary>
    /// 开始事务
    /// </summary>
    public async Task BeginTransactionAsync(System.Data.IsolationLevel isolationLevel = System.Data.IsolationLevel.ReadCommitted)
    {
        await _dbContext.BeginTransactionAsync(isolationLevel);
        _logger.LogDebug("工作单元事务已开启，深度: {Depth}", TransactionDepth);
    }

    /// <summary>
    /// 提交事务
    /// </summary>
    public async Task CommitAsync()
    {
        await _dbContext.CommitTransactionAsync();
        _logger.LogDebug("工作单元事务已提交，深度: {Depth}", TransactionDepth);
    }

    /// <summary>
    /// 回滚事务
    /// </summary>
    public async Task RollbackAsync()
    {
        await _dbContext.RollbackTransactionAsync();
        _logger.LogDebug("工作单元事务已回滚，深度: {Depth}", TransactionDepth);
    }

    /// <summary>
    /// 获取仓储
    /// </summary>
    public IRepository<T, TKey> GetRepository<T, TKey>() where T : class, new() where TKey : struct
    {
        return _dbContext.GetRepository<T, TKey>();
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _dbContext?.Dispose();
            _disposed = true;
        }
    }
}

