using KH.WMS.Algorithms.Strategy.Constants;
using KH.WMS.Algorithms.Strategy.Interfaces;
using KH.WMS.Algorithms.Strategy.QueryServices;
using KH.WMS.Algorithms.Strategy.Strategies;

namespace KH.WMS.Algorithms.Strategy.Implementations.InventoryAllocation
{
    /// <summary>
    /// 按批次出库策略
    /// 优先分配指定批次的库存，适用于有批次追溯要求的场景
    /// 指定批次不足时，按FIFO从其他批次补充；全仓查询不预选分区（A2）
    /// </summary>
    public class BatchStrategy(IInventoryQueryService inventoryQueryService) : InventoryAllocationStrategyBase()
    {
        public override string Name => "按批次出库策略";
        public override string Code => "BATCH";
        public override string Author => "System";
        public override string Description => "按批次号分配库存，优先分配指定批次的库存，不足时按FIFO补充";

        protected override async Task<InventoryAllocationResult> SelectInventoryAsync(IPolicyContext context, CancellationToken cancellationToken = default)
        {
            var inventoryService = inventoryQueryService;
            var warehouseCode = context.GetData<string>(StrategyParams.InventoryAllocationInput.WAREHOUSE_CODE) ?? context.WarehouseCode ?? string.Empty;
            var materialCode = context.GetData<string>(StrategyParams.InventoryAllocationInput.MATERIAL_CODE) ?? string.Empty;
            var requiredQty = context.GetData<decimal>(StrategyParams.InventoryAllocationInput.REQUIRED_QTY);
            var targetBatchNo = context.GetData<string>(StrategyParams.InventoryAllocationInput.TARGET_BATCH_NO);

            // #40：必填服务/参数缺失属配置错误，抛异常由基类转为 Failure
            if (inventoryService == null || string.IsNullOrWhiteSpace(warehouseCode) || string.IsNullOrWhiteSpace(materialCode))
                throw new InvalidOperationException("库存分配缺少必填项(IInventoryQueryService/WarehouseCode/MaterialCode)");

            if (requiredQty <= 0)
                return new InventoryAllocationResult();

            var result = new InventoryAllocationResult();
            var remaining = requiredQty;
            var priority = 1;

            // A2: 全仓查询，不再由 OutboundAllocation 预选分区。
            // 第一步：如果有指定批次，优先分配该批次库存
            if (!string.IsNullOrWhiteSpace(targetBatchNo))
            {
                var batchInventory = await inventoryService.GetByBatchAsync(warehouseCode, materialCode, targetBatchNo, cancellationToken);
                batchInventory = ApplyPreferences(batchInventory, remaining, context);
                remaining = FillAllocation(result, batchInventory, remaining, ref priority, $"指定批次（{targetBatchNo}）");
            }

            // 第二步：指定批次不够时，按FIFO从其他批次补充
            if (remaining > 0)
            {
                var fifoInventory = await inventoryService.GetByFIFOAsync(warehouseCode, materialCode, cancellationToken);
                fifoInventory = ApplyPreferences(fifoInventory, remaining, context);
                // 排除已分配的指定批次；targetBatchNo 为空时不排除任何库存（含无批次库存）
                var supplementList = string.IsNullOrWhiteSpace(targetBatchNo)
                    ? fifoInventory
                    : fifoInventory.Where(inv => inv.BatchNo != targetBatchNo).ToList();
                FillAllocation(result, supplementList, remaining, ref priority, "FIFO补充");
            }

            return result;
        }

        /// <summary>
        /// 填充库存分配结果，返回剩余未分配数量
        /// </summary>
        private decimal FillAllocation(InventoryAllocationResult result,
            List<Strategy.DTOs.InventoryInfoDTO> inventoryList,
            decimal remaining, ref int priority, string reason)
        {
            foreach (var inv in inventoryList)
            {
                if (remaining <= 0) break;

                var availableQty = inv.Qty - inv.LockedQty;
                if (availableQty <= 0) continue;

                var allocateQty = Math.Min(availableQty, remaining);

                result.Items.Add(new InventoryAllocationItem
                {
                    InventoryDetailId = inv.Id,
                    InventoryHeaderId = inv.HeaderId,
                    LocationId = inv.LocationId?.ToString() ?? string.Empty,
                    LocationCode = inv.LocationCode ?? string.Empty,
                    ContainerCode = inv.ContainerCode,
                    MaterialId = inv.MaterialId,
                    BatchNo = inv.BatchNo,
                    SerialNo = inv.SerialNo,
                    AvailableQty = availableQty,
                    AllocatedQty = allocateQty,
                    ExpiryDate = inv.ExpiryDate,
                    ManufactureDate = inv.ProductionDate,
                    InboundTime = inv.InboundTime,
                    Priority = priority++,
                    Reason = reason
                });

                remaining -= allocateQty;
            }

            return remaining;
        }
    }
}
