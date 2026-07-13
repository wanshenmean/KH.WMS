using KH.WMS.Algorithms.Strategy.Constants;
using KH.WMS.Algorithms.Strategy.Interfaces;
using KH.WMS.Algorithms.Strategy.QueryServices;
using KH.WMS.Algorithms.Strategy.Strategies;

namespace KH.WMS.Algorithms.Strategy.Implementations.InventoryAllocation
{
    /// <summary>
    /// 库位利用率优先策略
    /// 优先清空可用量少的库位，提升仓库整体利用率
    /// 排序规则：可用量升序（优先清空少量库存的库位）→ 入库时间升序（FIFO 兜底）
    /// 适用于高位立库中需要腾出货位的场景
    /// </summary>
    public class UtilizationPriorityStrategy(IInventoryQueryService inventoryQueryService) : InventoryAllocationStrategyBase()
    {
        public override string Name => "库位利用率优先策略";
        public override string Code => "UTILIZATION_PRIORITY";
        public override string Author => "System";
        public override string Description => "优先清空低库存库位，提升仓库空间利用率，FIFO兜底";

        protected override async Task<InventoryAllocationResult> SelectInventoryAsync(IPolicyContext context, CancellationToken cancellationToken = default)
        {
            var inventoryService = inventoryQueryService;
            var warehouseCode = context.GetData<string>(StrategyParams.InventoryAllocationInput.WAREHOUSE_CODE) ?? context.WarehouseCode ?? string.Empty;
            var materialCode = context.GetData<string>(StrategyParams.InventoryAllocationInput.MATERIAL_CODE) ?? string.Empty;
            var requiredQty = context.GetData<decimal>(StrategyParams.InventoryAllocationInput.REQUIRED_QTY);

            // #40：必填服务/参数缺失属配置错误，抛异常由基类转为 Failure
            if (inventoryService == null || string.IsNullOrWhiteSpace(warehouseCode) || string.IsNullOrWhiteSpace(materialCode))
                throw new InvalidOperationException("库存分配缺少必填项(IInventoryQueryService/WarehouseCode/MaterialCode)");

            if (requiredQty <= 0)
                return new InventoryAllocationResult();

            // A2: 出库不再由 OutboundAllocation 预选分区。全仓按 FIFO 取基础列表。
            var inventoryList = await inventoryService.GetByFIFOAsync(warehouseCode, materialCode, cancellationToken);

            // 核心排序：可用量升序（优先清空少量库存的库位）→ 入库时间升序（FIFO 兜底，NULL 排后）
            var sorted = inventoryList
                .OrderBy(i => i.Qty - i.LockedQty)         // 可用量少的优先（容易清空）
                .ThenBy(i => i.InboundTime ?? DateTime.MaxValue)  // FIFO 兜底，无入库时间排最后
                .ToList();

            // 出库偏好（分区优先级/整托优先）作为最外层稳定排序
            sorted = ApplyPreferences(sorted, requiredQty, context);

            return AllocateInventory(sorted, requiredQty);
        }

        /// <summary>
        /// 遍历库存列表，分配到满足需求量为止
        /// </summary>
        private InventoryAllocationResult AllocateInventory(
            List<Strategy.DTOs.InventoryInfoDTO> inventoryList, decimal requiredQty)
        {
            var result = new InventoryAllocationResult();
            var remaining = requiredQty;
            var priority = 1;

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
                    Reason = "利用率优先"
                });

                remaining -= allocateQty;
            }

            return result;
        }
    }
}
