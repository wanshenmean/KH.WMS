using KH.WMS.Core.Api.Responses;
using KH.WMS.Core.Services;
using KH.WMS.Entities.System;

namespace KH.WMS.Modules.SystemModule.Interfaces
{
    /// <summary>
    /// 操作日志服务接口
    /// </summary>
    public interface ISysOperateLogService : ICrudService<SysOperateLog>
    {
        /// <summary>
        /// 清理过期日志（按天数）
        /// </summary>
        Task<ApiResponse> CleanLogsAsync(int retainDays);
    }
}
