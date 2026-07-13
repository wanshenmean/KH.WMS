using KH.WMS.Core.Api.Responses;
using KH.WMS.Core.Services;
using KH.WMS.Entities.Inventory;

namespace KH.WMS.Modules.InventoryModule.Interfaces
{
    public interface IInvSnapshotHeaderService : ICrudService<InvSnapshotHeader>
    {
        /// <summary>
        /// 创建库存快照
        /// </summary>
        /// <param name="snapshotName">快照名称</param>
        /// <param name="snapshotType">快照类型（MANUAL/DAILY/STOCKTAKE）</param>
        /// <param name="description">快照说明</param>
        /// <param name="stocktakeId">关联盘点单ID（STOCKTAKE 类型时使用）</param>
        Task<ApiResponse> CreateSnapshotAsync(string snapshotName, string snapshotType, string? description, long? stocktakeId = null);

        /// <summary>
        /// 获取快照明细列表
        /// </summary>
        Task<ApiResponse> GetSnapshotDetailsAsync(long headerId);
    }
}
