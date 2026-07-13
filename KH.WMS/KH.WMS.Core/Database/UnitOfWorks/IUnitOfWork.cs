using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KH.WMS.Core.Database.Repositories;

namespace KH.WMS.Core.Database.UnitOfWorks
{
    /// <summary>
    /// 工作单元接口
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// 开始事务
        /// </summary>
        Task BeginTransactionAsync(System.Data.IsolationLevel isolationLevel = System.Data.IsolationLevel.ReadCommitted);

        /// <summary>
        /// 提交事务
        /// </summary>
        Task CommitAsync();

        /// <summary>
        /// 回滚事务
        /// </summary>
        Task RollbackAsync();

        /// <summary>
        /// 是否在事务中
        /// </summary>
        bool HasActiveTransaction { get; }

        /// <summary>
        /// 获取事务深度（嵌套层数）
        /// </summary>
        int TransactionDepth { get; }

        /// <summary>
        /// 获取数据库上下文
        /// </summary>
        IDbContext DbContext { get; }

        /// <summary>
        /// 获取仓储
        /// </summary>
        IRepository<T, TKey> GetRepository<T, TKey>() where T : class, new() where TKey : struct;
    }
}
