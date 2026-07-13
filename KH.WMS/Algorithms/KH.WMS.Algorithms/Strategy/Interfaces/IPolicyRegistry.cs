using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KH.WMS.Algorithms.Strategy.Enums;

namespace KH.WMS.Algorithms.Strategy.Interfaces
{
    /// <summary>
    /// 策略注册接口
    /// </summary>
    public interface IPolicyRegistry
    {
        /// <summary>
        /// 注册策略
        /// </summary>
        void RegisterStrategy(IPolicyStrategy strategy);

        /// <summary>
        /// 注册过滤器
        /// </summary>
        void RegisterFilter(IPolicyFilter filter);

        /// <summary>
        /// 注册策略链
        /// </summary>
        void RegisterChain(IPolicyChain chain);

        /// <summary>
        /// 获取指定类型的所有策略
        /// </summary>
        IEnumerable<IPolicyStrategy> GetStrategies(PolicyType policyType);

        /// <summary>
        /// 获取所有过滤器
        /// </summary>
        IEnumerable<IPolicyFilter> GetFilters();

        /// <summary>
        /// 根据编码获取策略
        /// </summary>
        IPolicyStrategy? GetStrategy(string code);

        /// <summary>
        /// 根据编码获取策略链
        /// </summary>
        IPolicyChain? GetChain(string chainCode);

        /// <summary>
        /// 根据名称获取过滤器
        /// </summary>
        IPolicyFilter? GetFilter(string name);

        /// <summary>
        /// 创建策略链
        /// </summary>
        IPolicyChain CreateChain(string chainCode, PolicyType policyType);
    }
}
