using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KH.WMS.Algorithms.Strategy.Enums;

namespace KH.WMS.Algorithms.Strategy.Interfaces
{
    /// <summary>
    /// 策略引擎接口
    /// </summary>
    public interface IPolicyEngine
    {
        /// <summary>
        /// 执行指定类型的策略
        /// </summary>
        Task<IPolicyResult> ExecuteAsync(PolicyType policyType, IPolicyContext context, CancellationToken cancellationToken = default);

        /// <summary>
        /// 执行指定策略
        /// </summary>
        Task<IPolicyResult> ExecuteAsync(string policyCode, IPolicyContext context, CancellationToken cancellationToken = default);

        /// <summary>
        /// 执行策略链
        /// </summary>
        Task<IPolicyResult> ExecuteChainAsync(string chainCode, IPolicyContext context, CancellationToken cancellationToken = default);
    }
}
