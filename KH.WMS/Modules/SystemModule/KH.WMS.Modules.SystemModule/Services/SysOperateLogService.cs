using System.Linq.Expressions;
using KH.WMS.Core.Api.Responses;
using KH.WMS.Core.Database.Repositories;
using KH.WMS.Core.Database.UnitOfWorks;
using KH.WMS.Core.DependencyInjection.ServiceLifetimes;
using KH.WMS.Core.Helpers;
using KH.WMS.Core.Services;
using KH.WMS.Entities.System;
using KH.WMS.Modules.SystemModule.Interfaces;

namespace KH.WMS.Modules.SystemModule.Services
{
    /// <summary>
    /// 操作日志服务实现
    /// </summary>
    [RegisteredService(ServiceType = typeof(ISysOperateLogService))]
    public class SysOperateLogService(
        IRepository<SysOperateLog, long> logRepository,
        IUnitOfWork unitOfWork,
        IDetailSaveService detailSaveService) : CrudService<SysOperateLog>(logRepository, unitOfWork, detailSaveService), ISysOperateLogService
    {
        private readonly IRepository<SysOperateLog, long> _logRepository = logRepository;

        public async Task<ApiResponse> CleanLogsAsync(int retainDays)
        {
            if (retainDays < 1)
                return ApiResponse.Fail(ResponseCode.BAD_REQUEST, "保留天数必须大于0");

            var cutoffTime = DateTime.Now.AddDays(-retainDays);
            await _logRepository.DeleteAsync(l => l.OperTime < cutoffTime);

            return ApiResponse.Ok(message: $"已清理 {retainDays} 天前的日志");
        }
    }
}
