using KH.WMS.Core.Database.Repositories;
using SqlSugar;

namespace KH.WMS.Core.Database;

/// <summary>
/// 数据库上下文接口
/// </summary>
public interface IDbContext : IDisposable
{
    /// <summary>
    /// 数据库操作对象
    /// </summary>
    ISqlSugarClient Db { get; }

    /// <summary>
    /// 开始事务
    /// </summary>
    Task BeginTransactionAsync(System.Data.IsolationLevel isolationLevel = System.Data.IsolationLevel.ReadCommitted);

    /// <summary>
    /// 提交事务
    /// </summary>
    Task CommitTransactionAsync();

    /// <summary>
    /// 回滚事务
    /// </summary>
    Task RollbackTransactionAsync();

    /// <summary>
    /// 是否在事务中
    /// </summary>
    bool HasActiveTransaction { get; }

    /// <summary>
    /// 获取事务深度（嵌套层数）
    /// </summary>
    int TransactionDepth { get; }

    /// <summary>
    /// 获取当前事务隔离级别
    /// </summary>
    System.Data.IsolationLevel? CurrentIsolationLevel { get; }

    /// <summary>
    /// 获取仓储
    /// </summary>
    IRepository<T, TKey> GetRepository<T, TKey>()
        where T : class, new()
        where TKey : struct;
}


