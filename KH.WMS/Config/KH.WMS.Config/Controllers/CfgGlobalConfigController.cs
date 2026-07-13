using KH.WMS.Config.Abstractions;
using KH.WMS.Core.Api.Responses;
using KH.WMS.Core.Controllers;
using KH.WMS.Config.Abstractions;
using KH.WMS.Config.DTOs;
using KH.WMS.Config.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace KH.WMS.Config.Controllers
{
    [Route("api/global-config")]
    public class CfgGlobalConfigController(
        ICfgGlobalConfigService cfgGlobalConfigService,
        IConfigResolverContract configResolver) : CrudController<CfgGlobalConfig>(cfgGlobalConfigService)
    {
        /// <summary>
        /// 按分组查询所有配置
        /// </summary>
        [HttpGet("groups")]
        public async Task<List<ConfigGroupDto>> GetGroups()
        {
            return await cfgGlobalConfigService.GetByGroupAsync();
        }

        /// <summary>
        /// 查询指定分组的配置
        /// </summary>
        [HttpGet("group/{groupCode}")]
        public async Task<List<ConfigItemDto>> GetByGroup(string groupCode)
        {
            return await cfgGlobalConfigService.GetByGroupCodeAsync(groupCode);
        }

        /// <summary>
        /// 获取指定配置值
        /// </summary>
        [HttpGet("value/{group}/{key}")]
        public async Task<string> GetConfigValue(string group, string key)
        {
            return await cfgGlobalConfigService.GetConfigValueAsync(group, key);
        }

        /// <summary>
        /// 按作用域查询配置项列表
        /// </summary>
        [HttpGet("scoped/{groupCode}/{scopeLevel}")]
        public async Task<List<ConfigItemDto>> GetByScope(string groupCode, string scopeLevel, long? scopeId = null)
        {
            return await cfgGlobalConfigService.GetByGroupAndScopeAsync(groupCode, scopeLevel, scopeId);
        }

        /// <summary>
        /// 解析配置值（按优先级逐级查找：DOC_TYPE → ZONE → WAREHOUSE → GLOBAL）
        /// </summary>
        [HttpGet("resolve/{group}/{key}")]
        public async Task<string> ResolveValue(string group, string key, long? warehouseId = null, long? zoneId = null, string? docTypeCode = null)
        {
            var scope = new ConfigScopeContext
            {
                WarehouseId = warehouseId,
                ZoneId = zoneId,
                DocTypeCode = docTypeCode,
            };
            return await configResolver.ResolveConfigValueAsync(group, key, scope);
        }

        /// <summary>
        /// 批量修改配置值
        /// </summary>
        [HttpPut("batch")]
        public async Task BatchUpdate([FromBody] BatchUpdateConfigRequest request)
        {
            await cfgGlobalConfigService.BatchUpdateAsync(request);
        }

        /// <summary>
        /// 重置分组内所有配置为默认值
        /// </summary>
        [HttpPost("reset/{groupCode}")]
        public async Task ResetToDefault(string groupCode)
        {
            await cfgGlobalConfigService.ResetToDefaultAsync(groupCode);
        }

        /// <summary>
        /// 禁止手动新增配置项（所有配置由种子数据初始化）
        /// </summary>
        public override Task<ApiResponse> Create([FromBody] CfgGlobalConfig entity)
        {
            return Task.FromResult(ApiResponse.Fail(ResponseCode.BAD_REQUEST, "全局配置不允许手动新增，请通过系统初始化脚本管理"));
        }

        /// <summary>
        /// 禁止删除配置项
        /// </summary>
        public override Task<ApiResponse> Delete(long id)
        {
            return Task.FromResult(ApiResponse.Fail(ResponseCode.BAD_REQUEST, "全局配置不允许删除"));
        }

        /// <summary>
        /// 禁止批量删除配置项
        /// </summary>
        public override Task<ApiResponse> BatchDelete([FromBody] List<long> ids)
        {
            return Task.FromResult(ApiResponse.Fail(ResponseCode.BAD_REQUEST, "全局配置不允许删除"));
        }
    }
}
