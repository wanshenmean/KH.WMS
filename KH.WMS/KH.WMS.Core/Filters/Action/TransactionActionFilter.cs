using System.Data;
using KH.WMS.Core.Database.UnitOfWorks;
using KH.WMS.Core.Logging;
using Microsoft.AspNetCore.Mvc.Filters;

namespace KH.WMS.Core.Filters.Action;

/// <summary>
/// 事务 Action Filter — 由 [Transaction] 特性通过 IFilterFactory 创建
/// 自动管理请求级别的事务（Begin / Commit / Rollback）
/// </summary>
public class TransactionActionFilter : IAsyncActionFilter
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILoggerService _logger;
    private readonly IsolationLevel _isolationLevel;

    public TransactionActionFilter(IUnitOfWork unitOfWork, ILoggerService logger, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _isolationLevel = isolationLevel;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var actionName = $"{context.RouteData.Values["controller"]}/{context.RouteData.Values["action"]}";

        // 如果已经在一个事务中（Service 层手动开启），直接执行
        if (_unitOfWork.HasActiveTransaction)
        {
            _logger.LogDebug("已在事务中，跳过自动事务管理: {Action}", actionName);
            await next();
            return;
        }

        _logger.LogDebug("开始事务: {Action}, 隔离级别: {Level}", actionName, _isolationLevel);

        await _unitOfWork.BeginTransactionAsync(_isolationLevel);

        var executedContext = await next();

        // 只要 Action 执行过程中抛出过异常，无论是否已被全局异常过滤器标记为已处理（ExceptionHandled），
        // 业务都未成功完成，事务必须回滚；否则会把半成功的多步写一并提交，造成脏数据。
        if (executedContext.Exception == null)
        {
            await _unitOfWork.CommitAsync();
            _logger.LogDebug("事务提交成功: {Action}", actionName);
        }
        else
        {
            await _unitOfWork.RollbackAsync();
            _logger.LogDebug("事务已回滚（异常已处理: {Handled}）: {Action}", executedContext.ExceptionHandled, actionName);
        }
    }
}
