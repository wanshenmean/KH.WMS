using KH.WMS.Algorithms.Strategy.Constants;
using KH.WMS.Algorithms.Strategy.Interfaces;
using KH.WMS.Algorithms.Strategy.QueryServices;
using KH.WMS.Algorithms.Strategy.Strategies;

namespace KH.WMS.Algorithms.Strategy.Implementations.InventoryAllocation
{
    /// <summary>
    /// 先进先出策略
    /// 按入库时间先后分配库存，优先出库早入库的物料
    /// 全仓查询后按出库偏好（分区优先级/整托优先）稳定排序，不预选分区（避免破坏跨仓 FIFO）
    /// </summary>
    public class FifoStrategy(IInventoryQueryService inventoryQueryService) : InventoryAllocationStrategyBase
    {
        public override string Name => "先进先出策略";
        public override string Code => "FIFO";
        public override string Author => "System";
        public override string Description => "按入库时间先后出库，优先分配入库时间最早的库存";

        protected override async Task<InventoryAllocationResult> SelectInventoryAsync(IPolicyContext context, CancellationToken cancellationToken = default)
        {
            var inventoryService = inventoryQueryService;
            var warehouseCode = context.GetData<string>(StrategyParams.InventoryAllocationInput.WAREHOUSE_CODE) ?? context.WarehouseCode ?? string.Empty;
            var materialCode = context.GetData<string>(StrategyParams.InventoryAllocationInput.MATERIAL_CODE) ?? string.Empty;
            var requiredQty = context.GetData<decimal>(StrategyParams.InventoryAllocationInput.REQUIRED_QTY);

            // #40：必填服务/参数缺失属配置错误，抛异常由基类转为 Failure（与"无库存"的 Skipped 区分）
            if (inventoryService == null || string.IsNullOrWhiteSpace(warehouseCode) || string.IsNullOrWhiteSpace(materialCode))
                throw new InvalidOperationException("库存分配缺少必填项(IInventoryQueryService/WarehouseCode/MaterialCode)");

            if (requiredQty <= 0)
                return new InventoryAllocationResult();

            // A2: 出库不再由 OutboundAllocation 预选分区（会破坏跨仓先进先出）。
            // 全仓按 FIFO 查询，再用出库偏好（分区优先级/整托优先）做稳定排序后分配。
            var inventoryList = await inventoryService.GetByFIFOAsync(warehouseCode, materialCode, cancellationToken);
            inventoryList = ApplyPreferences(inventoryList, requiredQty, context);

            return AllocateInventory(inventoryList, requiredQty);
        }

        /// <summary>
        /// 按FIFO顺序遍历库存列表，分配到满足需求量为止
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
                    Reason = "先进先出"
                });

                remaining -= allocateQty;
            }

            return result;
        }
    }
}
