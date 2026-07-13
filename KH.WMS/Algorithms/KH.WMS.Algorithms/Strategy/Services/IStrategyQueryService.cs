using KH.WMS.Algorithms.Strategy.Interfaces;

namespace KH.WMS.Algorithms.Strategy.Services
{
    /// <summary>
    /// 策略查询服务接口
    /// 从策略注册中心查询已注册的策略实现信息，供前端配置时选择
    /// </summary>
    public interface IStrategyQueryService
    {
        /// <summary>
        /// 获取所有已注册的策略列表（供前端下拉选择）
        /// </summary>
        List<StrategyRuntimeInfo> GetRegisteredStrategies(string? strategyType = null);

        /// <summary>
        /// 获取所有已注册的策略链列表
        /// </summary>
        List<StrategyChainRuntimeInfo> GetRegisteredChains();

        /// <summary>
        /// 获取所有策略类型及其已注册的策略数量
        /// </summary>
        List<StrategyTypeInfo> GetStrategyTypes();

        /// <summary>
        /// 获取前端配置页面所需的策略选项聚合数据
        /// 返回策略类型列表 + 按类型分组的规则编码映射
        /// </summary>
        StrategyOptionsResult GetStrategyOptions();
    }

    /// <summary>
    /// 策略选项聚合结果（供前端配置页面使用）
    /// </summary>
    public class StrategyOptionsResult
    {
        /// <summary>策略类型列表 [{ value, label }]</summary>
        public List<StrategyOptionItem> Types { get; set; } = new();

        /// <summary>按策略类型分组的规则编码映射 { "PUTAWAY": [{ value, label }, ...], ... }</summary>
        public Dictionary<string, List<StrategyOptionItem>> RuleCodeMap { get; set; } = new();

        /// <summary>按规则编码分组的参数Schema映射 { "ABC_CLASS": [...fields], ... }</summary>
        public Dictionary<string, List<ParamFieldDef>> ParamSchemaMap { get; set; } = new();
    }

    /// <summary>
    /// 策略选项项
    /// </summary>
    public class StrategyOptionItem
    {
        /// <summary>选项值</summary>
        public string Value { get; set; } = string.Empty;

        /// <summary>选项显示文本</summary>
        public string Label { get; set; } = string.Empty;
    }

    /// <summary>
    /// 策略参数字段定义（供前端动态渲染参数表单）
    /// 与前端 StrategyParamForm 组件的字段定义一一对应
    /// </summary>
    public class ParamFieldDef
    {
        public string Prop { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public object? DefaultValue { get; set; }
        public bool Required { get; set; }
        public int? Min { get; set; }
        public int? Max { get; set; }
        public int? Step { get; set; }
        public int? Precision { get; set; }
        public int? Rows { get; set; }
        public int? Maxlength { get; set; }
        public string? Placeholder { get; set; }
        public string? Hint { get; set; }
        public List<StrategyOptionItem>? Options { get; set; }
    }

    /// <summary>
    /// 策略运行时信息（从注册中心获取）
    /// </summary>
    public class StrategyRuntimeInfo
    {
        /// <summary>策略编码</summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>策略名称</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>策略类型</summary>
        public string PolicyType { get; set; } = string.Empty;

        /// <summary>作者</summary>
        public string Author { get; set; } = string.Empty;

        /// <summary>描述</summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>是否启用</summary>
        public bool IsEnabled { get; set; }

        /// <summary>优先级</summary>
        public int Priority { get; set; }
    }

    /// <summary>
    /// 策略链运行时信息
    /// </summary>
    public class StrategyChainRuntimeInfo
    {
        /// <summary>链编码</summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>链名称</summary>
        public string Name { get; set; } = string.Empty;
    }

    /// <summary>
    /// 策略类型信息
    /// </summary>
    public class StrategyTypeInfo
    {
        /// <summary>类型编码</summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>类型名称</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>已注册策略数量</summary>
        public int StrategyCount { get; set; }
    }
}
