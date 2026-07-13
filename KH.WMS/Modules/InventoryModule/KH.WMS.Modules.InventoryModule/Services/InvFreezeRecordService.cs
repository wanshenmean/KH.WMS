using KH.WMS.Core.Api.Responses;
using KH.WMS.Core.Database.Repositories;
using KH.WMS.Core.Database.UnitOfWorks;
using KH.WMS.Core.DependencyInjection.ServiceLifetimes;
using KH.WMS.Core.Models;
using KH.WMS.Core.Services;
using KH.WMS.Entities.Constants;
using KH.WMS.Entities.Inventory;
using KH.WMS.Modules.InventoryModule.Interfaces;
using SqlSugar;

namespace KH.WMS.Modules.InventoryModule.Services
{
    [RegisteredService(ServiceType = typeof(IInvFreezeRecordService))]
    public class InvFreezeRecordService(
        IRepository<InvFreezeRecord, long> repository,
        ISqlSugarClient db,
        IUnitOfWork unitOfWork,
        IDetailSaveService detailSaveService) : CrudService<InvFreezeRecord>(repository, unitOfWork, detailSaveService), IInvFreezeRecordService
    {
        public async Task<ApiResponse> UnfreezeAsync(long id)
        {
            var record = await repository.GetByIdAsync(id);
            if (record == null)
                return ApiResponse.Fail(ResponseCode.BAD_REQUEST, "冻结记录不存在");

            if (record.Status != BizConstants.FreezeRecordStatus.FROZEN)
                return ApiResponse.Fail(ResponseCode.BAD_REQUEST, "当前状态不是冻结中，无法解冻");

            record.Status = BizConstants.FreezeRecordStatus.UNFROZEN;
            record.UnfreezeTime = DateTime.Now;
            await repository.UpdateAsync(record);

            // 恢复库存头状态为可用
            if (!string.IsNullOrEmpty(record.ContainerCode))
            {
                var header = await db.Queryable<InvInventoryHeader>()
                    .FirstAsync(h => h.ContainerCode == record.ContainerCode);
                if (header != null && header.InventoryStatus == BizConstants.InventoryStatus.FROZEN)
                {
                    header.InventoryStatus = BizConstants.InventoryStatus.AVAILABLE;
                    await db.Updateable(header).UpdateColumns(h => h.InventoryStatus).ExecuteCommandAsync();
                }
            }

            return ApiResponse.Ok("解冻成功");
        }
    }
}
