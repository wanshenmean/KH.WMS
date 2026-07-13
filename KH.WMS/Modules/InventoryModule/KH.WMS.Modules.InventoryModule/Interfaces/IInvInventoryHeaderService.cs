using KH.WMS.Core.Api.Responses;
using KH.WMS.Core.Services;
using KH.WMS.Entities.Inventory;

namespace KH.WMS.Modules.InventoryModule.Interfaces
{
    public interface IInvInventoryHeaderService : ICrudService<InvInventoryHeader>
    {
        Task<ApiResponse> GetStatDataAsync();
        Task<ApiResponse> FreezeAsync(long headerId, string reason);
        Task<ApiResponse> UnfreezeAsync(long headerId);
    }
}
