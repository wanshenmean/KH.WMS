using System.Text.Json;
using KH.WMS.Algorithms.Strategy.Constants;
using KH.WMS.Algorithms.Strategy.Enums;
using KH.WMS.Algorithms.Strategy.Interfaces;
using KH.WMS.Algorithms.Strategy.QueryServices;

namespace KH.WMS.Algorithms.Strategy.Strategies
{
    /// <summary>
    /// 货位分配策略基类
    /// 入库上架时根据物料属性、仓库布局、业务规则等因素，智能计算最优货位
    /// 高位立体库场景：筛选之后还要可配置按照排列层等属性排序
    /// </summary>
    public abstract class LocationAllocationStrategyBase(ILocationQueryService locationQueryService = null) : PolicyStrategyBase
    {
        public sealed override PolicyType PolicyType => PolicyType.LocationAllocation;

        /// <summary>
        /// 计算推荐货位列表
        /// </summary>
        protected abstract Task<LocationAllocationResult> AllocateAsync(IPolicyContext context, CancellationToken cancellationToken = default);

        public sealed override async Task<IPolicyResult> ExecuteAsync(IPolicyContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await AllocateAsync(context, cancellationToken);
                if (result == null || !result.Locations.Any())
                    return PolicyResult.Skipped("无可用货位");

                context.SetData(StrategyParams.LocationAllocationOutput.RESULT, result);
                return PolicyResult.Success(result);
            }
            catch (Exception ex)
            {
                return PolicyResult.Failure($"货位分配失败: {ex.Message}");
            }
        }

        // =====================================================================
        //  通用后处理方法 — 子类在 AllocateAsync 末尾调用
        // =====================================================================

        /// <summary>
        /// 对推荐货位列表进行后处理：可配置排序 → 双深过滤 → 数量截断
        /// 子类在构建完原始推荐列表后调用此方法
        /// </summary>
        protected async Task<List<LocationRecommendation>> PostProcess(
            List<LocationRecommendation> locations,
            IPolicyContext context,
            CancellationToken cancellationToken = default)
        {
            var processed = locations.AsEnumerable();

            // 1. 可配置排序
            processed = ApplySortRules(processed, context);

            // 2. 双深货位约束过滤（#3：前排占用校验，需异步查询）
            processed = await ApplyDoubleDeepFilter(processed, context, cancellationToken);

            // 3. 数量截断
            processed = ApplyMaxCount(processed, context);

            return processed.ToList();
        }

        /// <summary>
        /// 从上下文中读取步骤参数 JSON
        /// </summary>
        protected string? GetParamsJson(IPolicyContext context)
        {
            return context.GetData<string>(StrategyParams.LocationAllocationInput.STEP_PARAMS)
                ?? context.GetData<string>(StrategyParams.LocationAllocationInput.STRATEGY_PARAMS);
        }

        /// <summary>
        /// 从上下文中获取上游策略传递过来的候选货位列表（流水线模式）
        /// 返回 null 表示无上游结果（当前为链中第一个策略，需要从数据库查询）
        /// </summary>
        protected List<LocationRecommendation>? GetPipelineCandidates(IPolicyContext context)
        {
            var existingResult = context.GetData<LocationAllocationResult>(StrategyParams.LocationAllocationOutput.RESULT);
            return existingResult?.Locations;
        }

        /// <summary>
        /// 浅拷贝货位推荐项（#48：流水线筛选追加 Reason/Score 前先克隆，避免直接变异上游共享对象）
        /// </summary>
        protected static LocationRecommendation CloneRecommendation(LocationRecommendation r)
        {
            return new LocationRecommendation
            {
                LocationId = r.LocationId,
                LocationCode = r.LocationCode,
                AisleNo = r.AisleNo,
                RowNo = r.RowNo,
                ColNo = r.ColNo,
                LayerNo = r.LayerNo,
                Depth = r.Depth,
                ZoneCode = r.ZoneCode,
                Score = r.Score,
                Reason = r.Reason,
                Side = r.Side,
            };
        }

        /// <summary>
        /// 从上下文中读取最大推荐数量
        /// </summary>
        protected int GetMaxRecommendCount(IPolicyContext context)
        {
            var paramsJson = GetParamsJson(context);
            if (string.IsNullOrWhiteSpace(paramsJson))
                return AlgoConstants.DEFAULT_MAX_RECOMMEND_COUNT;

            try
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var doc = JsonSerializer.Deserialize<JsonElement>(paramsJson, options);
                if (doc.TryGetProperty(StrategyParams.LocationAllocationInput.MAX_RECOMMEND_COUNT, out var prop) && prop.ValueKind == JsonValueKind.Number)
                    return prop.GetInt32();
            }
            catch { }

            return AlgoConstants.DEFAULT_MAX_RECOMMEND_COUNT;
        }

        /// <summary>
        /// 从上下文中读取是否启用双深货位约束
        /// </summary>
        protected bool IsDoubleDeepEnabled(IPolicyContext context)
        {
            var paramsJson = GetParamsJson(context);
            if (string.IsNullOrWhiteSpace(paramsJson))
                return false;

            try
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var doc = JsonSerializer.Deserialize<JsonElement>(paramsJson, options);
                if (doc.TryGetProperty(StrategyParams.LocationAllocationInput.ENABLE_DOUBLE_DEEP, out var prop) && prop.ValueKind == JsonValueKind.True)
                    return true;
            }
            catch { }

            return false;
        }

        /// <summary>
        /// 从上下文中读取双深货位分配模式
        /// </summary>
        protected string GetDoubleDeepMode(IPolicyContext context)
        {
            var paramsJson = GetParamsJson(context);
            if (string.IsNullOrWhiteSpace(paramsJson))
                return AlgoConstants.DoubleDeepMode.FRONT_FIRST;

            try
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var doc = JsonSerializer.Deserialize<JsonElement>(paramsJson, options);
                if (doc.TryGetProperty(StrategyParams.LocationAllocationInput.DOUBLE_DEEP_MODE, out var prop) && prop.ValueKind == JsonValueKind.String)
                    return prop.GetString() ?? AlgoConstants.DoubleDeepMode.FRONT_FIRST;
            }
            catch { }

            return AlgoConstants.DoubleDeepMode.FRONT_FIRST;
        }

        // =====================================================================
        //  内部实现
        // =====================================================================

        /// <summary>
        /// 应用可配置排序规则
        /// </summary>
        private IEnumerable<LocationRecommendation> ApplySortRules(
            IEnumerable<LocationRecommendation> locations, IPolicyContext context)
        {
            var sortRules = ParseSortRules(context);
            if (sortRules == null || sortRules.Count == 0)
                return locations;

            var result = locations;
            // 按规则逆序依次排序（稳定排序）
            for (var i = sortRules.Count - 1; i >= 0; i--)
            {
                var rule = sortRules[i];
                result = rule.Direction == AlgoConstants.SortDirection.DESC
                    ? result.OrderByDescending(l => GetFieldValue(l, rule.Field))
                    : result.OrderBy(l => GetFieldValue(l, rule.Field));
            }

            return result;
        }

        /// <summary>
        /// 应用双深货位约束过滤
        /// 核心规则：
        /// - FRONT_FIRST: 优先前排，后排货位只有在对应前排被占用时才能分配（后排不放空前排前面）
        /// - BACK_FIRST: 优先后排，适用于先进后出场景
        /// - PAIR: 同一地址前后配对使用
        /// </summary>
        private async Task<IEnumerable<LocationRecommendation>> ApplyDoubleDeepFilter(
            IEnumerable<LocationRecommendation> locations, IPolicyContext context, CancellationToken cancellationToken)
        {
            if (!IsDoubleDeepEnabled(context))
                return locations;

            var mode = GetDoubleDeepMode(context);

            return mode switch
            {
                AlgoConstants.DoubleDeepMode.FRONT_FIRST => await ApplyFrontFirstFilter(locations, context, cancellationToken),
                AlgoConstants.DoubleDeepMode.BACK_FIRST => ApplyBackFirstFilter(locations),
                AlgoConstants.DoubleDeepMode.PAIR => ApplyPairFilter(locations),
                _ => locations
            };
        }

        /// <summary>
        /// FRONT_FIRST 模式过滤（#3 修复：真正校验前排占用）：
        /// 前排空闲货位全部保留；后排空闲货位仅当对应前排被占用(OCCUPIED)或不存在前排(单深)时才保留，
        /// 前排为空(EMPTY)时丢弃该后排（避免放后排堵住前排导致无法出库）。
        /// 需注入 ILocationQueryService 查询前排状态；未注入时保守保留后排（仅标注）。
        /// </summary>
        private async Task<IEnumerable<LocationRecommendation>> ApplyFrontFirstFilter(
            IEnumerable<LocationRecommendation> locations, IPolicyContext context, CancellationToken cancellationToken)
        {
            var list = locations.ToList();

            var frontLocations = list.Where(l => l.Depth == AlgoConstants.DepthType.FRONT).ToList();
            var backLocations = list.Where(l => l.Depth == AlgoConstants.DepthType.BACK).ToList();

            var result = new List<LocationRecommendation>(frontLocations);

            var locationService = locationQueryService;

            foreach (var back in backLocations)
            {
                bool keepBack = true;
                if (locationService != null && long.TryParse(back.LocationId, out var backLocId) && backLocId > 0)
                {
                    var frontStatus = await locationService.GetFrontLocationStatusAsync(backLocId, cancellationToken);
                    // 前排空闲(EMPTY) → 放后排会堵住前排，丢弃；前排占用(OCCUPIED)或无前排(null) → 保留
                    keepBack = !string.Equals(frontStatus, AlgoConstants.LocationStatus.EMPTY, StringComparison.OrdinalIgnoreCase);
                    if (!keepBack)
                    {
                        back.Reason += " [双深-后排已丢弃：前排空闲，放后排会堵仓]";
                        continue;
                    }
                }
                back.Reason += " [双深-后排，前排已占用或无前排]";
                result.Add(back);
            }

            // 按前排优先排序
            return result.OrderBy(l => l.Depth ?? 1);
        }

        /// <summary>
        /// BACK_FIRST 模式过滤：
        /// 优先后排空货位，后排满后再用前排
        /// </summary>
        private IEnumerable<LocationRecommendation> ApplyBackFirstFilter(
            IEnumerable<LocationRecommendation> locations)
        {
            var list = locations.ToList();

            var backLocations = list.Where(l => l.Depth == AlgoConstants.DepthType.BACK).ToList();
            var frontLocations = list.Where(l => l.Depth == AlgoConstants.DepthType.FRONT).ToList();

            // 后排优先
            var result = new List<LocationRecommendation>(backLocations);
            result.AddRange(frontLocations);

            return result;
        }

        /// <summary>
        /// PAIR 模式过滤：
        /// 同一地址（仓库+巷道+排+列+层）的前后排配对使用
        /// 只有前后排都为空时才推荐（成对分配）
        /// </summary>
        private IEnumerable<LocationRecommendation> ApplyPairFilter(
            IEnumerable<LocationRecommendation> locations)
        {
            var list = locations.ToList();

            // 按物理地址分组
            var groups = list
                .GroupBy(l => new { l.AisleNo, l.Side, l.RowNo, l.ColNo, l.LayerNo, l.ZoneCode })
                .ToList();

            var result = new List<LocationRecommendation>();
            foreach (var group in groups)
            {
                var hasFront = group.Any(l => l.Depth == AlgoConstants.DepthType.FRONT);
                var hasBack = group.Any(l => l.Depth == AlgoConstants.DepthType.BACK);

                if (hasFront && hasBack)
                {
                    // 前后都有空货位，配对推荐（前排排前面）
                    result.AddRange(group.OrderBy(l => l.Depth));
                }
                else if (hasFront && !hasBack)
                {
                    // 只有前排空，也可以单独推荐前排
                    result.AddRange(group.Where(l => l.Depth == AlgoConstants.DepthType.FRONT));
                }
                // 只有后排空、前排被占用 → 不推荐（挡住前排货物）
            }

            return result;
        }

        /// <summary>
        /// 应用最大推荐数量限制
        /// </summary>
        private IEnumerable<LocationRecommendation> ApplyMaxCount(
            IEnumerable<LocationRecommendation> locations, IPolicyContext context)
        {
            var maxCount = GetMaxRecommendCount(context);
            return locations.Take(maxCount);
        }

        /// <summary>
        /// 从上下文中解析排序规则
        /// </summary>
        private List<SortField>? ParseSortRules(IPolicyContext context)
        {
            var paramsJson = GetParamsJson(context);
            if (string.IsNullOrWhiteSpace(paramsJson))
                return null;

            try
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var doc = JsonSerializer.Deserialize<JsonElement>(paramsJson, options);
                if (doc.TryGetProperty(StrategyParams.LocationAllocationInput.SORT_RULES, out var sortRulesEl) && sortRulesEl.ValueKind == JsonValueKind.Array)
                {
                    var rules = JsonSerializer.Deserialize<List<SortField>>(sortRulesEl.GetRawText(), options);
                    return rules;
                }
            }
            catch { }

            return null;
        }

        /// <summary>
        /// 根据字段名获取排序值
        /// </summary>
        private static object GetFieldValue(LocationRecommendation location, string field)
        {
            return field switch
            {
                AlgoConstants.SortField.AISLE_NO => location.AisleNo ?? 0,
                AlgoConstants.SortField.ROW_NO => location.RowNo ?? 0,
                AlgoConstants.SortField.COL_NO => location.ColNo ?? 0,
                AlgoConstants.SortField.LAYER_NO => location.LayerNo ?? 0,
                AlgoConstants.SortField.DEPTH => location.Depth ?? 0,
                AlgoConstants.SortField.SCORE => location.Score,
                AlgoConstants.SortField.ZONE_CODE => location.ZoneCode ?? string.Empty,
                AlgoConstants.SortField.LOCATION_CODE => location.LocationCode,
                _ => 0
            };
        }
    }

    /// <summary>
    /// 货位分配结果
    /// </summary>
    public class LocationAllocationResult
    {
        /// <summary>推荐货位列表（按优先级排序）</summary>
        public List<LocationRecommendation> Locations { get; set; } = new();
    }

    /// <summary>
    /// 货位推荐项
    /// </summary>
    public class LocationRecommendation
    {
        /// <summary>货位ID</summary>
        public string LocationId { get; set; } = string.Empty;

        /// <summary>货位编码</summary>
        public string LocationCode { get; set; } = string.Empty;

        /// <summary>巷道号</summary>
        public int? AisleNo { get; set; }

        /// <summary>行号（排）</summary>
        public int? RowNo { get; set; }

        /// <summary>列号</summary>
        public int? ColNo { get; set; }

        /// <summary>层号</summary>
        public int? LayerNo { get; set; }

        /// <summary>深度（1前排 2后排）</summary>
        public int? Depth { get; set; }

        /// <summary>库区编码</summary>
        public string? ZoneCode { get; set; }

        /// <summary>评分（用于排序，越高越优）</summary>
        public decimal Score { get; set; }

        /// <summary>推荐理由</summary>
        public string Reason { get; set; } = string.Empty;

        /// <summary>
        /// 排（巷道左右两侧，1-左排 2-右排）
        /// 用于双深货位配对判断
        /// </summary>
        public int? Side { get; set; }
    }

    /// <summary>
    /// 排序字段定义
    /// </summary>
    public class SortField
    {
        /// <summary>排序字段名</summary>
        public string Field { get; set; } = string.Empty;

        /// <summary>排序方向（ASC / DESC）</summary>
        public string Direction { get; set; } = AlgoConstants.SortDirection.ASC;
    }
}
