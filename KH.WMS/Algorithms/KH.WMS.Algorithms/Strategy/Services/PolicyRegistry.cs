using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KH.WMS.Algorithms.Strategy.Enums;
using KH.WMS.Algorithms.Strategy.Interfaces;
using KH.WMS.Core.DependencyInjection.ServiceLifetimes;
using Microsoft.Extensions.DependencyInjection;

namespace KH.WMS.Algorithms.Strategy.Services
{
    /// <summary>
    /// 策略注册中心实现
    /// </summary>
    [RegisteredService(Lifetime = ServiceLifetime.Singleton, ServiceType = typeof(IPolicyRegistry))]
    public class PolicyRegistry : IPolicyRegistry
    {
        private readonly Dictionary<PolicyType, List<IPolicyStrategy>> _strategies = new();
        private readonly List<IPolicyFilter> _filters = new();
        private readonly Dictionary<string, IPolicyStrategy> _strategyIndex = new();
        private readonly Dictionary<string, IPolicyFilter> _filterIndex = new();
        private readonly Dictionary<string, IPolicyChain> _chainIndex = new();

        public void RegisterStrategy(IPolicyStrategy strategy)
        {
            if (!_strategies.ContainsKey(strategy.PolicyType))
            {
                _strategies[strategy.PolicyType] = new List<IPolicyStrategy>();
            }

            _strategies[strategy.PolicyType].Add(strategy);
            _strategyIndex[strategy.Code] = strategy;
        }

        public void RegisterFilter(IPolicyFilter filter)
        {
            _filters.Add(filter);
            _filterIndex[filter.Name] = filter;
        }

        public void RegisterChain(IPolicyChain chain)
        {
            _chainIndex[chain.Code] = chain;
        }

        public IEnumerable<IPolicyStrategy> GetStrategies(PolicyType policyType)
        {
            return _strategies.TryGetValue(policyType, out var strategies)
                ? strategies
                : Enumerable.Empty<IPolicyStrategy>();
        }

        public IEnumerable<IPolicyFilter> GetFilters()
        {
            return _filters;
        }

        public IPolicyStrategy? GetStrategy(string code)
        {
            return _strategyIndex.TryGetValue(code, out var strategy) ? strategy : null;
        }

        public IPolicyChain? GetChain(string chainCode)
        {
            return _chainIndex.TryGetValue(chainCode, out var chain) ? chain : null;
        }

        public IPolicyFilter? GetFilter(string name)
        {
            return _filterIndex.TryGetValue(name, out var filter) ? filter : null;
        }

        public IPolicyChain CreateChain(string chainCode, PolicyType policyType)
        {
            var chain = new PolicyChain(chainCode, chainCode);

            // #17：货位分配回退链也用流水线模式（每个策略从上游结果筛选），与 DB 配置链行为一致
            if (policyType == PolicyType.LocationAllocation)
                chain.PipelineMode = true;

            // 添加所有过滤器
            foreach (var filter in _filters.OrderBy(f => f.Order))
            {
                chain.AddFilter(filter);
            }

            // 添加指定类型的所有策略
            var strategies = GetStrategies(policyType);
            foreach (var strategy in strategies)
            {
                chain.AddStrategy(strategy);
            }

            return chain;
        }
    }
}
