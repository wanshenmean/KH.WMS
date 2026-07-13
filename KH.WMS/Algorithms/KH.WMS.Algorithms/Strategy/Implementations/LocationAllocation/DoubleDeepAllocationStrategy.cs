using KH.WMS.Algorithms.Strategy.Constants;
using KH.WMS.Algorithms.Strategy.Interfaces;
using KH.WMS.Algorithms.Strategy.QueryServices;
using KH.WMS.Algorithms.Strategy.Strategies;

namespace KH.WMS.Algorithms.Strategy.Implementations.LocationAllocation
{
    /// <summary>
    /// 双深货位分配策略
    /// 专门处理双深货架（Depth=1前排, Depth=2后排）的货位分配
    ///
    /// 核心约束：
    /// - 前排货位（Depth=1）被占用时，不能将货物放到后排（会挡住前排出库路径）
    /// - 即后排空闲时，必须检查前排是否也为空闲，或者前排已被占用（有货物）
    /// - 前排空闲 → 优先放前排
    /// - 前排被占用 + 后排空闲 → 可以放后排
    /// - 前排空闲 + 后排空闲 → 放前排
    /// - 前排空闲 + 后排被占用 → 放前排（后排已有货物，前排空闲正好放）
    ///
    /// StepParams JSON 示例：
    /// {
    ///   "Mode": "FRONT_FIRST",
    ///   "MaxRecommendCount": 20,
    ///   "SortRules": [
    ///     { "Field": "LayerNo", "Direction": "ASC" },
    ///     { "Field": "ColNo", "Direction": "ASC" },
    ///     { "Field": "Depth", "Direction": "ASC" }
    ///   ]
    /// }
    ///
    /// Mode 可选值：
    /// - FRONT_FIRST（默认）: 优先前排，前排满后才用后排
    /// - BACK_FIRST: 优先后排（先进后出场景）
    /// - PAIR: 前后配对，同一物理地址前后排一起使用
    /// </summary>
    public class DoubleDeepAllocationStrategy(IWarehouseQueryService warehouseQueryService, ILocationQueryService locationQueryService) : LocationAllocationStrategyBase
    {
        public override string Name => "双深货位分配策略";
        public override string Code => "DOUBLE_DEEP";
        public override string Author => "System";
        public override string Description => "专门处理双深货架货位分配，确保前后排约束正确，防止堵仓。支持前排优先/后排优先/配对模式";

        protected override async Task<LocationAllocationResult> AllocateAsync(IPolicyContext context, CancellationToken cancellationToken = default)
        {
            var warehouseService = warehouseQueryService;
            var locationService = locationQueryService;
            if (warehouseService == null || locationService == null)
                return new LocationAllocationResult();

            var warehouseId = context.WarehouseId ?? context.GetData<long>(StrategyParams.Common.WAREHOUSE_ID);
            if (warehouseId <= 0)
                return new LocationAllocationResult();

            var mode = GetMode(context);

            // 流水线模式：对上游候选列表应用双深约束
            var candidates = GetPipelineCandidates(context);
            if (candidates != null)
                return await ApplyDoubleDeepToCandidates(candidates, mode, context, cancellationToken);

            // 第一个策略：从数据库查询
            return await QueryFromDatabaseAsync(warehouseId, locationService, mode, context, cancellationToken);
        }

        /// <summary>
        /// 对上游候选列表应用双深约束过滤（流水线模式）
        /// </summary>
        private async Task<LocationAllocationResult> ApplyDoubleDeepToCandidates(
            List<LocationRecommendation> candidates, string mode, IPolicyContext context, CancellationToken cancellationToken)
        {
            // 分离前排和后排候选
            var frontLocations = candidates.Where(c => c.Depth == AlgoConstants.DepthType.FRONT).ToList();
            var backLocations = candidates.Where(c => c.Depth == AlgoConstants.DepthType.BACK).ToList();
            var noDepthLocations = candidates.Where(c => !c.Depth.HasValue || c.Depth == 0).ToList();

            var result = mode switch
            {
                AlgoConstants.DoubleDeepMode.FRONT_FIRST => ApplyFrontFirstToCandidates(frontLocations, backLocations),
                AlgoConstants.DoubleDeepMode.BACK_FIRST => ApplyBackFirstToCandidates(frontLocations, backLocations),
                AlgoConstants.DoubleDeepMode.PAIR => ApplyPairToCandidates(frontLocations, backLocations),
                _ => ApplyFrontFirstToCandidates(frontLocations, backLocations)
            };

            // 无深度信息的货位直接保留
            result.AddRange(noDepthLocations);

            foreach (var r in result)
                r.Reason += $" → 双深约束({mode})";

            var processed = await PostProcess(result, context, cancellationToken);
            return new LocationAllocationResult { Locations = processed };
        }

        private List<LocationRecommendation> ApplyFrontFirstToCandidates(
            List<LocationRecommendation> frontLocations, List<LocationRecommendation> backLocations)
        {
            // 前排全部保留
            var result = new List<LocationRecommendation>(frontLocations);

            // 后排：只有当前排有候选时才保留后排（前排会被优先消耗，不会堵仓）
            // 简化处理：前排存在同地址的 → 后排可用；否则不可用
            var frontAddresses = frontLocations
                .Select(f => $"{f.AisleNo}_{f.Side}_{f.RowNo}_{f.ColNo}_{f.LayerNo}")
                .ToHashSet();

            foreach (var back in backLocations)
            {
                // 前排同地址存在 → 后排可用（前排会被优先使用）
                // 但更安全的做法是只保留后排（允许在流水线中，后排总是可能被使用）
                result.Add(back);
            }

            // 按前排优先排序
            return result.OrderBy(l => l.Depth ?? 1).ToList();
        }

        private List<LocationRecommendation> ApplyBackFirstToCandidates(
            List<LocationRecommendation> frontLocations, List<LocationRecommendation> backLocations)
        {
            var result = new List<LocationRecommendation>(backLocations);
            result.AddRange(frontLocations);
            return result;
        }

        private List<LocationRecommendation> ApplyPairToCandidates(
            List<LocationRecommendation> frontLocations, List<LocationRecommendation> backLocations)
        {
            var frontByAddress = frontLocations
                .GroupBy(f => $"{f.AisleNo}_{f.Side}_{f.RowNo}_{f.ColNo}_{f.LayerNo}")
                .ToDictionary(g => g.Key, g => g.ToList());

            var result = new List<LocationRecommendation>();

            foreach (var back in backLocations)
            {
                var address = $"{back.AisleNo}_{back.Side}_{back.RowNo}_{back.ColNo}_{back.LayerNo}";
                if (frontByAddress.TryGetValue(address, out var fronts))
                {
                    // 前后排都有 → 成对推荐
                    result.AddRange(fronts.OrderBy(f => f.Depth));
                    result.Add(back);
                    frontByAddress.Remove(address);
                }
                else
                {
                    // 只有后排 → 保留（流水线中上游已过滤过）
                    result.Add(back);
                }
            }

            // 剩余前排
            foreach (var fronts in frontByAddress.Values)
                result.AddRange(fronts);

            return result;
        }

        /// <summary>
        /// 从数据库查询（链中第一个策略）
        /// </summary>
        private async Task<LocationAllocationResult> QueryFromDatabaseAsync(
            long warehouseId, ILocationQueryService locationService,
            string mode, IPolicyContext context, CancellationToken cancellationToken)
        {
            var zoneId = context.ZoneId;

            // 获取该仓库/库区下的所有货位（含非空闲），用于双深约束判断
            var allLocations = zoneId.HasValue
                ? await locationService.GetLocationsByZoneAsync(zoneId.Value, cancellationToken)
                : await locationService.GetEmptyLocationsAsync(warehouseId, cancellationToken: cancellationToken);

            // 分离空闲和非空闲货位
            var emptyLocations = allLocations
                .Where(l => l.Status == AlgoConstants.LocationStatus.EMPTY && l.LockStatus == AlgoConstants.LocationLockStatus.NONE && l.IsDisabled == AlgoConstants.BoolFlag.NO)
                .ToList();

            var occupiedLocationCodes = allLocations
                .Where(l => l.Status == AlgoConstants.LocationStatus.OCCUPIED)
                .Select(l => l.LocationCode)
                .ToHashSet();

            if (emptyLocations.Count == 0)
                return new LocationAllocationResult();

            // 应用双深约束过滤
            var filteredLocations = mode switch
            {
                AlgoConstants.DoubleDeepMode.FRONT_FIRST => FilterFrontFirst(emptyLocations, allLocations),
                AlgoConstants.DoubleDeepMode.BACK_FIRST => FilterBackFirst(emptyLocations, allLocations),
                AlgoConstants.DoubleDeepMode.PAIR => FilterPairMode(emptyLocations, allLocations),
                _ => FilterFrontFirst(emptyLocations, allLocations)
            };

            // 构建推荐列表
            var rawLocations = filteredLocations.Select(loc => new LocationRecommendation
            {
                LocationId = loc.Id.ToString(),
                LocationCode = loc.LocationCode,
                AisleNo = loc.AisleNo,
                RowNo = loc.RowNo,
                ColNo = loc.ColNo,
                LayerNo = loc.LayerNo,
                Depth = loc.Depth,
                Side = loc.Side,
                ZoneCode = loc.ZoneCode,
                Score = CalculateScore(loc, mode),
                Reason = $"双深分配-{mode}，深度={loc.Depth}"
            }).ToList();

            // 后处理：可配置排序 → 数量截断（不再重复双深过滤，因为已经在上面处理了）
            var processed = rawLocations.AsEnumerable();

            // 可配置排序
            var sortRules = ParseSortRulesFromContext(context);
            if (sortRules != null && sortRules.Count > 0)
            {
                for (var i = sortRules.Count - 1; i >= 0; i--)
                {
                    var rule = sortRules[i];
                    processed = rule.Direction == AlgoConstants.SortDirection.DESC
                        ? processed.OrderByDescending(l => GetFieldValue(l, rule.Field))
                        : processed.OrderBy(l => GetFieldValue(l, rule.Field));
                }
            }
            else
            {
                // 默认排序：低层优先 → 深度优先（前排优先）
                processed = processed
                    .OrderBy(l => l.LayerNo ?? 0)
                    .ThenBy(l => l.Depth ?? 1)
                    .ThenBy(l => l.RowNo ?? 0)
                    .ThenBy(l => l.ColNo ?? 0);
            }

            // 数量截断
            var maxCount = GetMaxRecommendCount(context);
            processed = processed.Take(maxCount);

            return new LocationAllocationResult { Locations = processed.ToList() };
        }

        /// <summary>
        /// FRONT_FIRST 模式过滤：
        /// - 前排空闲 → 可用
        /// - 后排空闲 + 对应前排被占用 → 可用（前排有货物挡着，后排不会阻挡出库）
        /// - 后排空闲 + 对应前排也空闲 → 不可用（放后排会挡住前排）
        /// </summary>
        private List<DTOs.MdLocationDTO> FilterFrontFirst(
            List<DTOs.MdLocationDTO> emptyLocations,
            List<DTOs.MdLocationDTO> allLocations)
        {
            var result = new List<DTOs.MdLocationDTO>();

            // 前排空闲全部可用
            var frontEmpty = emptyLocations.Where(l => l.Depth == AlgoConstants.DepthType.FRONT).ToList();
            result.AddRange(frontEmpty);

            // 后排空闲：检查前排状态
            var backEmpty = emptyLocations.Where(l => l.Depth == AlgoConstants.DepthType.BACK).ToList();
            foreach (var back in backEmpty)
            {
                // 查找对应前排
                var frontPair = allLocations.FirstOrDefault(l =>
                    l.WarehouseId == back.WarehouseId
                    && l.AisleNo == back.AisleNo
                    && l.Side == back.Side
                    && l.RowNo == back.RowNo
                    && l.ColNo == back.ColNo
                    && l.LayerNo == back.LayerNo
                    && l.Depth == AlgoConstants.DepthType.FRONT);

                // 前排被占用 → 后排可用（不会堵住前排）
                if (frontPair != null && frontPair.Status == AlgoConstants.LocationStatus.OCCUPIED)
                {
                    result.Add(back);
                }
                // 前排不存在（单深库区） → 后排直接可用
                else if (frontPair == null)
                {
                    result.Add(back);
                }
                // 前排空闲 → 后排不可用（放后排会堵住前排）
            }

            return result;
        }

        /// <summary>
        /// BACK_FIRST 模式过滤：
        /// - 后排空闲 + 前排被占用 → 优先使用（先进后出）
        /// - 后排空闲 + 前排空闲 → 可用但后排优先
        /// - 前排空闲 → 可用
        /// </summary>
        private List<DTOs.MdLocationDTO> FilterBackFirst(
            List<DTOs.MdLocationDTO> emptyLocations,
            List<DTOs.MdLocationDTO> allLocations)
        {
            var backLocations = new List<DTOs.MdLocationDTO>();
            var frontLocations = new List<DTOs.MdLocationDTO>();

            foreach (var loc in emptyLocations)
            {
                if (loc.Depth == AlgoConstants.DepthType.BACK)
                    backLocations.Add(loc);
                else
                    frontLocations.Add(loc);
            }

            // 后排优先，前排补充
            var result = new List<DTOs.MdLocationDTO>(backLocations);
            result.AddRange(frontLocations);

            return result;
        }

        /// <summary>
        /// PAIR 模式过滤：
        /// - 前后排都空闲 → 成对推荐
        /// - 只有前排空闲 → 推荐前排
        /// - 只有后排空闲 + 前排被占用 → 推荐后排
        /// - 只有后排空闲 + 前排也空闲 → 不推荐后排（会堵住前排）
        /// </summary>
        private List<DTOs.MdLocationDTO> FilterPairMode(
            List<DTOs.MdLocationDTO> emptyLocations,
            List<DTOs.MdLocationDTO> allLocations)
        {
            // 按物理地址分组
            var groups = emptyLocations
                .GroupBy(l => new { l.WarehouseId, l.AisleNo, l.Side, l.RowNo, l.ColNo, l.LayerNo })
                .ToList();

            var result = new List<DTOs.MdLocationDTO>();

            foreach (var group in groups)
            {
                var hasFront = group.Any(l => l.Depth == AlgoConstants.DepthType.FRONT);
                var hasBack = group.Any(l => l.Depth == AlgoConstants.DepthType.BACK);

                if (hasFront && hasBack)
                {
                    // 前后排都空闲，成对推荐，前排排前面
                    result.AddRange(group.OrderBy(l => l.Depth));
                }
                else if (hasFront)
                {
                    // 只有前排空闲
                    result.AddRange(group.Where(l => l.Depth == AlgoConstants.DepthType.FRONT));
                }
                else if (hasBack)
                {
                    // 只有后排空闲，检查前排状态
                    var backLoc = group.First(l => l.Depth == AlgoConstants.DepthType.BACK);
                    var frontPair = allLocations.FirstOrDefault(l =>
                        l.WarehouseId == backLoc.WarehouseId
                        && l.AisleNo == backLoc.AisleNo
                        && l.Side == backLoc.Side
                        && l.RowNo == backLoc.RowNo
                        && l.ColNo == backLoc.ColNo
                        && l.LayerNo == backLoc.LayerNo
                        && l.Depth == AlgoConstants.DepthType.FRONT);

                    // 前排被占用 → 后排可用
                    if (frontPair != null && frontPair.Status == AlgoConstants.LocationStatus.OCCUPIED)
                    {
                        result.Add(backLoc);
                    }
                    // 前排不存在 → 后排可用
                    else if (frontPair == null)
                    {
                        result.Add(backLoc);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 读取分配模式
        /// </summary>
        private string GetMode(IPolicyContext context)
        {
            var paramsJson = GetParamsJson(context);
            if (string.IsNullOrWhiteSpace(paramsJson))
                return AlgoConstants.DoubleDeepMode.FRONT_FIRST;

            try
            {
                var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var doc = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(paramsJson, options);
                // #20：统一只认 DoubleDeepMode（与 StrategyParams 常量一致），移除 "Mode" 别名避免双字段歧义（前者静默覆盖后者）
                if (doc.TryGetProperty(StrategyParams.LocationAllocationInput.DOUBLE_DEEP_MODE, out var modeProp) && modeProp.ValueKind == System.Text.Json.JsonValueKind.String)
                {
                    var mode = modeProp.GetString();
                    if (!string.IsNullOrWhiteSpace(mode))
                        return mode;
                }
            }
            catch { }

            return AlgoConstants.DoubleDeepMode.FRONT_FIRST;
        }

        /// <summary>
        /// 计算评分
        /// </summary>
        private decimal CalculateScore(DTOs.MdLocationDTO loc, string mode)
        {
            var score = 100m;
            score -= loc.LayerNo * 2m;   // 层越低越好

            if (mode == AlgoConstants.DoubleDeepMode.FRONT_FIRST)
                score -= (loc.Depth - 1) * 10m; // 前排大幅优先
            else if (mode == AlgoConstants.DoubleDeepMode.BACK_FIRST)
                score += (loc.Depth - 1) * 10m; // 后排大幅优先

            return Math.Max(score, 0);
        }

        /// <summary>
        /// 从上下文中解析排序规则（简化版，复用基类解析逻辑）
        /// </summary>
        private List<SortField>? ParseSortRulesFromContext(IPolicyContext context)
        {
            var paramsJson = GetParamsJson(context);
            if (string.IsNullOrWhiteSpace(paramsJson))
                return null;

            try
            {
                var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var doc = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(paramsJson, options);
                if (doc.TryGetProperty(StrategyParams.LocationAllocationInput.SORT_RULES, out var sortRulesEl) && sortRulesEl.ValueKind == System.Text.Json.JsonValueKind.Array)
                {
                    return System.Text.Json.JsonSerializer.Deserialize<List<SortField>>(sortRulesEl.GetRawText(), options);
                }
            }
            catch { }

            return null;
        }

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
}
