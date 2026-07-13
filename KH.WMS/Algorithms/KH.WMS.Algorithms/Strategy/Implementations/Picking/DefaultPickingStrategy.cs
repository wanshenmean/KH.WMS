using KH.WMS.Algorithms.Strategy.Constants;
using KH.WMS.Algorithms.Strategy.Interfaces;
using KH.WMS.Algorithms.Strategy.Optimizers;
using KH.WMS.Algorithms.Strategy.QueryServices;
using KH.WMS.Algorithms.Strategy.Strategies;

namespace KH.WMS.Algorithms.Strategy.Implementations.Picking
{
    /// <summary>
    /// 默认下架策略
    /// 控制下架作业流程：调度库存分配（选哪些库存）→ 确定取货路径 → 确定目标出口/站台 → 生成送达路径
    /// </summary>
    public class DefaultPickingStrategy(IWarehouseQueryService warehouseQueryService) : PickingStrategyBase
    {
        public override string Name => "默认下架策略";
        public override string Code => "DEFAULT_PICKING";
        public override string Author => "System";
        public override string Description => "高位立库默认下架策略，支持整托优先、路径优化、自动分配目标出口";

        protected override async Task<PickingResult> ExecutePickingAsync(IPolicyContext context, CancellationToken cancellationToken = default)
        {
            var result = new PickingResult
            {
                RequireInventoryAllocation = true
            };

            // 从上下文获取库存分配结果，转换为下架任务
            var allocationResult = context.GetData<InventoryAllocationResult>(StrategyParams.InventoryAllocationOutput.RESULT);
            if (allocationResult != null && allocationResult.Items.Any())
            {
                foreach (var item in allocationResult.Items)
                {
                    result.Tasks.Add(new PickingTaskItem
                    {
                        FromLocationId = item.LocationId,
                        FromLocationCode = item.LocationCode,
                        ContainerCode = item.ContainerCode,
                        MaterialId = item.MaterialId,
                        BatchNo = item.BatchNo,
                        SerialNo = item.SerialNo,
                        Qty = item.AllocatedQty,
                        Priority = item.Priority,
                        IsFullPallet = item.AllocatedQty >= item.AvailableQty, // #38：仅当取走整库位库存时才标整托
                        InventoryDetailId = item.InventoryDetailId,
                        InventoryHeaderId = item.InventoryHeaderId
                    });
                }
            }

            // 目标出口/站台（优先使用上下文传入的值）
            result.DestinationStationId = context.GetData<long>(StrategyParams.PickingInput.DESTINATION_STATION_ID);
            result.DestinationStationCode = context.GetData<string>(StrategyParams.PickingInput.DESTINATION_STATION_CODE);
            result.DestinationZoneId = context.GetData<long>(StrategyParams.PickingInput.DESTINATION_ZONE_ID);
            result.DestinationZoneCode = context.GetData<string>(StrategyParams.PickingInput.DESTINATION_ZONE_CODE);

            // 自动匹配目标出口（仅在未指定时执行）
            if ((result.DestinationStationId == null || result.DestinationStationId <= 0) && string.IsNullOrWhiteSpace(result.DestinationStationCode))
            {
                await AutoMatchDestinationAsync(context, result, cancellationToken);
            }

            result.PathOptimization = context.GetData<string>(StrategyParams.Common.PATH_OPTIMIZATION) ?? AlgoConstants.PathOptimization.S_SHAPE;

            // Step 3: 生成取货路径（按任务优先级排序的货位编码序列）
            GeneratePickRoute(result);

            // Step 4: 生成送达路径（从最后一个取货位到目标出口）
            GenerateDeliveryRoute(result);

            // 传递给下游库存分配策略的参数
            result.AllocationParameters[StrategyParams.PickingInput.ENABLE_PALLET_FIRST] = context.GetData<bool?>(StrategyParams.PickingInput.ENABLE_PALLET_FIRST) ?? true;

            return result;
        }

        /// <summary>
        /// 自动匹配目标出口
        /// 优先通过 CfgDocTypePort 配置匹配，未配置时根据源库区查找出库口
        /// </summary>
        private async Task AutoMatchDestinationAsync(IPolicyContext context, PickingResult result,
            CancellationToken cancellationToken)
        {
            var warehouseService = warehouseQueryService;
            if (warehouseService == null) return;

            var warehouseId = context.WarehouseId ?? context.GetData<long>(StrategyParams.Common.WAREHOUSE_ID);
            if (warehouseId <= 0) return;

            // 优先通过 DocTypePort 配置匹配
            var docTypeId = context.GetData<long?>(StrategyParams.PickingInput.DOC_TYPE_ID);
            if (docTypeId.HasValue && docTypeId.Value > 0)
            {
                var port = await warehouseService.GetPortByDocTypeAsync(
                    docTypeId.Value, AlgoConstants.PortDirection.OUTBOUND, warehouseId, cancellationToken);

                if (port != null)
                {
                    result.DestinationStationId = port.Id;
                    result.DestinationStationCode = port.PortCode;
                    if (port.ZoneId.HasValue)
                        result.DestinationZoneId = port.ZoneId.Value;
                    return;
                }
            }

            // 降级：根据源巷道号查找出库口（巷道 → 接驳口 → 输送线 → 站台）
            var sourceAisleNo = context.GetData<int>(StrategyParams.PickingInput.SOURCE_AISLE_NO);
            if (sourceAisleNo > 0)
            {
                var port = await warehouseService.GetOutboundPortByAisleAsync(warehouseId, sourceAisleNo, cancellationToken);
                if (port != null)
                {
                    result.DestinationStationId = port.Id;
                    result.DestinationStationCode = port.PortCode;
                    if (port.ZoneId.HasValue)
                        result.DestinationZoneId = port.ZoneId.Value;
                }
            }
        }

        /// <summary>
        /// 生成取货路径
        /// 使用堆垛机路径优化器对下架任务排序，生成最优取货行走序列
        /// </summary>
        private void GeneratePickRoute(PickingResult result)
        {
            if (!result.Tasks.Any()) return;

            var pathMode = result.PathOptimization ?? AlgoConstants.PathOptimization.S_SHAPE;

            // 使用堆垛机路径优化器排序（ASRS场景：水平优先于垂直，按巷道分组）
            var sorted = CranePathOptimizer.Optimize(result.Tasks, pathMode);

            // 同步更新任务列表和取货路径
            result.Tasks.Clear();
            result.Tasks.AddRange(sorted);
            result.PickRoute = sorted.Select(t => t.FromLocationCode).ToList();
        }

        /// <summary>
        /// 生成送达路径
        /// 从最后一个取货位到目标出口的路径
        /// </summary>
        private void GenerateDeliveryRoute(PickingResult result)
        {
            result.DeliveryRoute.Clear();

            if (!result.PickRoute.Any() || string.IsNullOrWhiteSpace(result.DestinationStationCode))
                return;

            // 送达路径：最后一个取货位 → 目标出口
            var lastPickLocation = result.PickRoute.Last();
            result.DeliveryRoute.Add(lastPickLocation);
            result.DeliveryRoute.Add(result.DestinationStationCode);
        }
    }
}
