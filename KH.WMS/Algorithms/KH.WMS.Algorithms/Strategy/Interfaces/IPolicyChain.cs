using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KH.WMS.Algorithms.Strategy.Interfaces
{
    /// <summary>
    /// 策略链接口
    /// </summary>
    public interface IPolicyChain
    {
        /// <summary>
        /// 策略链名称
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 策略链编码
        /// </summary>
        string Code { get; }

        /// <summary>
        /// 流水线模式（true=每个策略从上游结果中筛选，不中断；false=第一个成功即停）
        /// </summary>
        bool PipelineMode { get; set; }

        /// <summary>
        /// 添加策略到链（无参数）
        /// </summary>
        IPolicyChain AddStrategy(IPolicyStrategy strategy);

        /// <summary>
        /// 添加策略到链（带步骤级参数）
        /// </summary>
        /// <param name="strategy">策略实例</param>
        /// <param name="stepParams">步骤参数（JSON，优先级高，可覆盖策略默认参数）</param>
        /// <param name="strategyParams">策略默认参数（JSON，作为回退）</param>
        IPolicyChain AddStrategy(IPolicyStrategy strategy, string? stepParams, string? strategyParams);

        /// <summary>
        /// 添加过滤器到链
        /// </summary>
        IPolicyChain AddFilter(IPolicyFilter filter);

        /// <summary>
        /// 执行策略链
        /// </summary>
        Task<IPolicyResult> ExecuteAsync(IPolicyContext context, CancellationToken cancellationToken = default);
    }
}
