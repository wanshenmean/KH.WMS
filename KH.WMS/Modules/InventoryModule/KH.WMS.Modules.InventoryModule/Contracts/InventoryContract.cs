using KH.WMS.Contracts.Inventory;
using KH.WMS.Core.Database.UnitOfWorks;
using KH.WMS.Core.DependencyInjection.ServiceLifetimes;
using KH.WMS.Entities.Constants;
using KH.WMS.Entities.Inventory;
using KH.WMS.Entities.Warehouse;
using Microsoft.Extensions.DependencyInjection;
using SqlSugar;
using static KH.WMS.Core.Logging.WMSError.WMSErrorCodes;
using KH.WMS.Core.Logging.WMSError;

namespace KH.WMS.Modules.InventoryModule.Contracts
{
    [RegisteredService(Lifetime = ServiceLifetime.Scoped, ServiceType = typeof(IInventoryContract))]
    public class InventoryContract(IUnitOfWork unitOfWork) : IInventoryContract
    {
        /// <inheritdoc />
        public async Task<bool> ContainerHasInventoryAsync(string containerCode)
        {
            var headerRepo = unitOfWork.GetRepository<InvInventoryHeader, long>();
            return await headerRepo.GetFirstOrDefaultAsync(h => h.ContainerCode == containerCode) != null;
        }

        /// <inheritdoc />
        public async Task<decimal> GetContainerQtyAsync(string containerCode)
        {
            var headerRepo = unitOfWork.GetRepository<InvInventoryHeader, long>();
            var header = await headerRepo.GetFirstOrDefaultAsync(h => h.ContainerCode == containerCode);
            if (header == null) return 0;

            var detailRepo = unitOfWork.GetRepository<InvInventoryDetail, long>();
            var details = await detailRepo.GetListAsync(d => d.HeaderId == header.Id);
            return details.Sum(d => d.Qty);
        }

        /// <inheritdoc />
        public async Task<bool> IsLocationAvailableAsync(long locationId)
        {
            var headerRepo = unitOfWork.GetRepository<InvInventoryHeader, long>();
            var header = await headerRepo.GetFirstOrDefaultAsync(h => h.LocationId == locationId);
            return header == null || header.InventoryStatus == BizConstants.InventoryStatus.AVAILABLE;
        }

        /// <inheritdoc />
        public async Task<long> GenerateInventoryFromPutawayAsync(InventoryGenerationRequest request)
        {
            var headerRepo = unitOfWork.GetRepository<InvInventoryHeader, long>();
            var detailRepo = unitOfWork.GetRepository<InvInventoryDetail, long>();

            // WarehouseCode 为空时从仓库表补查
            var warehouseCode = request.WarehouseCode;
            if (string.IsNullOrWhiteSpace(warehouseCode) && request.WarehouseId > 0)
            {
                var warehouseRepo = unitOfWork.GetRepository<MdWarehouse, long>();
                var warehouse = await warehouseRepo.GetByIdAsync(request.WarehouseId);
                warehouseCode = warehouse?.WarehouseCode ?? string.Empty;
            }

            var now = DateTime.Now;

            var inventoryHeader = new InvInventoryHeader
            {
                ContainerCode = request.ContainerCode,
                WarehouseId = request.WarehouseId,
                WarehouseCode = warehouseCode,
                LocationId = request.LocationId,
                LocationCode = request.LocationCode,
                InventoryStatus = BizConstants.InventoryStatus.AVAILABLE,
                DetailCount = request.Lines.Count,
                InboundTime = now,
            };

            var headerId = await headerRepo.AddAsync(inventoryHeader);

            var inventoryDetails = request.Lines.Select(line => new InvInventoryDetail
            {
                HeaderId = headerId,
                MaterialId = line.MaterialId,
                MaterialCode = line.MaterialCode,
                BatchNo = line.BatchNo,
                Qty = line.Qty,
                ProductionDate = line.ProductionDate,
                ExpiryDate = line.ExpiryDate,
                InboundDocNo = line.InboundDocNo,
                InboundTime = now,
            }).ToList();

            await detailRepo.AddAsync(inventoryDetails);

            return headerId;
        }

        /// <inheritdoc />
        public async Task<decimal?> DeductInventoryAsync(InventoryDeductRequest request)
        {
            var detailRepo = unitOfWork.GetRepository<InvInventoryDetail, long>();
            var headerRepo = unitOfWork.GetRepository<InvInventoryHeader, long>();
            var movementRepo = unitOfWork.GetRepository<InvMovement, long>();

            var invDetail = await detailRepo.GetByIdAsync(request.InventoryDetailId);
            if (invDetail == null || request.DeductQty <= 0) return null;

            // 通过 HeaderId 获取 LocationId 和 ContainerCode
            var invHeader = await headerRepo.GetByIdAsync(invDetail.HeaderId);

            // 扣减前校验：冻结状态库存不允许扣减（与 LockInventoryAsync 保持一致）
            if (invHeader != null && invHeader.InventoryStatus == BizConstants.InventoryStatus.FROZEN)
                throw new InvalidOperationException(WMSErrorMessages.GetMessage(INVENTORY_FROZEN, request.InventoryDetailId));

            // 扣减前校验可用量：不足则拒绝（返回 null），不再静默 clamp 到 0 掩盖超扣
            if (invDetail.Qty < request.DeductQty)
                return null;

            var qtyBefore = invDetail.Qty;
            invDetail.Qty -= request.DeductQty;
            // LockedQty 同步释放对应锁定量，下限 0（锁定量可能小于扣减量，全部释放即可）
            invDetail.LockedQty = Math.Max(0, invDetail.LockedQty - request.DeductQty);
            await detailRepo.UpdateAsync(invDetail);

            await movementRepo.AddAsync(new InvMovement
            {
                WarehouseId = request.WarehouseId,
                MaterialId = invDetail.MaterialId,
                LocationId = invHeader?.LocationId ?? 0,
                BatchNo = invDetail.BatchNo,
                ContainerCode = invHeader?.ContainerCode ?? string.Empty,
                MovementType = request.MovementType,
                Direction = BizConstants.MovementDirections.DECREASE,
                MovementQty = request.DeductQty,
                QtyBefore = qtyBefore,
                QtyAfter = invDetail.Qty,
                DocType = request.DocType,
                DocNo = request.DocNo,
                MovementTime = DateTime.Now,
            });

            return invDetail.Qty;
        }

        /// <inheritdoc />
        public async Task<decimal?> LockInventoryAsync(long inventoryDetailId, decimal lockQty)
        {
            var detailRepo = unitOfWork.GetRepository<InvInventoryDetail, long>();
            var headerRepo = unitOfWork.GetRepository<InvInventoryHeader, long>();
            var invDetail = await detailRepo.GetByIdAsync(inventoryDetailId);
            if (invDetail == null || lockQty <= 0) return null;

            // 校验库存是否冻结
            var invHeader = await headerRepo.GetByIdAsync(invDetail.HeaderId);
            if (invHeader != null && invHeader.InventoryStatus == BizConstants.InventoryStatus.FROZEN)
                throw new InvalidOperationException(WMSErrorMessages.GetMessage(INVENTORY_FROZEN, inventoryDetailId));

            // 原子条件锁定：仅当可用量 (Qty - LockedQty) >= lockQty 时才自增 LockedQty，
            // 杜绝并发分配互相覆盖导致的超锁/超卖（DB 层序列化该 UPDATE）。
            // 0 行受影响说明可用量不足或已被并发占用，返回 null 由调用方失败处理。
            var affected = await unitOfWork.DbContext.Db.Updateable<InvInventoryDetail>()
                .SetColumns(d => new InvInventoryDetail { LockedQty = d.LockedQty + lockQty })
                .Where(d => d.Id == inventoryDetailId && (d.Qty - d.LockedQty) >= lockQty)
                .ExecuteCommandAsync();

            if (affected == 0) return null;

            // 返回锁定后的最新 LockedQty
            return (await detailRepo.GetByIdAsync(inventoryDetailId))?.LockedQty;
        }

        /// <inheritdoc />
        public async Task<decimal?> UnlockInventoryAsync(long inventoryDetailId, decimal unlockQty)
        {
            var detailRepo = unitOfWork.GetRepository<InvInventoryDetail, long>();
            var invDetail = await detailRepo.GetByIdAsync(inventoryDetailId);
            if (invDetail == null || unlockQty <= 0) return null;

            // 仅减少锁定数量，不动实际 Qty；锁定量不允许减为负数
            invDetail.LockedQty -= unlockQty;
            if (invDetail.LockedQty < 0) invDetail.LockedQty = 0;
            await detailRepo.UpdateAsync(invDetail);

            return invDetail.LockedQty;
        }

        /// <inheritdoc />
        public async Task MoveInventoryLocationAsync(InventoryMoveLocationRequest request)
        {
            var headerRepo = unitOfWork.GetRepository<InvInventoryHeader, long>();
            var detailRepo = unitOfWork.GetRepository<InvInventoryDetail, long>();
            var movementRepo = unitOfWork.GetRepository<InvMovement, long>();

            // 查找该容器在原货位的库存头；找不到不再静默 no-op，抛出便于调用方感知
            var header = await headerRepo.GetFirstOrDefaultAsync(
                h => h.ContainerCode == request.ContainerCode && h.LocationId == request.FromLocationId);
            if (header == null)
                throw new InvalidOperationException(
                    $"移库失败：容器 {request.ContainerCode} 在货位 {request.FromLocationId} 无库存");

            var now = DateTime.Now;

            // 更新库存头货位信息
            header.LocationId = request.ToLocationId;
            header.LocationCode = request.ToLocationCode;
            await headerRepo.UpdateAsync(header);

            // 记录库存流水（移库，数量与方向不变），按明细逐条写入
            var details = await detailRepo.GetListAsync(d => d.HeaderId == header.Id);
            foreach (var detail in details)
            {
                await movementRepo.AddAsync(new InvMovement
                {
                    WarehouseId = request.WarehouseId,
                    MaterialId = detail.MaterialId,
                    LocationId = request.ToLocationId,
                    BatchNo = detail.BatchNo,
                    ContainerCode = request.ContainerCode,
                    MovementType = BizConstants.MovementTypes.TRANSFER,
                    Direction = BizConstants.MovementDirections.UNCHANGED,
                    MovementQty = detail.Qty,
                    QtyBefore = detail.Qty,
                    QtyAfter = detail.Qty,
                    DocType = request.DocType,
                    DocNo = request.DocNo,
                    MovementTime = now,
                });
            }
        }
    }
}
