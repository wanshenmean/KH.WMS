using KH.WMS.Core.Api.Responses;
using KH.WMS.Core.Services;
using KH.WMS.Entities.BaseData;
using KH.WMS.Modules.BaseDataModule.DTOs;

namespace KH.WMS.Modules.BaseDataModule.Interfaces
{
    public interface IMaterialTurnoverService : ICrudService<MdMaterialTurnover>
    {
        /// <summary>
        /// 执行ABC分类计算
        /// </summary>
        Task<ApiResponse> CalculateAsync(TurnoverCalculateRequest request);

        /// <summary>
        /// 查询指定周期的分类结果
        /// </summary>
        Task<List<MaterialTurnoverDto>> GetResultsAsync(string period);
    }
}
