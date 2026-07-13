using KH.WMS.Contracts.Inventory;
using KH.WMS.Contracts.Outbound;
using KH.WMS.Core.Database.UnitOfWorks;
using KH.WMS.Core.DependencyInjection.ServiceLifetimes;
using KH.WMS.Core.Logging.WMSError;
using KH.WMS.Core.Models;
using KH.WMS.Entities.Constants;
using KH.WMS.Entities.Outbound;
using Microsoft.Extensions.DependencyInjection;
using static KH.WMS.Core.Logging.WMSError.WMSErrorCodes;

namespace KH.WMS.Modules.OutboundModule.Contracts
{
    [RegisteredService(Lifetime = ServiceLifetime.Scoped, ServiceType = typeof(IOutboundContract))]
    public class OutboundContract(
        IUnitOfWork unitOfWork,
        IInventoryContract inventoryContract) : IOutboundContract
    {
        /// <inheritdoc />
        /// <remarks>
        /// 【按设计】自动化高位立库的"出库完成"指货从立库出来（下架），此处并不扣减库存。
        /// 真正的库存扣减发生在后续人工/设备拣选环节，由 PDA 调用本方法触发。
        /// 因此 WCS 任务完成路径（TaskHeaderService.PostTaskCompletionAsync）只做库位/容器状态流转，
        /// 不调用本扣减逻辑——这不是缺陷，而是立库出库流程的固有设计。
        /// </remarks>
        public async Task<ServiceResult> OnPickingTaskCompletedAsync(PickingCompletedEvent evt)
        {
            var detailRepo = unitOfWork.GetRepository<OutboundAllocationDetail, long>();
            var headerRepo = unitOfWork.GetRepository<OutboundAllocationHeader, long>();

            // 1. 通过任务号查找 TaskHeader
            var taskRepo = unitOfWork.GetRepository<Entities.Task.TaskHeader, long>();
            var task = await taskRepo.GetFirstOrDefaultAsync(t =>
                t.TaskNo == evt.TaskNo && t.DocId == evt.OutboundOrderId);

            if (task == null)
                return ServiceResult.Fail(WMSErrorMessages.GetMessage(TASK_NOT_EXIST, evt.TaskNo));

            // 2. 通过 TaskHeaderId 精确定位该任务关联的 PICKING 状态分配明细
            var allocationDetails = await detailRepo.GetListAsync(d =>
                d.TaskHeaderId == task.Id &&
                d.LineStatus == BizConstants.AllocationStatus.PICKING);

            if (allocationDetails.Count == 0)
                return ServiceResult.Ok(WMSErrorMessages.GetMessage(ALLOCATION_NO_NEED_PROCESS, evt.TaskNo));

            // 3. 获取分配头
            var allocationHeader = await headerRepo.GetByIdAsync(allocationDetails[0].HeaderId);
            if (allocationHeader == null)
                return ServiceResult.Fail(WMSErrorMessages.GetMessage(ALLOCATION_HEAD_NOT_EXIST));

            // 4. 逐条扣减库存并更新分配明细
            foreach (var detail in allocationDetails)
            {
                var deductQty = detail.AllocQty - detail.PickedQty;
                if (deductQty <= 0) continue;

                var result = await inventoryContract.DeductInventoryAsync(
                    new InventoryDeductRequest
                    {
                        // 必须使用库存明细ID（精确批次行），而非库存头ID，否则扣减会命中错误行/查不到而静默跳过
                        InventoryDetailId = detail.InventoryDetailId,
                        DeductQty = deductQty,
                        WarehouseId = allocationHeader.WarehouseId ?? 0,
                        DocType = BizConstants.DocTypes.OUTBOUND_ORDER,
                        DocNo = allocationHeader.OutboundOrderNo ?? string.Empty,
                    });

                // 扣减失败必须显式报错，不能静默 continue（否则订单仍会流转到 COMPLETED 而库存未扣）
                if (result == null)
                    return ServiceResult.Fail(WMSErrorMessages.GetMessage(ALLOCATION_DETAIL_NOT_EXIST, detail.InventoryDetailId));

                // 持久化 PickedQty + 行状态到同一已加载实体（旧版用只写状态的重载助手会丢弃 PickedQty）
                detail.PickedQty += deductQty;
                detail.LineStatus = BizConstants.AllocationStatus.PICKED;
                await detailRepo.UpdateAsync(detail);
            }

            // 5. 逐级向上检查状态
            await TryCompleteAllocationHeaderAsync(allocationHeader.Id);

            foreach (var lineId in allocationDetails.Select(d => d.OrderLineId).Distinct())
                await TryCompleteOrderLineAsync(lineId);

            await TryCompleteOrderAsync(evt.OutboundOrderId);

            return ServiceResult.Ok(WMSErrorMessages.GetMessage(ALLOCATION_PICK_COMPLETE_INFO, evt.TaskNo, allocationDetails.Count));
        }

        /// <inheritdoc />
        public async Task UpdateAllocationDetailStatusAsync(long detailId, string status, long? taskHeaderId = null)
        {
            var detailRepo = unitOfWork.GetRepository<OutboundAllocationDetail, long>();
            var detail = await detailRepo.GetByIdAsync(detailId);
            if (detail == null) return;

            detail.LineStatus = status;
            if (taskHeaderId.HasValue)
                detail.TaskHeaderId = taskHeaderId;
            await detailRepo.UpdateAsync(detail);
        }

        /// <inheritdoc />
        public async Task UpdateAllocationHeaderStatusAsync(long headerId, string status)
        {
            var headerRepo = unitOfWork.GetRepository<OutboundAllocationHeader, long>();
            var header = await headerRepo.GetByIdAsync(headerId);
            if (header == null) return;

            header.AllocStatus = status;
            if (status == BizConstants.AllocationStatus.PICKED)
                header.CompleteTime = DateTime.Now;
            await headerRepo.UpdateAsync(header);
        }

        /// <inheritdoc />
        public async Task UpdateOrderLineStatusAsync(long lineId, string status)
        {
            var lineRepo = unitOfWork.GetRepository<OutboundOrderLine, long>();
            var line = await lineRepo.GetByIdAsync(lineId);
            if (line == null) return;

            line.LineStatus = status;
            await lineRepo.UpdateAsync(line);
        }

        /// <inheritdoc />
        public async Task UpdateOrderStatusAsync(long orderId, string status)
        {
            var orderRepo = unitOfWork.GetRepository<OutboundOrder, long>();
            var order = await orderRepo.GetByIdAsync(orderId);
            if (order == null) return;

            order.OrderStatus = status;
            await orderRepo.UpdateAsync(order);
        }

        /// <inheritdoc />
        public async Task<bool> TryCompleteAllocationHeaderAsync(long allocationHeaderId)
        {
            var detailRepo = unitOfWork.GetRepository<OutboundAllocationDetail, long>();

            var allDetails = await detailRepo.GetListAsync(d => d.HeaderId == allocationHeaderId);
            if (!allDetails.All(d => d.LineStatus == BizConstants.AllocationStatus.PICKED))
                return false;

            await UpdateAllocationHeaderStatusAsync(allocationHeaderId, BizConstants.AllocationStatus.PICKED);
            return true;
        }

        /// <inheritdoc />
        public async Task<bool> TryCompleteOrderLineAsync(long orderLineId)
        {
            var detailRepo = unitOfWork.GetRepository<OutboundAllocationDetail, long>();

            var lineAllDetails = await detailRepo.GetListAsync(d => d.OrderLineId == orderLineId);
            if (!lineAllDetails.All(d => d.LineStatus == BizConstants.AllocationStatus.PICKED))
                return false;

            await UpdateOrderLineStatusAsync(orderLineId, BizConstants.OutboundLineStatus.PICKED);
            return true;
        }

        /// <inheritdoc />
        public async Task<bool> TryCompleteOrderAsync(long outboundOrderId)
        {
            var lineRepo = unitOfWork.GetRepository<OutboundOrderLine, long>();

            var allOrderLines = await lineRepo.GetListAsync(l => l.OrderId == outboundOrderId);
            if (!allOrderLines.All(l => l.LineStatus == BizConstants.OutboundLineStatus.PICKED))
                return false;

            await UpdateOrderStatusAsync(outboundOrderId, BizConstants.OutboundOrderStatus.COMPLETED);
            return true;
        }

        /// <inheritdoc />
        public async Task ResetAllocationByTaskAsync(long taskHeaderId)
        {
            var detailRepo = unitOfWork.GetRepository<OutboundAllocationDetail, long>();
            var headerRepo = unitOfWork.GetRepository<OutboundAllocationHeader, long>();

            // 1. 回退分配明细：PICKING → ALLOCATED，清空 TaskHeaderId
            var allocDetails = await detailRepo.GetListAsync(d =>
                d.TaskHeaderId == taskHeaderId &&
                d.LineStatus == BizConstants.AllocationStatus.PICKING);

            foreach (var detail in allocDetails)
            {
                // 释放该分配明细在分配时锁定的库存（防止取消/回滚后 LockedQty 永久泄漏）
                if (detail.InventoryDetailId > 0)
                {
                    await inventoryContract.UnlockInventoryAsync(detail.InventoryDetailId, detail.AllocQty);
                }

                detail.LineStatus = BizConstants.AllocationStatus.ALLOCATED;
                detail.TaskHeaderId = null;
                await detailRepo.UpdateAsync(detail);
            }

            // 2. 回退分配头：如果无其他 PICKING 明细，PICKING → ALLOCATED
            if (allocDetails.Count > 0)
            {
                var allocHeaderId = allocDetails[0].HeaderId;
                var otherPickingCount = await detailRepo.CountAsync(d =>
                    d.HeaderId == allocHeaderId &&
                    d.LineStatus == BizConstants.AllocationStatus.PICKING);

                if (otherPickingCount == 0)
                {
                    var allocHeader = await headerRepo.GetByIdAsync(allocHeaderId);
                    if (allocHeader != null && allocHeader.AllocStatus == BizConstants.AllocationStatus.PICKING)
                    {
                        allocHeader.AllocStatus = BizConstants.AllocationStatus.ALLOCATED;
                        await headerRepo.UpdateAsync(allocHeader);
                    }
                }
            }
        }
    }
}
