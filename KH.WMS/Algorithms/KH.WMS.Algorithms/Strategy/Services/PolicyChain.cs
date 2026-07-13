using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KH.WMS.Algorithms.Strategy.Constants;
using KH.WMS.Algorithms.Strategy.Interfaces;
using KH.WMS.Algorithms.Strategy.Models;

namespace KH.WMS.Algorithms.Strategy.Services
{
    /// <summary>
    /// 策略链实现
    /// </summary>
    public class PolicyChain : IPolicyChain
    {
        private readonly List<StepEntry> _entries = new();
        private readonly List<IPolicyFilter> _filters = new();

        private struct StepEntry
        {
            public IPolicyStrategy Strategy;
            public string? StepParams;
            public string? StrategyParams;
        }

        public string Name { get; private set; } = string.Empty;
        public string Code { get; private set; } = string.Empty;
        public bool PipelineMode { get; set; } = false;
        public int StrategyCount => _entries.Count;

        public PolicyChain(string name, string code)
        {
            Name = name;
            Code = code;
        }

        public IPolicyChain AddStrategy(IPolicyStrategy strategy)
        {
            _entries.Add(new StepEntry { Strategy = strategy });
            return this;
        }

        public IPolicyChain AddStrategy(IPolicyStrategy strategy, string? stepParams, string? strategyParams)
        {
            _entries.Add(new StepEntry
            {
                Strategy = strategy,
                StepParams = stepParams,
                StrategyParams = strategyParams
            });
            return this;
        }

        public IPolicyChain AddFilter(IPolicyFilter filter)
        {
            _filters.Add(filter);
            return this;
        }

        public async Task<IPolicyResult> ExecuteAsync(IPolicyContext context, CancellationToken cancellationToken = default)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            IPolicyResult result = PolicyResult.Failure("策略链未执行任何策略");
            bool anyExecuted = false;
            var stepFailures = new List<string>(); // #11: 收集流水线中失败步骤，供部分成功告警

            try
            {
                // 执行前置过滤器
                foreach (var filter in _filters.OrderBy(f => f.Order))
                {
                    context = await filter.OnBeforeExecutionAsync(context, cancellationToken);
                }

                // 按优先级排序
                var sortedEntries = _entries
                    .Where(e => e.Strategy.IsEnabled)
                    .OrderByDescending(e => e.Strategy.Priority)
                    .ToList();

                foreach (var entry in sortedEntries)
                {
                    var strategy = entry.Strategy;
                    var strategyStopwatch = System.Diagnostics.Stopwatch.StartNew();

                    try
                    {
                        // 注入步骤级参数到上下文（策略基类通过 GetParamsJson 读取）。
                        // #25 修复：每步执行前先清空这两个键，避免上一步的参数在本步未配置时"跨步污染"。
                        context.SetData(StrategyParams.LocationAllocationInput.STRATEGY_PARAMS, (string?)null);
                        context.SetData(StrategyParams.LocationAllocationInput.STEP_PARAMS, (string?)null);
                        if (!string.IsNullOrEmpty(entry.StrategyParams))
                            context.SetData(StrategyParams.LocationAllocationInput.STRATEGY_PARAMS, entry.StrategyParams);
                        if (!string.IsNullOrEmpty(entry.StepParams))
                            context.SetData(StrategyParams.LocationAllocationInput.STEP_PARAMS, entry.StepParams);

                        // 检查策略是否适用
                        if (!await strategy.IsApplicableAsync(context, cancellationToken))
                        {
                            continue;
                        }

                        anyExecuted = true;

                        // 执行策略
                        var strategyResult = await strategy.ExecuteAsync(context, cancellationToken);

                        strategyStopwatch.Stop();

                        // 记录执行日志
                        result.AddExecutionLog(new PolicyExecutionLog
                        {
                            PolicyId = Guid.NewGuid(),
                            PolicyName = strategy.Name,
                            PolicyCode = strategy.Code,
                            IsSuccess = strategyResult.IsSuccess,
                            Message = strategyResult.ErrorMessage,
                            Duration = strategyStopwatch.ElapsedMilliseconds,
                            ExecutedAt = DateTime.Now
                        });

                        // 如果策略已处理且成功
                        if (strategyResult.IsHandled && strategyResult.IsSuccess)
                        {
                            // 原地更新成功状态，保留链级执行日志
                            ((PolicyResult)result).MarkAsSuccess(strategyResult.Output);
                            result.ExecutionLogs.AddRange(strategyResult.ExecutionLogs);

                            // 流水线模式：继续执行后续策略（从上游结果中筛选）
                            // 回退模式：第一个成功即停
                            if (!PipelineMode)
                                break;
                        }

                        // 如果策略失败，记录错误但继续执行（#11：收集失败信息供流水线告警）
                        if (!strategyResult.IsSuccess)
                        {
                            stepFailures.Add($"{strategy.Name}: {strategyResult.ErrorMessage}");
                            result.ExecutionLogs.AddRange(strategyResult.ExecutionLogs);
                        }
                    }
                    catch (Exception ex)
                    {
                        strategyStopwatch.Stop();

                        anyExecuted = true;
                        stepFailures.Add($"{strategy.Name}(异常): {ex.Message}");

                        result.AddExecutionLog(new PolicyExecutionLog
                        {
                            PolicyId = Guid.NewGuid(),
                            PolicyName = strategy.Name,
                            PolicyCode = strategy.Code,
                            IsSuccess = false,
                            Message = ex.Message,
                            Duration = strategyStopwatch.ElapsedMilliseconds
                        });
                    }
                }

                // 如果没有策略被执行（全部不适用），返回跳过结果
                if (!anyExecuted)
                {
                    result = PolicyResult.Skipped("没有适用的策略");
                }

                // #11: 流水线模式下若有步骤失败，保留部分成功结果但在 ErrorMessage 记录告警，供调用方感知
                if (PipelineMode && stepFailures.Count > 0 && result.IsSuccess)
                    ((PolicyResult)result).MarkPartialFailure("流水线部分步骤失败: " + string.Join("; ", stepFailures));

                stopwatch.Stop();
                result.SetDuration(stopwatch.ElapsedMilliseconds);

                // 执行后置过滤器
                foreach (var filter in _filters.OrderBy(f => f.Order))
                {
                    result = await filter.OnAfterExecutionAsync(context, result, cancellationToken);
                }

                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                return PolicyResult.Failure($"策略链执行失败: {ex.Message}");
            }
        }
    }
}
