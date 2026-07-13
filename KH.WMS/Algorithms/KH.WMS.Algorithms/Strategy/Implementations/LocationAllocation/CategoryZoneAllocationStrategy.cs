using KH.WMS.Algorithms.Strategy.Constants;
using KH.WMS.Algorithms.Strategy.Interfaces;
using KH.WMS.Algorithms.Strategy.QueryServices;
using KH.WMS.Algorithms.Strategy.Strategies;

namespace KH.WMS.Algorithms.Strategy.Implementations.LocationAllocation
{
    /// <summary>
    /// 品类分区分配策略
    /// 按物料品类将物料分配到对应品类的专用库区
    /// 通过物料分类ID匹配库区ZoneType，相同品类的物料存放在同一库区
    ///
    /// StepParams JSON 示例：
    /// {
    ///   "CategoryZoneMapping": { "1": "ZONE_ELEC", "2": "ZONE_MECH" },
    ///   "MaxRecommendCount": 20,
    ///   "SortRules": [
    ///     { "Field": "LayerNo", "Direction": "ASC" },
    ///     { "Field": "ColNo", "Direction": "ASC" }
    ///   ],
    ///   "EnableDoubleDeep": true,
    ///   "DoubleDeepMode": "FRONT_FIRST"
    /// }
    /// </summary>
    public class CategoryZoneAllocationStrategy(IWarehouseQueryService warehouseQueryService, ILocationQueryService locationQueryService) : LocationAllocationStrategyBase
    {
        public override string Name => "品类分区分配策略";
        public override string Code => "CATEGORY_ZONE";
        public override string Author => "System";
        public override string Description => "按物料品类分区存放，相同品类的物料分配到同一库区。支持可配置排序、数量限制和双深货位";

        protected override async Task<LocationAllocationResult> AllocateAsync(IPolicyContext context, CancellationToken cancellationToken = default)
        {
            var warehouseService = warehouseQueryService;
            var locationService = locationQueryService;
            if (warehouseService == null || locationService == null)
                return new LocationAllocationResult();

            var warehouseId = context.WarehouseId ?? context.GetData<long>(StrategyParams.Common.WAREHOUSE_ID);
            if (warehouseId <= 0)
                return new LocationAllocationResult();

            // 获取物料分类ID
            var categoryId = context.MaterialCategoryId ?? context.GetData<long?>(StrategyParams.PutawayInput.MATERIAL_CATEGORY_ID);

            // 获取匹配的库区列表
            var matchedZones = await ResolveMatchedZonesAsync(warehouseId, categoryId, warehouseService, context);

            // 流水线模式：从上游候选列表中筛选匹配品类的货位
            var candidates = GetPipelineCandidates(context);
            if (candidates != null)
                return await FilterCandidatesByZone(candidates, matchedZones, context, cancellationToken);

            // 第一个策略：从数据库查询
            if (matchedZones.Count == 0)
                return await FallbackToAllZones(warehouseId, warehouseService, locationService, context, cancellationToken);

            var rawLocations = new List<LocationRecommendation>();
            foreach (var zone in matchedZones)
            {
                var emptyLocations = await locationService.GetEmptyLocationsAsync(
                    warehouseId, zone.Id, cancellationToken: cancellationToken);

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
                        Reason = $"品类分区，库区{zone.ZoneCode}"
                    });
                }

                // #26: 汇总所有匹配库区，不再首个有空货位即 break（交由 PostProcess 排序+截断）
            }

            var processed = await PostProcess(rawLocations, context, cancellationToken);
            return new LocationAllocationResult { Locations = processed };
        }

        /// <summary>
        /// 从上游候选列表中筛选匹配品类的货位（流水线模式）
        /// </summary>
        private async Task<LocationAllocationResult> FilterCandidatesByZone(
            List<LocationRecommendation> candidates,
            List<DTOs.MdWarehouseZoneDTO> matchedZones,
            IPolicyContext context,
            CancellationToken cancellationToken)
        {
            var zoneCodes = matchedZones.Select(z => z.ZoneCode).ToHashSet();

            var filtered = candidates
                .Where(c => zoneCodes.Contains(c.ZoneCode))
                .Select(c =>
                {
                    var clone = CloneRecommendation(c);   // #48：克隆后追加，不污染上游
                    clone.Reason += " → 品类筛选";
                    return clone;
                })
                .ToList();

            if (filtered.Count == 0)
                filtered = candidates;

            var processed = await PostProcess(filtered, context, cancellationToken);
            return new LocationAllocationResult { Locations = processed };
        }

        /// <summary>
        /// 解析匹配的库区列表
        /// </summary>
        private async Task<List<DTOs.MdWarehouseZoneDTO>> ResolveMatchedZonesAsync(
            long warehouseId, long? categoryId,
            IWarehouseQueryService warehouseService, IPolicyContext context)
        {
            var allZones = await warehouseService.GetStorageZonesAsync(warehouseId);

            if (!categoryId.HasValue || categoryId <= 0)
                return allZones;

            var stepParams = GetParamsJson(context);
            var categoryZoneMapping = ParseCategoryZoneMapping(stepParams);

            if (categoryZoneMapping.TryGetValue(categoryId.Value, out var mappedZoneCode))
            {
                var mapped = allZones.Where(z => z.ZoneCode == mappedZoneCode).ToList();
                if (mapped.Count > 0) return mapped;
            }

            return allZones.Where(z => z.ZoneType == $"CATEGORY_{categoryId}").ToList();
        }

        /// <summary>
        /// 回退：从仓库所有库区查找空闲货位
        /// </summary>
        private async Task<LocationAllocationResult> FallbackToAllZones(
            long warehouseId, IWarehouseQueryService warehouseService,
            ILocationQueryService locationService, IPolicyContext context, CancellationToken cancellationToken)
        {
            var zones = await warehouseService.GetZonesByWarehouseAsync(warehouseId, cancellationToken);
            var rawLocations = new List<LocationRecommendation>();

            foreach (var zone in zones)
            {
                var emptyLocations = await locationService.GetEmptyLocationsAsync(
                    warehouseId, zone.Id, cancellationToken: cancellationToken);

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
                        Reason = $"品类分区回退，库区{zone.ZoneCode}"
                    });
                }

                // #26: 汇总所有匹配库区，不再首个有空货位即 break（交由 PostProcess 排序+截断）
            }

            // 后处理：可配置排序 → 双深过滤 → 数量截断
            var processed = await PostProcess(rawLocations, context, cancellationToken);

            return new LocationAllocationResult { Locations = processed };
        }

        /// <summary>
        /// 解析品类-库区映射配置
        /// StepParams JSON: { "CategoryZoneMapping": { "分类ID": "库区编码" } }
        /// </summary>
        private Dictionary<long, string> ParseCategoryZoneMapping(string? stepParams)
        {
            if (string.IsNullOrWhiteSpace(stepParams)) return new();

            try
            {
                var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var dto = System.Text.Json.JsonSerializer.Deserialize<CategoryZoneMappingDto>(stepParams, options);
                return dto?.CategoryZoneMapping ?? new();
            }
            catch
            {
                return new();
            }
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

        private class CategoryZoneMappingDto
        {
            public Dictionary<long, string> CategoryZoneMapping { get; set; } = new();
        }
    }
}
