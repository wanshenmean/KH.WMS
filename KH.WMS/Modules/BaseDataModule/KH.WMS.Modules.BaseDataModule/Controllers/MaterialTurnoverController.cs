using KH.WMS.Core.Api.Responses;
using KH.WMS.Core.Controllers;
using KH.WMS.Entities.BaseData;
using KH.WMS.Modules.BaseDataModule.DTOs;
using KH.WMS.Modules.BaseDataModule.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace KH.WMS.Modules.BaseDataModule.Controllers
{
    [Route("api/material-turnover")]
    public class MaterialTurnoverController(IMaterialTurnoverService materialTurnoverService) : CrudController<MdMaterialTurnover>(materialTurnoverService)
    {
        /// <summary>
        /// 执行ABC分类计算
        /// </summary>
        [HttpPost("calculate")]
        public async Task<ApiResponse> Calculate([FromBody] TurnoverCalculateRequest request)
        {
            return await materialTurnoverService.CalculateAsync(request);
        }

        /// <summary>
        /// 查询指定周期的分类结果
        /// </summary>
        [HttpGet("results")]
        public async Task<ApiResponse<List<MaterialTurnoverDto>>> GetResults([FromQuery] string period)
        {
            var data = await materialTurnoverService.GetResultsAsync(period);
            return ApiResponse<List<MaterialTurnoverDto>>.Ok(data);
        }
    }
}
