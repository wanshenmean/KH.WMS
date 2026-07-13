using Microsoft.Extensions.Logging;

namespace KH.WMS.Core.Database.UnitOfWorks;

/// <summary>
/// UnitOfWork 扩展方法
/// </summary>
public static class UnitOfWorkExtensions
{
    /// <summary>
    /// 创建事务作用域，自动管理事务生命周期
    /// </summary>
    /// <param name="unitOfWork">工作单元</param>
    /// <param name="logger">日志记录器</param>
    /// <param name="isolationLevel">事务隔离级别</param>
    /// <returns>事务作用域</returns>
    public static async Task<ITransactionScope> BeginTransactionScopeAsync(
        this IUnitOfWork unitOfWork,
        ILogger logger,
        System.Data.IsolationLevel isolationLevel = System.Data.IsolationLevel.ReadCommitted)
    {
        await unitOfWork.BeginTransactionAsync(isolationLevel);
        return new TransactionScopeWrapper(unitOfWork, logger);
    }

    /// <summary>
    /// 在事务作用域中执行操作
    /// </summary>
    /// <param name="unitOfWork">工作单元</param>
    /// <param name="logger">日志记录器</param>
    /// <param name="action">要执行的操作</param>
    /// <param name="isolationLevel">事务隔离级别</param>
    public static async Task ExecuteInTransactionAsync(
        this IUnitOfWork unitOfWork,
        ILogger logger,
        Func<Task> action,
        System.Data.IsolationLevel isolationLevel = System.Data.IsolationLevel.ReadCommitted)
    {
        await using var scope = await unitOfWork.BeginTransactionScopeAsync(logger, isolationLevel);
        try
        {
            await action();
            await scope.CommitAsync();
        }
        catch
        {
            await scope.RollbackAsync();
            throw;
        }
    }

    /// <summary>
    /// 在事务作用域中执行操作（带返回值）
    /// </summary>
    /// <typeparam name="T">返回值类型</typeparam>
    /// <param name="unitOfWork">工作单元</param>
    /// <param name="logger">日志记录器</param>
    /// <param name="action">要执行的操作</param>
    /// <param name="isolationLevel">事务隔离级别</param>
    /// <returns>操作结果</returns>
    public static async Task<T> ExecuteInTransactionAsync<T>(
        this IUnitOfWork unitOfWork,
        ILogger logger,
        Func<Task<T>> action,
        System.Data.IsolationLevel isolationLevel = System.Data.IsolationLevel.ReadCommitted)
    {
        await using var scope = await unitOfWork.BeginTransactionScopeAsync(logger, isolationLevel);
        try
        {
            var result = await action();
            await scope.CommitAsync();
            return result;
        }
        catch
        {
            await scope.RollbackAsync();
            throw;
        }
    }
}

/// <summary>
/// 事务作用域接口
/// </summary>
public interface ITransactionScope : IAsyncDisposable
{
    /// <summary>
    /// 提交事务
    /// </summary>
    Task CommitAsync();

    /// <summary>
    /// 回滚事务
    /// </summary>
    Task RollbackAsync();

    /// <summary>
    /// 事务是否已提交
    /// </summary>
    bool IsCommitted { get; }
}

/// <summary>
/// 事务作用域包装器
/// </summary>
public class TransactionScopeWrapper : ITransactionScope
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _logger;
    private bool _disposed;
    private bool _committed;

    public TransactionScopeWrapper(IUnitOfWork unitOfWork, ILogger logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public bool IsCommitted => _committed;

    public async Task CommitAsync()
    {
        if (!_committed)
        {
            await _unitOfWork.CommitAsync();
            _committed = true;
            _logger.LogDebug("事务作用域已提交，深度: {Depth}", _unitOfWork.TransactionDepth);
        }
    }

    public async Task RollbackAsync()
    {
        if (!_committed)
        {
            await _unitOfWork.RollbackAsync();
            _committed = true;
            _logger.LogDebug("事务作用域已回滚，深度: {Depth}", _unitOfWork.TransactionDepth);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (!_disposed)
        {
            if (!_committed)
            {
                _logger.LogWarning("事务作用域释放时未提交，自动回滚");
                try
                {
                    await _unitOfWork.RollbackAsync();
                }
                catch
                {
                    // 忽略 dispose 时的异常
                }
            }
            _disposed = true;
        }
    }
}
