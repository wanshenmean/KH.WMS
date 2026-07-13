using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KH.WMS.Algorithms.Strategy.Configuration;
using KH.WMS.Algorithms.Strategy.Constants;
using KH.WMS.Algorithms.Strategy.Enums;
using KH.WMS.Algorithms.Strategy.Interfaces;
using KH.WMS.Core.Database.Repositories;
using KH.WMS.Core.DependencyInjection.ServiceLifetimes;
using KH.WMS.Core.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace KH.WMS.Algorithms.Strategy.Services
{
    /// <summary>
    /// 策略引擎实现
    /// </summary>
    [RegisteredService(ServiceType = typeof(IPolicyEngine))]
    public class PolicyEngine : IPolicyEngine
    {
        private readonly IPolicyRegistry _registry;
        private readonly ILoggerService _logger;
        private readonly IServiceProvider _serviceProvider;

        public PolicyEngine(IPolicyRegistry registry, ILoggerService logger, IServiceProvider serviceProvider)
        {
            _registry = registry;
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public IPolicyRegistry Registry => _registry;

        public async Task<IPolicyResult> ExecuteAsync(PolicyType policyType, IPolicyContext context, CancellationToken cancellationToken = default)
        {
            _logger.LogInfo($"执行策略类型: {policyType}, 仓库: {context.WarehouseId}, 库区: {context.ZoneId}, 单据类型: {context.DocType}, 物料: {context.MaterialId}, 单据: {context.BusinessCode}");

            // 尝试从数据库配置中加载策略链
            var chain = await BuildChainFromDbAsync(policyType, context, cancellationToken);
            if (chain != null)
            {
                _logger.LogInfo($"使用数据库配置的策略链执行，步骤数: {chain.StrategyCount}");
                return await chain.ExecuteAsync(context, cancellationToken);
            }

            // 回退：使用所有已注册策略（兜底逻辑）
            _logger.LogInfo($"未找到数据库配置的策略链，使用所有已注册的 {policyType} 策略");
            var fallbackChain = _registry.CreateChain($"{policyType}_Chain", policyType);
            return await fallbackChain.ExecuteAsync(context, cancellationToken);
        }

        /// <summary>
        /// 从数据库配置中构建策略链
        /// 匹配优先级（从高到低）：仓库+库区+单据类型 > 仓库+单据类型 > 仓库 > 默认链
        /// </summary>
        private async Task<PolicyChain?> BuildChainFromDbAsync(PolicyType policyType, IPolicyContext context, CancellationToken cancellationToken)
        {
            try
            {
                var chainType = policyType.ToString();
                var chainService = _serviceProvider.GetService<IStrategyChainService>();
                if (chainService == null)
                {
                    _logger.LogInfo($"无法获取 IStrategyChainService，跳过数据库策略链查询");
                    return null;
                }

                // 查询该类型的所有已启用策略链（按仓库过滤）
                var chains = await chainService.GetByTypeAsync(chainType, context.WarehouseId);
                if (chains == null || chains.Count == 0)
                {
                    _logger.LogInfo($"数据库中没有 {chainType} 类型的策略链配置");
                    return null;
                }

                // 按匹配优先级选择最佳策略链，匹配不到则使用默认链（IsDefault=1）
                var matchedChain = SelectBestChain(chains, context);
                if (matchedChain == null)
                {
                    matchedChain = chains
                        .Where(c => c.IsDefault == AlgoConstants.BoolFlag.YES)
                        .OrderByDescending(c => c.Priority)
                        .FirstOrDefault();

                    if (matchedChain != null)
                        _logger.LogInfo($"未精确匹配到策略链，使用默认链: {matchedChain.ChainCode} ({matchedChain.ChainName})");
                }

                if (matchedChain == null)
                {
                    _logger.LogInfo($"未找到匹配的策略链，也没有配置默认链");
                    return null;
                }

                _logger.LogInfo($"匹配到策略链: {matchedChain.ChainCode} ({matchedChain.ChainName}), ChainId={matchedChain.Id}");

                // 获取策略链步骤
                var steps = await chainService.GetStepsByChainIdAsync(matchedChain.Id);
                if (steps == null || steps.Count == 0)
                {
                    _logger.LogInfo($"策略链 {matchedChain.ChainCode} 没有配置任何步骤 (ChainId={matchedChain.Id})");
                    return null;
                }

                // 按 StepNo 排序，只取启用的步骤
                var enabledSteps = steps
                    .Where(s => s.IsEnabled == AlgoConstants.BoolFlag.YES)
                    .OrderBy(s => s.StepNo)
                    .ToList();

                if (enabledSteps.Count == 0)
                {
                    _logger.LogInfo($"策略链 {matchedChain.ChainCode} 所有步骤均已禁用 (总步骤数={steps.Count})");
                    return null;
                }

                // 构建 PolicyChain
                var chain = new PolicyChain(matchedChain.ChainName, matchedChain.ChainCode);

                // 货位分配策略链使用流水线模式（每个策略从上游结果中筛选）
                // A2: 出库分配层已移除，出库统一走 InventoryAllocation（单步，非流水线）
                if (policyType == PolicyType.LocationAllocation)
                    chain.PipelineMode = true;

                // 添加过滤器
                foreach (var filter in _registry.GetFilters().OrderBy(f => f.Order))
                {
                    chain.AddFilter(filter);
                }

                // 通过 StrategyConfigId 直接查询 CfgStrategyConfig，获取 RuleCode 再匹配 Registry 中的策略实例
                var configRepository = _serviceProvider.GetService<IRepository<CfgStrategyConfig, long>>();
                if (configRepository == null)
                {
                    _logger.LogInfo($"无法获取 CfgStrategyConfig 仓储，跳过策略实例查找");
                    return null;
                }

                var configIds = enabledSteps.Select(s => s.StrategyConfigId).Distinct().ToList();
                var configs = await configRepository.GetListAsync(c => configIds.Contains(c.Id));
                var configMap = configs.ToDictionary(c => c.Id);

                foreach (var step in enabledSteps)
                {
                    if (!configMap.TryGetValue(step.StrategyConfigId, out var config))
                    {
                        _logger.LogInfo($"策略链 {matchedChain.ChainCode} 步骤 {step.StepNo} 的 StrategyConfigId={step.StrategyConfigId} 不存在，跳过");
                        continue;
                    }

                    var ruleCode = config.RuleCode;
                    var strategy = _registry.GetStrategy(ruleCode);
                    if (strategy != null)
                    {
                        // 传入步骤级参数和策略默认参数，策略基类按 StepParams > StrategyParams 优先级读取
                        chain.AddStrategy(strategy, step.StepParams, config.StrategyParams);
                        _logger.LogInfo($"策略链 {matchedChain.ChainCode} 添加步骤 {step.StepNo}: {config.StrategyName} ({ruleCode}), StepParams={(string.IsNullOrEmpty(step.StepParams) ? "null" : "已配置")}, StrategyParams={(string.IsNullOrEmpty(config.StrategyParams) ? "null" : "已配置")}");
                    }
                    else
                    {
                        _logger.LogInfo($"策略链 {matchedChain.ChainCode} 步骤 {step.StepNo} 的规则 {ruleCode} (配置: {config.StrategyCode}) 未在引擎中注册，跳过");
                    }
                }

                if (chain.StrategyCount == 0)
                {
                    _logger.LogInfo($"策略链 {matchedChain.ChainCode} 构建后无可用策略实例");
                    return null;
                }

                return chain;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"从数据库构建策略链时发生异常: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 按匹配优先级选择最佳策略链
        /// 优先级：仓库+库区+订单类型+单据类型 > 仓库+订单类型+单据类型 > 仓库+库区+单据类型 > 仓库+单据类型 > 仓库 > 默认链
        /// 同级别按 Priority 字段降序
        /// </summary>
        private CfgStrategyChainConfig? SelectBestChain(List<CfgStrategyChainConfig> chains, IPolicyContext context)
        {
            var warehouseId = context.WarehouseId;
            var zoneId = context.ZoneId;
            var docType = context.DocType;

            // 按匹配精度分组打分
            CfgStrategyChainConfig? best = null;
            int bestScore = -1;

            foreach (var chain in chains)
            {
                int score = 0;
                bool mismatch = false;

                // 仓库匹配
                if (chain.WarehouseId.HasValue && chain.WarehouseId.Value > 0)
                {
                    if (warehouseId.HasValue && warehouseId.Value == chain.WarehouseId.Value)
                        score += AlgoConstants.ChainMatchScore.WAREHOUSE;
                    else
                        mismatch = true;
                }

                // 库区匹配
                if (chain.ZoneId.HasValue && chain.ZoneId.Value > 0)
                {
                    if (zoneId.HasValue && zoneId.Value == chain.ZoneId.Value)
                        score += AlgoConstants.ChainMatchScore.ZONE;
                    else
                        mismatch = true;
                }

                // 单据类型匹配（#16：仅匹配 context.DocType；CfgStrategyChainConfig 无 OrderType 列，
                // 将 chain.DocType 与 context.OrderType 混比属跨语义误匹配，已移除）
                if (!string.IsNullOrEmpty(chain.DocType))
                {
                    if (!string.IsNullOrEmpty(docType) && string.Equals(docType, chain.DocType, StringComparison.OrdinalIgnoreCase))
                        score += AlgoConstants.ChainMatchScore.DOC_TYPE;
                    else
                        mismatch = true;
                }

                if (mismatch)
                    continue;

                // 同匹配分数下，按 Priority 降序，再按 IsDefault 降序
                if (score > bestScore || (score == bestScore && chain.Priority > (best?.Priority ?? 0)))
                {
                    bestScore = score;
                    best = chain;
                }
            }

            return best;
        }

        public async Task<IPolicyResult> ExecuteAsync(string policyCode, IPolicyContext context, CancellationToken cancellationToken = default)
        {
            _logger.LogInfo($"执行策略: {policyCode}, 仓库: {context.WarehouseId}, 单据: {context.BusinessCode}");

            var strategy = _registry.GetStrategy(policyCode);
            if (strategy == null)
            {
                return PolicyResult.Failure($"策略 {policyCode} 未注册");
            }

            if (!strategy.IsEnabled)
            {
                return PolicyResult.Skipped($"策略 {strategy.Name} 未启用");
            }

            if (!await strategy.IsApplicableAsync(context, cancellationToken))
            {
                return PolicyResult.Skipped($"策略 {strategy.Name} 不适用当前上下文");
            }

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                var result = await strategy.ExecuteAsync(context, cancellationToken);
                stopwatch.Stop();

                _logger.LogInfo($"策略 {strategy.Name} 执行完成, 耗时: {stopwatch.ElapsedMilliseconds}ms, 成功: {result.IsSuccess}");

                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, $"策略 {strategy.Name} 执行失败");
                return PolicyResult.Failure($"策略执行失败: {ex.Message}");
            }
        }

        public async Task<IPolicyResult> ExecuteChainAsync(string chainCode, IPolicyContext context, CancellationToken cancellationToken = default)
        {
            _logger.LogInfo($"执行策略链: {chainCode}, 仓库: {context.WarehouseId}, 单据: {context.BusinessCode}");

            // 尝试从注册中心获取已配置的策略链
            var chain = _registry.GetChain(chainCode);
            if (chain != null)
            {
                return await chain.ExecuteAsync(context, cancellationToken);
            }

            return PolicyResult.Failure($"策略链 {chainCode} 未注册");
        }
    }
}
