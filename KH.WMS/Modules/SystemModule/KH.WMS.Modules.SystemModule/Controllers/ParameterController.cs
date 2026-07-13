using KH.WMS.Core.Api.Responses;
using KH.WMS.Core.Controllers;
using KH.WMS.Entities.System;
using KH.WMS.Modules.SystemModule.DTOs;
using KH.WMS.Modules.SystemModule.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace KH.WMS.Modules.SystemModule.Controllers
{
    [ApiController]
    [Route("api/parameter")]
    public class ParameterController(ISysParameterService parameterService) : CrudController<SysParameter>(parameterService)
    {
        private readonly ISysParameterService _parameterService = parameterService;

        /// <summary>
        /// 获取所有参数分组
        /// </summary>
        [HttpGet("groups")]
        public async Task<ApiResponse> GetGroups()
        {
            return await _parameterService.GetParameterGroupsAsync();
        }

        /// <summary>
        /// 新增/编辑参数
        /// </summary>
        [HttpPost("save")]
        public async Task<ApiResponse> Save([FromBody] SaveParameterDto dto)
        {
            return await _parameterService.SaveParameterAsync(dto);
        }

        /// <summary>
        /// 按编码获取参数值
        /// </summary>
        [HttpGet("by-code/{paramCode}")]
        public async Task<ApiResponse> GetByCode(string paramCode)
        {
            return await _parameterService.GetParameterByCodeAsync(paramCode);
        }
    }
}
