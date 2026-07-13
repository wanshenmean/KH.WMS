using KH.WMS.Core.Api.Responses;
using KH.WMS.Core.Database.Repositories;
using KH.WMS.Core.Database.UnitOfWorks;
using KH.WMS.Core.DependencyInjection.ServiceLifetimes;
using KH.WMS.Core.Services;
using KH.WMS.Entities.Constants;
using KH.WMS.Entities.Inventory;
using KH.WMS.Modules.InventoryModule.Interfaces;
using SqlSugar;

namespace KH.WMS.Modules.InventoryModule.Services
{
    [RegisteredService(ServiceType = typeof(IInvInventoryHeaderService))]
    public class InvInventoryHeaderService(
        IRepository<InvInventoryHeader, long> repository,
        ISqlSugarClient db,
        IUnitOfWork unitOfWork,
        IDetailSaveService detailSaveService) : CrudService<InvInventoryHeader>(repository, unitOfWork, detailSaveService), IInvInventoryHeaderService
    {
        /// <inheritdoc />
        public async Task<ApiResponse> GetStatDataAsync()
        {
            var all = await repository.GetAllAsync();
            return ApiResponse.Ok(new
            {
                totalCount = all.Count,
                availableCount = all.Count(h => h.InventoryStatus == BizConstants.InventoryStatus.AVAILABLE),
                lockedCount = all.Count(h => h.InventoryStatus == BizConstants.InventoryStatus.LOCKED),
                frozenCount = all.Count(h => h.InventoryStatus == BizConstants.InventoryStatus.FROZEN),
            });
        }

        /// <inheritdoc />
        public async Task<ApiResponse> FreezeAsync(long headerId, string reason)
        {
            var header = await repository.GetByIdAsync(headerId);
            if (header == null)
                return ApiResponse.Fail(ResponseCode.BAD_REQUEST, "库存记录不存在");

            if (header.InventoryStatus == BizConstants.InventoryStatus.FROZEN)
                return ApiResponse.Fail(ResponseCode.BAD_REQUEST, "该库存已处于冻结状态");

            if (header.InventoryStatus == BizConstants.InventoryStatus.LOCKED)
                return ApiResponse.Fail(ResponseCode.BAD_REQUEST, "锁定状态的库存不能冻结，请先解锁");

            var oldStatus = header.InventoryStatus;
            header.InventoryStatus = BizConstants.InventoryStatus.FROZEN;
            await repository.UpdateAsync(header);

            // 写冻结记录
            var record = new InvFreezeRecord
            {
                FreezeNo = $"DJ{DateTime.Now:yyyyMMddHHmmss}",
                WarehouseId = header.WarehouseId ?? 0,
                MaterialId = 0,
                MaterialCode = "",
                MaterialName = "",
                LocationId = header.LocationId,
                LocationCode = header.LocationCode,
                ContainerCode = header.ContainerCode,
                FreezeQty = header.DetailCount,
                FreezeReason = reason,
                Status = BizConstants.FreezeRecordStatus.FROZEN,
                FreezeTime = DateTime.Now,
            };
            await db.Insertable(record).ExecuteCommandAsync();

            // 写库存变动记录
            await WriteMovementAsync(header, BizConstants.MovementTypes.FREEZE, $"{oldStatus}→FROZEN: {reason}");

            return ApiResponse.Ok($"托盘 {header.ContainerCode} 已冻结");
        }

        /// <inheritdoc />
        public async Task<ApiResponse> UnfreezeAsync(long headerId)
        {
            var header = await repository.GetByIdAsync(headerId);
            if (header == null)
                return ApiResponse.Fail(ResponseCode.BAD_REQUEST, "库存记录不存在");

            if (header.InventoryStatus != BizConstants.InventoryStatus.FROZEN)
                return ApiResponse.Fail(ResponseCode.BAD_REQUEST, "当前状态不是冻结，无法解冻");

            header.InventoryStatus = BizConstants.InventoryStatus.AVAILABLE;
            await repository.UpdateAsync(header);

            // 更新冻结记录
            await db.Updateable<InvFreezeRecord>()
                .SetColumns(r => r.Status == BizConstants.FreezeRecordStatus.UNFROZEN)
                .SetColumns(r => r.UnfreezeTime == DateTime.Now)
                .Where(r => r.ContainerCode == header.ContainerCode && r.Status == BizConstants.FreezeRecordStatus.FROZEN)
                .ExecuteCommandAsync();

            // 写库存变动记录
            await WriteMovementAsync(header, BizConstants.MovementTypes.UNFREEZE, "FROZEN→AVAILABLE");

            return ApiResponse.Ok($"托盘 {header.ContainerCode} 已解冻");
        }

        /// <summary>
        /// 写入库存变动记录（库存流水）
        /// </summary>
        private async Task WriteMovementAsync(InvInventoryHeader header, string movementType, string remark, decimal? qty = null)
        {
            var movement = new InvMovement
            {
                WarehouseId = header.WarehouseId ?? 0,
                MaterialId = 0,
                LocationId = header.LocationId,
                ContainerCode = header.ContainerCode,
                MovementType = movementType,
                Direction = movementType switch
                {
                    BizConstants.MovementTypes.INBOUND => BizConstants.MovementDirections.INCREASE,
                    BizConstants.MovementTypes.OUTBOUND => BizConstants.MovementDirections.DECREASE,
                    _ => "", // 冻结/解冻/移库等不涉及方向
                },
                MovementQty = qty ?? header.DetailCount,
                MovementTime = DateTime.Now,
                DocType = movementType,
                DocNo = remark, // 状态变更类记录：存变更描述
            };
            await db.Insertable(movement).ExecuteCommandAsync();
        }
    }
}
