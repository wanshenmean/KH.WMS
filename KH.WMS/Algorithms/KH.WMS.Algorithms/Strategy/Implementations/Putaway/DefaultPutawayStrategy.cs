using KH.WMS.Algorithms.Strategy.Constants;
using KH.WMS.Algorithms.Strategy.Interfaces;
using KH.WMS.Algorithms.Strategy.QueryServices;
using KH.WMS.Algorithms.Strategy.Strategies;

namespace KH.WMS.Algorithms.Strategy.Implementations.Putaway
{
    /// <summary>
    /// 默认上架策略
    /// 控制上架作业流程：确定入库口 → 分配库区 → 分配巷道 → 生成路径 → 调度货位分配
    /// </summary>
    public class DefaultPutawayStrategy(IWarehouseQueryService warehouseQueryService) : PutawayStrategyBase
    {
        public override string Name => "默认上架策略";
        public override string Code => "DEFAULT_PUTAWAY";
        public override string Author => "System";
        public override string Description => "默认上架策略，按单据类型匹配入库口、库区分区、ABC分类、就近上架规则执行上架作业，自动分配巷道和生成路径";

        protected override async Task<PutawayResult> ExecutePutawayAsync(IPolicyContext context, CancellationToken cancellationToken = default)
        {
            var result = new PutawayResult
            {
                RequireLocationAllocation = true
            };

            // 策略参数
            var enableZonePartition = context.GetData<bool?>(StrategyParams.PutawayInput.ENABLE_ZONE_PARTITION) ?? true;
            var enableAbcClass = context.GetData<bool?>(StrategyParams.PutawayInput.ENABLE_ABC_CLASS) ?? true;
            result.PathOptimization = context.GetData<string>(StrategyParams.Common.PATH_OPTIMIZATION) ?? AlgoConstants.PathOptimization.S_SHAPE;

            var warehouseService = warehouseQueryService;
            var warehouseId = context.WarehouseId ?? context.GetData<long>(StrategyParams.Common.WAREHOUSE_ID);
            if (warehouseId <= 0)
                return result;

            // Step 1: 确定入库口（#5 修正：优先使用调用方显式传入的入库口，其次按单据类型自动匹配）
            var docTypeId = context.GetData<long?>(StrategyParams.PutawayInput.DOC_TYPE_ID);
            if (warehouseService != null)
            {
                // 优先：上下文显式传入的入库口（InboundStationId/Code）
                result.InboundStationId = context.GetData<string>(StrategyParams.PutawayInput.INBOUND_STATION_ID);
                result.InboundStationCode = context.GetData<string>(StrategyParams.PutawayInput.INBOUND_STATION_CODE);

                // 回退：按单据类型自动匹配
                if (string.IsNullOrEmpty(result.InboundStationId) && docTypeId.HasValue && docTypeId.Value > 0)
                {
                    var port = await warehouseService.GetPortByDocTypeAsync(
                        docTypeId.Value, AlgoConstants.PortDirection.INBOUND, warehouseId, cancellationToken);
                    if (port != null)
                    {
                        result.InboundStationId = port.Id.ToString();
                        result.InboundStationCode = port.PortCode;
                    }
                }
            }

            // Step 2: 确定目标库区（ABC分类匹配 → 回退到默认库区）
            // #22：移除原先按 ZoneType=="CATEGORY_{id}" 的分类匹配（合法 ZoneType 无此类型、永不命中）；
            //      品类级货位分配由 CategoryZoneAllocationStrategy 在货位分配阶段处理。
            if (enableZonePartition && warehouseService != null)
            {
                var materialId = context.MaterialId ?? context.GetData<long>(StrategyParams.Common.MATERIAL_ID);

                // 根据ABC分类匹配库区
                if (result.TargetZoneId == null && enableAbcClass && materialId > 0)
                {
                    var abcClass = await warehouseService.GetMaterialTurnoverClassAsync(materialId, cancellationToken);
                    if (!string.IsNullOrWhiteSpace(abcClass))
                    {
                        var abcZones = await warehouseService.GetZonesByAbcClassAsync(warehouseId, abcClass, cancellationToken);
                        if (abcZones.Count > 0)
                        {
                            result.TargetZoneId = abcZones.First().Id;
                            result.TargetZoneCode = abcZones.First().ZoneCode;
                        }
                    }
                }

                // 最后回退到仓库第一个存储类库区
                if (result.TargetZoneId == null)
                {
                    var fallbackZones = await warehouseService.GetStorageZonesAsync(warehouseId, cancellationToken);
                    if (fallbackZones.Count > 0)
                    {
                        result.TargetZoneId = fallbackZones.First().Id;
                        result.TargetZoneCode = fallbackZones.First().ZoneCode;
                    }
                }
            }

            // Step 3: 在目标库区内计算最优巷道（根据库区负载）
            if (warehouseService != null)
            {
                var leastLoadedAisle = await warehouseService.GetLeastLoadedAisleAsync(
                    warehouseId, result.TargetZoneId, cancellationToken);

                if (leastLoadedAisle != null)
                {
                    result.AisleId = leastLoadedAisle.Id;
                    result.AisleCode = leastLoadedAisle.AisleCode;
                }
            }

            // Step 3.5: 如果尚未确定入库口，通过巷道 → 接驳口 → 输送线 → 入库口 关联链反推
            if (string.IsNullOrEmpty(result.InboundStationId) && result.AisleId.HasValue && warehouseService != null)
            {
                // 通过巷道找接驳口
                var transferPoint = await warehouseService.GetInboundTransferPointAsync(
                    warehouseId, result.AisleId.Value, cancellationToken);

                if (transferPoint != null)
                {
                    // 通过接驳口的输送线找入库口
                    var port = await warehouseService.GetInboundPortByConveyorAsync(
                        warehouseId, transferPoint.ConveyorLineId, cancellationToken);

                    if (port != null)
                    {
                        result.InboundStationId = port.Id.ToString();
                        result.InboundStationCode = port.PortCode;
                    }
                }
            }

            // Step 4: 生成上架路径（入库口 → 巷道）
            if (!string.IsNullOrEmpty(result.InboundStationCode) && !string.IsNullOrEmpty(result.AisleCode))
            {
                result.Route.Clear();
                result.Route.Add(result.InboundStationCode);
                result.Route.Add(result.AisleCode);
            }

            // #6: AllocationParameters 此前写入但下游货位分配策略从不读取（调用方通过 context.ZoneId /
            // PutawayInput.TARGET_ZONE_ID 单独传参），已移除该死数据。TargetZoneId/AisleId 仍在 PutawayResult 上供调用方使用。

            return result;
        }
    }
}
