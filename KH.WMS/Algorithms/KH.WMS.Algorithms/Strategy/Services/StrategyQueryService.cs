using KH.WMS.Algorithms.Strategy.Constants;
using KH.WMS.Algorithms.Strategy.Enums;
using KH.WMS.Algorithms.Strategy.Interfaces;
using KH.WMS.Core.DependencyInjection.ServiceLifetimes;
using Microsoft.Extensions.DependencyInjection;

namespace KH.WMS.Algorithms.Strategy.Services
{
    /// <summary>
    /// 策略查询服务实现
    /// 从策略注册中心读取已注册的策略实现信息
    /// </summary>
    [RegisteredService(Lifetime = ServiceLifetime.Scoped, ServiceType = typeof(IStrategyQueryService))]
    public class StrategyQueryService : IStrategyQueryService
    {
        private readonly IPolicyRegistry _registry;

        private static readonly Dictionary<string, string> PolicyTypeNames = new()
        {
            [PolicyType.Putaway.ToString()] = "上架策略",
            [PolicyType.LocationAllocation.ToString()] = "货位分配策略",
            [PolicyType.Picking.ToString()] = "下架策略",
            [PolicyType.InventoryAllocation.ToString()] = "库存分配策略",
            [PolicyType.Wave.ToString()] = "波次策略",
            [PolicyType.OutboundAllocation.ToString()] = "出库分配策略"
        };

        /// <summary>
        /// 策略参数Schema定义（按策略规则编码索引）
        /// 每个策略的参数表单定义，供前端动态渲染
        /// </summary>
        private static readonly Dictionary<string, List<ParamFieldDef>> ParamSchemas = new()
        {
            ["DEFAULT_PUTAWAY"] = new()
            {
                new() { Prop = "EnableZonePartition", Label = "按库区分区", Type = "switch", DefaultValue = true },
                new() { Prop = "EnableAbcClass", Label = "ABC分类匹配", Type = "switch", DefaultValue = true },
                new() { Prop = "EnableNearby", Label = "就近上架", Type = "switch", DefaultValue = true },
                new() { Prop = "PathOptimization", Label = "路径优化", Type = "select", DefaultValue = "S_SHAPE", Options = PathOptimizationOptions },
            },
            ["DEFAULT_PICKING"] = new()
            {
                new() { Prop = "EnablePalletFirst", Label = "整托优先", Type = "switch", DefaultValue = true },
                new() { Prop = "PathOptimization", Label = "路径优化", Type = "select", DefaultValue = "S_SHAPE", Options = PathOptimizationOptions },
            },
            ["FIFO"] = new()
            {
                new() { Prop = "RequiredQty", Label = "需求数量", Type = "number", Min = 0, Max = 99999, Required = true, DefaultValue = 0 },
            },
            ["FEFO"] = new()
            {
                new() { Prop = "RequiredQty", Label = "需求数量", Type = "number", Min = 0, Max = 99999, Required = true, DefaultValue = 0 },
            },
            ["BATCH"] = new()
            {
                new() { Prop = "TargetBatchNo", Label = "目标批次号", Type = "input", Required = false, Placeholder = "为空时使用FIFO" },
                new() { Prop = "RequiredQty", Label = "需求数量", Type = "number", Min = 0, Max = 99999, Required = true, DefaultValue = 0 },
            },
            ["UTILIZATION_PRIORITY"] = new()
            {
                new() { Prop = "RequiredQty", Label = "需求数量", Type = "number", Min = 0, Max = 99999, Required = true, DefaultValue = 0 },
            },
            ["ABC_CLASS"] = new()
            {
                new()
                {
                    Prop = "MaterialAbcClass", Label = "物料ABC分类", Type = "select",
                    Required = false, Placeholder = "不选则自动查询物料周转分类",
                    Options = new()
                    {
                        new() { Label = "A类 - 高频物料", Value = "A" },
                        new() { Label = "B类 - 中频物料", Value = "B" },
                        new() { Label = "C类 - 低频物料", Value = "C" }
                    }
                },
                new() { Prop = StrategyParams.LocationAllocationInput.MAX_RECOMMEND_COUNT, Label = "最大推荐数量", Type = "number", Min = 1, Max = 200, DefaultValue = 20 },
                new() { Prop = StrategyParams.LocationAllocationInput.SORT_RULES, Label = "排序规则", Type = "sort-rules", DefaultValue = new List<object>(), Hint = "按顺序依次排序。不填则按策略内部默认排序。" },
                new() { Prop = StrategyParams.LocationAllocationInput.ENABLE_DOUBLE_DEEP, Label = "启用双深货位约束", Type = "switch", DefaultValue = false },
                new() { Prop = StrategyParams.LocationAllocationInput.DOUBLE_DEEP_MODE, Label = "双深分配模式", Type = "select", DefaultValue = AlgoConstants.DoubleDeepMode.FRONT_FIRST, Options = DoubleDeepModeOptions },
            },
            ["CATEGORY_ZONE"] = new()
            {
                new() { Prop = "CategoryZoneMapping", Label = "品类-库区映射", Type = "category-mapping", DefaultValue = new List<object>(), Hint = "指定物料分类与库区的对应关系。不填则由系统自动匹配。" },
                new() { Prop = StrategyParams.LocationAllocationInput.MAX_RECOMMEND_COUNT, Label = "最大推荐数量", Type = "number", Min = 1, Max = 200, DefaultValue = 20 },
                new() { Prop = StrategyParams.LocationAllocationInput.SORT_RULES, Label = "排序规则", Type = "sort-rules", DefaultValue = new List<object>(), Hint = "按顺序依次排序。不填则按策略内部默认排序。" },
                new() { Prop = StrategyParams.LocationAllocationInput.ENABLE_DOUBLE_DEEP, Label = "启用双深货位约束", Type = "switch", DefaultValue = false },
                new() { Prop = StrategyParams.LocationAllocationInput.DOUBLE_DEEP_MODE, Label = "双深分配模式", Type = "select", DefaultValue = AlgoConstants.DoubleDeepMode.FRONT_FIRST, Options = DoubleDeepModeOptions },
            },
            ["CENTRALIZED"] = new()
            {
                new() { Prop = "MaxNearbyCount", Label = "附近货位数量", Type = "number", Min = 1, Max = 100, DefaultValue = 10 },
                new() { Prop = StrategyParams.LocationAllocationInput.MAX_RECOMMEND_COUNT, Label = "最大推荐数量", Type = "number", Min = 1, Max = 200, DefaultValue = 20 },
                new() { Prop = StrategyParams.LocationAllocationInput.SORT_RULES, Label = "排序规则", Type = "sort-rules", DefaultValue = new List<object>(), Hint = "按顺序依次排序。不填则按策略内部默认排序。" },
                new() { Prop = StrategyParams.LocationAllocationInput.ENABLE_DOUBLE_DEEP, Label = "启用双深货位约束", Type = "switch", DefaultValue = false },
                new() { Prop = StrategyParams.LocationAllocationInput.DOUBLE_DEEP_MODE, Label = "双深分配模式", Type = "select", DefaultValue = AlgoConstants.DoubleDeepMode.FRONT_FIRST, Options = DoubleDeepModeOptions },
            },
            ["DOUBLE_DEEP"] = new()
            {
                new() { Prop = "Mode", Label = "双深分配模式", Type = "select", Required = true, DefaultValue = AlgoConstants.DoubleDeepMode.FRONT_FIRST, Options = DoubleDeepModeOptions },
                new() { Prop = StrategyParams.LocationAllocationInput.MAX_RECOMMEND_COUNT, Label = "最大推荐数量", Type = "number", Min = 1, Max = 200, DefaultValue = 20, Required = false },
                new() { Prop = StrategyParams.LocationAllocationInput.SORT_RULES, Label = "排序规则", Type = "sort-rules", DefaultValue = new List<object>(), Hint = "按顺序依次排序。不填则默认按层→深→排→列升序。" },
            },
            ["WHOLE_PALLET_FIRST"] = new()
            {
                new() { Prop = "EnableWholePalletFirst", Label = "整托优先", Type = "switch", DefaultValue = true },
                new()
                {
                    Prop = "AllocationMode", Label = "分配模式", Type = "select", Required = false,
                    DefaultValue = "FULL_PALLET",
                    Options = new()
                    {
                        new() { Label = "整托优先", Value = "FULL_PALLET" },
                        new() { Label = "散件拣选", Value = "SCATTERED" },
                        new() { Label = "混合模式", Value = "MIXED" }
                    }
                },
            },
            ["ZONE_PRIORITY"] = new()
            {
                new()
                {
                    Prop = "ZonePriorityList", Label = "分区优先级", Type = "textarea", Rows = 6,
                    Placeholder = "[{\"ZoneType\":\"PICKING\",\"Priority\":1},{\"ZoneType\":\"STORAGE\",\"Priority\":2}]",
                    Hint = "JSON数组格式，按Priority升序依次查找。支持ZoneId（精确匹配库区）和ZoneType（匹配库区类型）。"
                },
            },
            ["SCATTERED_PICK"] = new()
            {
                new()
                {
                    Prop = "AllocationMode", Label = "分配模式", Type = "select", Required = false,
                    DefaultValue = "SCATTERED",
                    Options = new()
                    {
                        new() { Label = "散件拣选", Value = "SCATTERED" },
                        new() { Label = "混合模式", Value = "MIXED" }
                    }
                },
            },
        };

        private static List<StrategyOptionItem> PathOptimizationOptions => new()
        {
            new() { Label = "S型路线", Value = "S_SHAPE" },
            new() { Label = "Z型路线", Value = "Z_SHAPE" },
            new() { Label = "U型路线", Value = "U_SHAPE" },
        };

        private static List<StrategyOptionItem> DoubleDeepModeOptions => new()
        {
            new() { Label = "前排优先 (FRONT_FIRST)", Value = "FRONT_FIRST" },
            new() { Label = "后排优先 (BACK_FIRST)", Value = "BACK_FIRST" },
            new() { Label = "前后配对 (PAIR)", Value = "PAIR" },
        };

        public StrategyQueryService(IPolicyRegistry registry)
        {
            _registry = registry;
        }

        public List<StrategyRuntimeInfo> GetRegisteredStrategies(string? strategyType = null)
        {
            // #35：非法 strategyType 用 TryParse 容错返回空，避免 Enum.Parse 抛异常导致 500
            var types = string.IsNullOrEmpty(strategyType)
                ? Enum.GetValues<PolicyType>()
                : (Enum.TryParse<PolicyType>(strategyType, ignoreCase: true, out var t)
                    ? new[] { t }
                    : Array.Empty<PolicyType>());

            var result = new List<StrategyRuntimeInfo>();
            foreach (var type in types)
            {
                var strategies = _registry.GetStrategies(type);
                foreach (var s in strategies)
                {
                    result.Add(new StrategyRuntimeInfo
                    {
                        Code = s.Code,
                        Name = s.Name,
                        PolicyType = s.PolicyType.ToString(),
                        Author = s.Author,
                        Description = s.Description,
                        IsEnabled = s.IsEnabled,
                        Priority = s.Priority
                    });
                }
            }

            return result;
        }

        public List<StrategyChainRuntimeInfo> GetRegisteredChains()
        {
            // 从注册中心无法直接遍历所有chain，返回空列表
            // 运行时策略链由配置表驱动，不在此处列出
            return new List<StrategyChainRuntimeInfo>();
        }

        public List<StrategyTypeInfo> GetStrategyTypes()
        {
            return Enum.GetValues<PolicyType>().Select(type => new StrategyTypeInfo
            {
                Code = type.ToString(),
                Name = PolicyTypeNames.GetValueOrDefault(type.ToString(), type.ToString()),
                StrategyCount = _registry.GetStrategies(type).Count()
            }).ToList();
        }

        public StrategyOptionsResult GetStrategyOptions()
        {
            var result = new StrategyOptionsResult();
            var ruleCodeMap = new Dictionary<string, List<StrategyOptionItem>>();

            foreach (var type in Enum.GetValues<PolicyType>())
            {
                var typeName = type.ToString();
                var displayName = PolicyTypeNames.GetValueOrDefault(typeName, typeName);

                // 仅添加有已注册策略的类型
                var strategies = _registry.GetStrategies(type).ToList();
                if (strategies.Count > 0)
                {
                    result.Types.Add(new StrategyOptionItem
                    {
                        Value = typeName,
                        Label = displayName
                    });

                    ruleCodeMap[typeName] = strategies.Select(s => new StrategyOptionItem
                    {
                        Value = s.Code,
                        Label = s.Name
                    }).OrderBy(x => x.Value).ToList();
                }
            }

            result.RuleCodeMap = ruleCodeMap;
            result.ParamSchemaMap = ParamSchemas;
            return result;
        }
    }
}
