using KH.WMS.Core.Api.Responses;
using KH.WMS.Core.Services;
using KH.WMS.Entities.Inventory;

namespace KH.WMS.Modules.InventoryModule.Interfaces
{
    public interface IInvFreezeRecordService : ICrudService<InvFreezeRecord>
    {
        Task<ApiResponse> UnfreezeAsync(long id);
    }
}
