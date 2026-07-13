using KH.WMS.Algorithms.Strategy.Constants;
using KH.WMS.Algorithms.Strategy.Interfaces;
using KH.WMS.Algorithms.Strategy.QueryServices;
using KH.WMS.Algorithms.Strategy.Strategies;

namespace KH.WMS.Algorithms.Strategy.Implementations.InventoryAllocation
{
    /// <summary>
    /// 先过期先出策略
    /// 按过期日期先后分配库存，优先出库即将过期的物料
    /// 全仓查询后按出库偏好（分区优先级/整托优先）稳定排序，不预选分区（避免破坏跨仓 FEFO）
    /// </summary>
    public class FefoStrategy(IInventoryQueryService inventoryQueryService) : InventoryAllocationStrategyBase()
    {
        public override string Name => "先过期先出策略";
        public override string Code => "FEFO";
        public override string Author => "System";
        public override string Description => "按过期日期先后出库，优先分配即将过期的库存";

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

            // A2: 出库不再由 OutboundAllocation 预选分区。全仓按 FEFO 查询，再用出库偏好稳定排序后分配。
            var inventoryList = await inventoryService.GetByFEFOAsync(warehouseCode, materialCode, cancellationToken);
            inventoryList = ApplyPreferences(inventoryList, requiredQty, context);

            return AllocateInventory(inventoryList, requiredQty);
        }

        /// <summary>
        /// 按FEFO顺序遍历库存列表，分配到满足需求量为止
        /// ExpiryDate 为 null 的排到最后（由查询服务保证排序）
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
                    Reason = inv.ExpiryDate.HasValue
                        ? $"先过期先出（过期日期: {inv.ExpiryDate:yyyy-MM-dd}）"
                        : "先过期先出（无过期日期）"
                });

                remaining -= allocateQty;
            }

            return result;
        }
    }
}
