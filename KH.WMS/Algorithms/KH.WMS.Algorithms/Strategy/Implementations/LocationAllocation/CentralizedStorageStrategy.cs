using KH.WMS.Algorithms.Strategy.Constants;
using KH.WMS.Algorithms.Strategy.Interfaces;
using KH.WMS.Algorithms.Strategy.QueryServices;
using KH.WMS.Algorithms.Strategy.Strategies;

namespace KH.WMS.Algorithms.Strategy.Implementations.LocationAllocation
{
    /// <summary>
    /// 集中存储分配策略
    /// 相同物料/批次的库存尽量集中存放在相邻货位
    /// 优先推荐已有同物料库存位置附近的空闲货位
    ///
    /// StepParams JSON 示例：
    /// {
    ///   "MaxNearbyCount": 10,
    ///   "MaxRecommendCount": 20,
    ///   "EnableDoubleDeep": true,
    ///   "DoubleDeepMode": "FRONT_FIRST"
    /// }
    /// </summary>
    public class CentralizedStorageStrategy(ILocationQueryService locationQueryService) : LocationAllocationStrategyBase()
    {
        public override string Name => "集中存储分配策略";
        public override string Code => "CENTRALIZED";
        public override string Author => "System";
        public override string Description => "相同物料/批次集中存放，优先分配已有同品类库存的库位附近。支持可配置排序、数量限制和双深货位";

        protected override async Task<LocationAllocationResult> AllocateAsync(IPolicyContext context, CancellationToken cancellationToken = default)
        {
            var locationService = locationQueryService;
            if (locationService == null)
                return new LocationAllocationResult();

            var warehouseId = context.WarehouseId ?? context.GetData<long>(StrategyParams.Common.WAREHOUSE_ID);
            var materialId = context.MaterialId ?? context.GetData<long>(StrategyParams.Common.MATERIAL_ID);
            if (warehouseId <= 0 || materialId <= 0)
                return new LocationAllocationResult();

            var batchNo = context.GetData<string>(StrategyParams.LocationAllocationInput.TARGET_BATCH_NO);
            var maxNearbyCount = context.GetData<int?>(StrategyParams.LocationAllocationInput.MAX_NEARBY_COUNT) ?? 10;

            // 查询已有同物料（+批次）库存的货位
            var existingLocationCodes = await locationService.GetLocationCodesWithInventoryAsync(
                warehouseId, materialId, batchNo, cancellationToken);

            // 流水线模式：始终基于上游候选列表（#24：无已有库存时不再丢弃候选另起查库）
            var candidates = GetPipelineCandidates(context);
            if (candidates != null)
            {
                if (existingLocationCodes.Count > 0)
                    return await FilterCandidatesByProximityAsync(candidates, existingLocationCodes, locationService, warehouseId, context, cancellationToken);

                // 无已有库存：直接对上游候选做后处理（排序/双深/截断）
                var processedNoExisting = await PostProcess(candidates, context, cancellationToken);
                return new LocationAllocationResult { Locations = processedNoExisting };
            }

            // 链首（无上游候选）：从数据库查询
            if (existingLocationCodes.Count == 0)
            {
                return await FallbackToAnyEmpty(warehouseId, locationService, context, cancellationToken);
            }

            var rawLocations = new List<LocationRecommendation>();

            // 对每个已有库存位置，查找附近的空闲货位
            foreach (var locationCode in existingLocationCodes)
            {
                var nearbyLocations = await locationService.GetLocationsNearAsync(
                    locationCode, warehouseId, maxNearbyCount, cancellationToken);

                foreach (var loc in nearbyLocations)
                {
                    if (rawLocations.Any(r => r.LocationCode == loc.LocationCode))
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
                        Score = CalculateScore(loc),
                        Reason = $"集中存储，靠近已有库存{locationCode}"
                    });
                }
            }

            var processed = await PostProcess(rawLocations, context, cancellationToken);
            return new LocationAllocationResult { Locations = processed };
        }

        /// <summary>
        /// 从上游候选列表中筛选靠近已有库存的货位（流水线模式）
        /// </summary>
        private async Task<LocationAllocationResult> FilterCandidatesByProximityAsync(
            List<LocationRecommendation> candidates,
            List<string> existingLocationCodes,
            ILocationQueryService locationService,
            long warehouseId,
            IPolicyContext context,
            CancellationToken cancellationToken)
        {
            // #23：按巷道号(AisleNo)判定就近，替代原先的字符串启发式(Reason.Contains/StartsWith 命中率低/前缀碰撞)
            var existingAisles = new HashSet<int>();
            foreach (var code in existingLocationCodes)
            {
                var parts = code.Split('-');
                if (parts.Length >= 2 && int.TryParse(parts[1], out var aisleNo))
                    existingAisles.Add(aisleNo);
            }

            var result = new List<LocationRecommendation>();

            foreach (var candidate in candidates)
            {
                var rec = candidate;
                // 同巷道视为就近（#48：仅当需要加分时克隆，避免污染上游共享对象）
                if (candidate.AisleNo.HasValue && existingAisles.Contains(candidate.AisleNo.Value))
                {
                    rec = CloneRecommendation(candidate);
                    rec.Score += 50m; // 就近加分
                    rec.Reason += " → 集中存储加分(同巷道)";
                }

                result.Add(rec);
            }

            // 按分数降序排列（就近的排前面）
            result = result.OrderByDescending(r => r.Score).ToList();

            var processed = await PostProcess(result, context, cancellationToken);
            return new LocationAllocationResult { Locations = processed };
        }

        /// <summary>
        /// 回退：查找仓库内任意空闲货位
        /// </summary>
        private async Task<LocationAllocationResult> FallbackToAnyEmpty(
            long warehouseId, ILocationQueryService locationService, IPolicyContext context, CancellationToken cancellationToken)
        {
            var emptyLocations = await locationService.GetEmptyLocationsAsync(warehouseId, cancellationToken: cancellationToken);
            var rawLocations = new List<LocationRecommendation>();

            foreach (var loc in emptyLocations)
            {
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
                    Score = CalculateScore(loc),
                    Reason = "集中存储回退，无已有库存"
                });
            }

            // 后处理：可配置排序 → 双深过滤 → 数量截断
            var processed = await PostProcess(rawLocations, context, cancellationToken);

            return new LocationAllocationResult { Locations = processed };
        }

        /// <summary>
        /// 计算货位评分
        /// </summary>
        private decimal CalculateScore(DTOs.MdLocationDTO loc)
        {
            var score = 100m;
            score -= loc.LayerNo * 2m;
            score -= (loc.Depth - 1) * 5m;
            return Math.Max(score, 0);
        }
    }
}
