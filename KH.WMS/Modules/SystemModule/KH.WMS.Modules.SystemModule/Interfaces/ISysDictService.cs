using KH.WMS.Core.Api.Responses;
using KH.WMS.Core.Models.Dtos;
using KH.WMS.Core.Services;
using KH.WMS.Entities.System;
using KH.WMS.Modules.SystemModule.DTOs;

namespace KH.WMS.Modules.SystemModule.Interfaces
{
    /// <summary>
    /// 系统字典服务接口
    /// </summary>
    public interface ISysDictService : ICrudService<SysDictType>
    {
        /// <summary>
        /// 获取字典项列表（自动判断静态/SQL数据源）
        /// </summary>
        Task<ApiResponse> GetDictItemsAsync(string dictCode);

        Task<ApiResponse> GetDictItemsList(int dictId, AdvancedQueryRequestDto query);

        /// <summary>
        /// 删除字典类型
        /// </summary>
        Task<ApiResponse> DeleteDictTypeAsync(long dictTypeId);

        /// <summary>
        /// 清除字典缓存
        /// </summary>
        Task<ApiResponse> ClearDictCacheAsync(string? dictCode = null);

        /// <summary>
        /// 获取字典项列表（按字典类型ID，管理用）
        /// </summary>
        Task<ApiResponse> GetDictItemListAsync(long dictTypeId);

        /// <summary>
        /// 新增/编辑字典项
        /// </summary>
        Task<ApiResponse> SaveDictItemAsync(SaveDictItemDto dto);

        /// <summary>
        /// 删除字典项
        /// </summary>
        Task<ApiResponse> DeleteDictItemAsync(long dictItemId);
    }
}
