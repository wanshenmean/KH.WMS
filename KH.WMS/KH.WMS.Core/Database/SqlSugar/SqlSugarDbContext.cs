using KH.WMS.Core.Database.Repositories;
using KH.WMS.Core.DependencyInjection.ServiceLifetimes;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SqlSugar;

namespace KH.WMS.Core.Database.SqlSugar;

/// <summary>
/// SQL Sugar 数据库上下文（支持事务嵌套）
/// </summary>
[RegisteredService(Lifetime = Microsoft.Extensions.DependencyInjection.ServiceLifetime.Scoped, WithoutInterceptor = true)]
public class SqlSugarDbContext : IDbContext
{
    private readonly ISqlSugarClient _sqlSugarClient;
    private readonly ILogger<SqlSugarDbContext> _logger;
    private readonly DatabaseOptions _options;

    // 事务栈，用于支持嵌套事务
    private readonly Stack<TransactionScope> _transactionScopes = new();

    public SqlSugarDbContext(
        ISqlSugarClient sqlSugarClient,
        ILogger<SqlSugarDbContext> logger,
        IOptions<DatabaseOptions> options)
    {
        _sqlSugarClient = sqlSugarClient;
        _logger = logger;
        _options = options.Value;
    }

    /// <summary>
    /// 是否在事务中
    /// </summary>
    public bool HasActiveTransaction => _transactionScopes.Count > 0;

    /// <summary>
    /// 获取事务深度（嵌套层数）
    /// </summary>
    public int TransactionDepth => _transactionScopes.Count;

    /// <summary>
    /// 获取当前事务隔离级别
    /// </summary>
    public System.Data.IsolationLevel? CurrentIsolationLevel =>
        _transactionScopes.Count > 0 ? _transactionScopes.Peek().IsolationLevel : null;

    public ISqlSugarClient Db => _sqlSugarClient;

    public async Task BeginTransactionAsync(System.Data.IsolationLevel isolationLevel = System.Data.IsolationLevel.ReadCommitted)
    {
        var scope = new TransactionScope(isolationLevel);
        _transactionScopes.Push(scope);

        // 只有最外层事务才真正开启数据库事务
        if (_transactionScopes.Count == 1)
        {
            try
            {
                await _sqlSugarClient.Ado.BeginTranAsync(isolationLevel);
                _logger.LogInformation("事务已开启，隔离级别: {Level}，深度: {Depth}", isolationLevel, _transactionScopes.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "开启事务失败");
                _transactionScopes.Pop();
                throw;
            }
        }
        else
        {
            _logger.LogDebug("嵌套事务已开启，隔离级别: {Level}，深度: {Depth}", isolationLevel, _transactionScopes.Count);
        }
    }

    public async Task CommitTransactionAsync()
    {
        if (_transactionScopes.Count == 0)
        {
            _logger.LogWarning("没有活动的事务可以提交");
            return;
        }

        var scope = _transactionScopes.Peek();

        // 检查是否有嵌套事务标记为需要回滚
        if (scope.RequiresRollback)
        {
            _logger.LogWarning("检测到嵌套事务已标记回滚，整个事务将回滚");
            await RollbackTransactionAsync();
            return;
        }

        // 只有最外层事务提交时才真正提交数据库事务
        if (_transactionScopes.Count == 1)
        {
            try
            {
                await _sqlSugarClient.Ado.CommitTranAsync();
                _logger.LogInformation("事务已提交，深度: {Depth}", _transactionScopes.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "提交事务失败");
                throw;
            }
            finally
            {
                _transactionScopes.Pop();
            }
        }
        else
        {
            // 嵌套事务只标记为已完成，并弹出栈
            scope.IsCompleted = true;
            _transactionScopes.Pop();
            _logger.LogDebug("嵌套事务已标记完成，深度: {Depth}", _transactionScopes.Count);
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transactionScopes.Count == 0)
        {
            _logger.LogWarning("没有活动的事务可以回滚");
            return;
        }

        var scope = _transactionScopes.Peek();

        // 只有最外层事务回滚时才真正回滚数据库事务
        if (_transactionScopes.Count == 1)
        {
            try
            {
                await _sqlSugarClient.Ado.RollbackTranAsync();
                _logger.LogInformation("事务已回滚，深度: {Depth}", _transactionScopes.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "回滚事务失败");
                throw;
            }
            finally
            {
                _transactionScopes.Clear();
            }
        }
        else
        {
            // 嵌套事务回滚时，标记为需要回滚
            scope.RequiresRollback = true;
            _logger.LogWarning("嵌套事务已标记回滚，深度: {Depth}", _transactionScopes.Count);

            // 弹出当前事务栈
            _transactionScopes.Pop();

            // 将回滚信号传播到外层事务：外层 Commit 只 Peek 当前栈顶，
            // 若不传播，已被 pop 的内层回滚标记会丢失，外层会误提交（原 #31）。
            if (_transactionScopes.Count > 0)
                _transactionScopes.Peek().RequiresRollback = true;
        }
    }

    public IRepository<T, TKey> GetRepository<T, TKey>() where T : class, new() where TKey : struct
    {
        return new RepositoryBase<T, TKey>(_sqlSugarClient, _logger);
    }

    public void Dispose()
    {
        // 如果有未提交的事务，自动回滚
        if (_transactionScopes.Count > 0)
        {
            _logger.LogWarning("检测到未提交的事务，自动回滚，深度: {Depth}", _transactionScopes.Count);
            try
            {
                if (_transactionScopes.Count == 1)
                {
                    _sqlSugarClient?.Ado.RollbackTran();
                }
            }
            catch
            {
                // 忽略 dispose 时的异常
            }
        }

        _sqlSugarClient?.Dispose();
    }

    /// <summary>
    /// 事务作用域
    /// </summary>
    private class TransactionScope
    {
        public System.Data.IsolationLevel IsolationLevel { get; }
        public bool IsCompleted { get; set; }
        public bool RequiresRollback { get; set; }

        public TransactionScope(System.Data.IsolationLevel isolationLevel)
        {
            IsolationLevel = isolationLevel;
            IsCompleted = false;
            RequiresRollback = false;
        }
    }
}
