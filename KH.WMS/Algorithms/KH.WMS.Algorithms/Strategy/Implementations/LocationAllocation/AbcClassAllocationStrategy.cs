using KH.WMS.Algorithms.Strategy.Constants;
using KH.WMS.Algorithms.Strategy.Interfaces;
using KH.WMS.Algorithms.Strategy.QueryServices;
using KH.WMS.Algorithms.Strategy.Strategies;

namespace KH.WMS.Algorithms.Strategy.Implementations.LocationAllocation
{
    /// <summary>
    /// ABC分类货位分配策略
    /// A类物料 → 靠近出入口的高频区（HIGH_FREQ）
    /// B类物料 → 中频区（MID_FREQ）
    /// C类物料 → 远离出入口的低频区（LOW_FREQ）
    ///
    /// StepParams JSON 示例：
    /// {
    ///   "MaxRecommendCount": 20,
    ///   "SortRules": [
    ///     { "Field": "LayerNo", "Direction": "ASC" },
    ///     { "Field": "ColNo", "Direction": "ASC" }
    ///   ],
    ///   "EnableDoubleDeep": true,
    ///   "DoubleDeepMode": "FRONT_FIRST"
    /// }
    /// </summary>
    public class AbcClassAllocationStrategy(IWarehouseQueryService warehouseQueryService, ILocationQueryService locationQueryService) : LocationAllocationStrategyBase
    {
        public override string Name => "ABC分类分配策略";
        public override string Code => "ABC_CLASS";
        public override string Author => "System";
        public override string Description => "根据物料ABC分类分配货位，A类靠近出入口，C类远离出入口。支持可配置排序、数量限制和双深货位";

        protected override async Task<LocationAllocationResult> AllocateAsync(IPolicyContext context, CancellationToken cancellationToken = default)
        {
            // 流水线模式：如果上游已有候选列表，从中筛选匹配ABC分类库区的货位
            var candidates = GetPipelineCandidates(context);
            if (candidates != null)
                return await FilterCandidatesAsync(candidates, context, cancellationToken);

            // 第一个策略：从数据库查询
            return await QueryFromDatabaseAsync(context, cancellationToken);
        }

        /// <summary>
        /// 从上游候选列表中筛选匹配ABC分类库区的货位
        /// </summary>
        private async Task<LocationAllocationResult> FilterCandidatesAsync(
            List<LocationRecommendation> candidates, IPolicyContext context, CancellationToken cancellationToken)
        {
            var warehouseService = warehouseQueryService;
            if (warehouseService == null)
                return new LocationAllocationResult { Locations = candidates };

            var warehouseId = context.WarehouseId ?? context.GetData<long>(StrategyParams.Common.WAREHOUSE_ID);
            if (warehouseId <= 0)
                return new LocationAllocationResult { Locations = candidates };

            var abcClass = await ResolveAbcClassAsync(warehouseService, context, cancellationToken);
            var zones = await warehouseService.GetZonesByAbcClassAsync(warehouseId, abcClass, cancellationToken);
            var zoneCodes = zones.Select(z => z.ZoneCode).ToHashSet();

            // 筛选属于ABC匹配库区的候选货位（#48：克隆后再追加 Reason，不污染上游共享对象）
            var filtered = candidates
                .Where(c => zoneCodes.Contains(c.ZoneCode))
                .Select(c =>
                {
                    var clone = CloneRecommendation(c);
                    clone.Reason += $" → ABC筛选({abcClass})";
                    return clone;
                })
                .ToList();

            // 如果筛选后为空，保留原始候选（不强制过滤）
            if (filtered.Count == 0)
                filtered = candidates;

            var processed = await PostProcess(filtered, context, cancellationToken);
            return new LocationAllocationResult { Locations = processed };
        }

        /// <summary>
        /// 从数据库查询（链中第一个策略）
        /// </summary>
        private async Task<LocationAllocationResult> QueryFromDatabaseAsync(IPolicyContext context, CancellationToken cancellationToken)
        {
            var warehouseService = warehouseQueryService;
            var locationService = locationQueryService;
            if (warehouseService == null || locationService == null)
                return new LocationAllocationResult();

            var warehouseId = context.WarehouseId ?? context.GetData<long>(StrategyParams.Common.WAREHOUSE_ID);
            if (warehouseId <= 0)
                return new LocationAllocationResult();

            var abcClass = await ResolveAbcClassAsync(warehouseService, context, cancellationToken);

            // 根据ABC分类获取匹配的库区列表
            var zones = await warehouseService.GetZonesByAbcClassAsync(warehouseId, abcClass, cancellationToken);
            if (zones.Count == 0)
                return new LocationAllocationResult();

            // 从匹配的库区中查找空闲货位
            var rawLocations = new List<LocationRecommendation>();
            foreach (var zone in zones)
            {
                var emptyLocations = await locationService.GetEmptyLocationsAsync(
                    warehouseId, zone.Id, cancellationToken: cancellationToken);

                foreach (var loc in emptyLocations)
                {
                    // 货位级 ABC 优先：货位已配置 AbcClass 且与物料 ABC 不符则跳过；
                    // 货位未配置 AbcClass 时沿用库区级匹配（库区已由 GetZonesByAbcClassAsync 按 ABC 筛选）
                    if (!string.IsNullOrWhiteSpace(loc.AbcClass)
                        && !string.Equals(loc.AbcClass, abcClass, StringComparison.OrdinalIgnoreCase))
                        continue;

                    rawLocations.Add(new LocationRecommendation
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
                        Score = CalculateScore(loc, abcClass),
                        Reason = $"ABC分类{abcClass}，库区{zone.ZoneCode}"
                    });
                }

                // #26: 汇总所有 ABC 匹配库区，不再首个有空货位即 break（交由 PostProcess 排序+截断）
            }

            var processed = await PostProcess(rawLocations, context, cancellationToken);
            return new LocationAllocationResult { Locations = processed };
        }

        /// <summary>
        /// 解析物料ABC分类
        /// </summary>
        private async Task<string> ResolveAbcClassAsync(
            IWarehouseQueryService warehouseService, IPolicyContext context, CancellationToken cancellationToken)
        {
            var materialId = context.MaterialId ?? context.GetData<long>(StrategyParams.Common.MATERIAL_ID);
            string abcClass = "C";
            if (materialId > 0)
            {
                var classCode = await warehouseService.GetMaterialTurnoverClassAsync(materialId, cancellationToken);
                if (!string.IsNullOrWhiteSpace(classCode))
                    abcClass = classCode;
            }

            var overrideClass = context.GetData<string>(StrategyParams.LocationAllocationInput.MATERIAL_ABC_CLASS);
            if (!string.IsNullOrWhiteSpace(overrideClass))
                abcClass = overrideClass;

            return abcClass;
        }

        /// <summary>
        /// 计算货位评分：低层高分、前排高分
        /// </summary>
        private decimal CalculateScore(DTOs.MdLocationDTO loc, string abcClass)
        {
            // 基础分100，低层加分，前排加分
            var score = 100m;
            score -= loc.LayerNo * 2m;   // 层越低越好
            score -= (loc.Depth - 1) * 5m; // 前排优先
            return Math.Max(score, 0);
        }
    }
}
