using KH.WMS.Core.Api.Responses;
using KH.WMS.Core.Controllers;
using KH.WMS.Core.Models.Dtos;
using KH.WMS.Entities.System;
using KH.WMS.Modules.SystemModule.DTOs;
using KH.WMS.Modules.SystemModule.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace KH.WMS.Modules.SystemModule.Controllers
{
    [ApiController]
    [Route("api/dict")]
    public class DictController(ISysDictService dictService) : CrudController<SysDictType>(dictService)
    {
        private readonly ISysDictService _dictService = dictService;

        /// <summary>
        /// 获取字典项列表（自动判断静态/SQL数据源）
        /// </summary>
        [HttpGet("items/{dictCode}")]
        public async Task<ApiResponse> GetDictItems(string dictCode)
        {
            return await _dictService.GetDictItemsAsync(dictCode);
        }

        [HttpPost("items/list/{dictId}")]
        public async Task<ApiResponse> GetDictItemsList(int dictId, [FromBody] AdvancedQueryRequestDto query)
        {
            return await _dictService.GetDictItemsList(dictId, query);
        }

        [HttpDelete("data/{id}")]
        public async Task<ApiResponse> DeleteDictItem(int id)
        {
            return await _dictService.DeleteDictItemAsync(id);
        }


        ///// <summary>
        ///// 新增/编辑字典类型
        ///// </summary>
        //[HttpPost("types/save")]
        //public async Task<ApiResponse> SaveDictType([FromBody] SaveDictTypeDto dto)
        //{
        //    return dto.Id == null
        //        ? await _dictService.AddDictTypeAsync(dto)
        //        : await _dictService.UpdateDictTypeAsync(dto);
        //}

        ///// <summary>
        ///// 删除字典类型
        ///// </summary>
        //[HttpDelete("types/{id}")]
        //public async Task<ApiResponse> DeleteDictType(long id)
        //{
        //    return await _dictService.DeleteDictTypeAsync(id);
        //}

        /// <summary>
        /// 清除字典缓存
        /// </summary>
        [HttpPost("cache/clear")]
        public async Task<ApiResponse> ClearCache(string? dictCode = null)
        {
            return await _dictService.ClearDictCacheAsync(dictCode);
        }

        ///// <summary>
        ///// 字典项列表（按字典类型ID）
        ///// </summary>
        //[HttpGet("items/list/{dictTypeId}")]
        //public async Task<ApiResponse> GetDictItemList(long dictTypeId)
        //{
        //    return await _dictService.GetDictItemListAsync(dictTypeId);
        //}

        /// <summary>
        /// 新增/编辑字典项
        /// </summary>
        [HttpPost("items/save")]
        public async Task<ApiResponse> SaveDictItem([FromBody] SaveDictItemDto dto)
        {
            return await _dictService.SaveDictItemAsync(dto);
        }

    }
}
