using KH.WMS.Core.Api.Responses;
using KH.WMS.Core.Controllers;
using KH.WMS.Entities.System;
using KH.WMS.Modules.SystemModule.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace KH.WMS.Modules.SystemModule.Controllers
{
    [ApiController]
    [Route("api/operate-log")]
    public class OperateLogController(ISysOperateLogService logService) : CrudController<SysOperateLog>(logService)
    {
        private readonly ISysOperateLogService _logService = logService;

        /// <summary>
        /// 清理过期日志（按天数）
        /// </summary>
        [HttpDelete("clean")]
        public async Task<ApiResponse> CleanLogs(int retainDays = 90)
        {
            return await _logService.CleanLogsAsync(retainDays);
        }
    }
}
