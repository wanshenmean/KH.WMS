using KH.WMS.Core.Api.Responses;
using KH.WMS.Core.Services;
using KH.WMS.Entities.System;
using KH.WMS.Modules.SystemModule.DTOs;

namespace KH.WMS.Modules.SystemModule.Interfaces
{
    /// <summary>
    /// 系统参数服务接口
    /// </summary>
    public interface ISysParameterService : ICrudService<SysParameter>
    {
        /// <summary>
        /// 预热：将所有启用的系统参数加载到缓存
        /// 建议在应用启动时调用
        /// </summary>
        Task WarmUpAsync();

        /// <summary>
        /// 获取所有参数分组
        /// </summary>
        Task<ApiResponse> GetParameterGroupsAsync();

        /// <summary>
        /// 新增/编辑参数
        /// </summary>
        Task<ApiResponse> SaveParameterAsync(SaveParameterDto dto);

        /// <summary>
        /// 删除参数
        /// </summary>
        Task<ApiResponse> DeleteParameterAsync(long id);

        /// <summary>
        /// 按编码获取参数值
        /// </summary>
        Task<ApiResponse> GetParameterByCodeAsync(string paramCode);
    }
}
