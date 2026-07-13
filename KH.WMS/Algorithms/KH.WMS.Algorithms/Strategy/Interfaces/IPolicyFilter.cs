using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KH.WMS.Algorithms.Strategy.Interfaces
{
    /// <summary>
    /// 策略过滤器接口
    /// </summary>
    public interface IPolicyFilter
    {
        /// <summary>
        /// 过滤器顺序（数字越小越先执行）
        /// </summary>
        int Order { get; }

        /// <summary>
        /// 过滤器名称
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 执行前置过滤（策略执行前）
        /// </summary>
        Task<IPolicyContext> OnBeforeExecutionAsync(IPolicyContext context, CancellationToken cancellationToken = default);

        /// <summary>
        /// 执行后置过滤（策略执行后）
        /// </summary>
        Task<IPolicyResult> OnAfterExecutionAsync(IPolicyContext context, IPolicyResult result, CancellationToken cancellationToken = default);
    }
}
