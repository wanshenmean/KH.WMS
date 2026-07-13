using KH.WMS.Core.Database.UnitOfWorks;
using KH.WMS.Core.Filters.Action;
using KH.WMS.Core.Logging;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace KH.WMS.Core.Attributes;

/// <summary>
/// 事务特性 - 标记控制器或 Action 需要自动事务管理
/// 实现 IFilterFactory，由 MVC 管道自动创建 TransactionActionFilter
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
public class TransactionAttribute : Attribute, IFilterFactory
{
    /// <summary>
    /// 事务隔离级别
    /// </summary>
    public System.Data.IsolationLevel IsolationLevel { get; set; } = System.Data.IsolationLevel.ReadCommitted;

    /// <summary>
    /// 事务超时时间（秒）
    /// </summary>
    public int Timeout { get; set; } = 30;

    public bool IsReusable => false;

    public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
    {
        var unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();
        var logger = serviceProvider.GetRequiredService<ILoggerService>();
        return new TransactionActionFilter(unitOfWork, logger, IsolationLevel);
    }
}
