using KH.WMS.Core.Api.Responses;
using KH.WMS.Core.Services;
using KH.WMS.Entities.BaseData;

namespace KH.WMS.Modules.BaseDataModule.Interfaces
{
    public interface IMaterialCategoryService : ICrudService<MdMaterialCategory>
    {
        Task<ApiResponse> GetTreeAsync();
    }
}
