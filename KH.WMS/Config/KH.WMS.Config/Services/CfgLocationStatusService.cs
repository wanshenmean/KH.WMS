using KH.WMS.Core.Database.Repositories;
using KH.WMS.Core.Database.UnitOfWorks;
using KH.WMS.Core.DependencyInjection.ServiceLifetimes;
using KH.WMS.Core.Services;
using KH.WMS.Config.Abstractions;
using KH.WMS.Core.Constants;
using KH.WMS.Config.Interfaces;

namespace KH.WMS.Config.Services
{
    [RegisteredService(ServiceType = typeof(ICfgLocationStatusService))]
    public class CfgLocationStatusService(
        IRepository<CfgLocationStatus, long> repository,
        IUnitOfWork unitOfWork,
        IDetailSaveService detailSaveService) : CrudService<CfgLocationStatus>(repository, unitOfWork, detailSaveService), ICfgLocationStatusService
    {
        /// <summary>
        /// 校验状态流转是否合法
        /// </summary>
        public async Task<bool> CanTransitionAsync(string fromStatus, string toStatus)
        {
            // 相同状态视为合法（幂等）
            if (fromStatus == toStatus)
                return true;

            var targetConfig = await _repository.GetFirstOrDefaultAsync(x => x.StatusCode == toStatus && x.Status == BoolFlag.YES);

            if (targetConfig == null)
                return false;

            // 留空不限制转入来源
            if (string.IsNullOrWhiteSpace(targetConfig.AllowedFromStatuses))
                return true;

            var allowedList = targetConfig.AllowedFromStatuses
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim());

            return allowedList.Contains(fromStatus);
        }

        /// <summary>
        /// 获取指定状态允许转入的所有目标状态
        /// </summary>
        public async Task<List<string>> GetAvailableTransitionsAsync(string fromStatus)
        {
            var allStatuses = await _repository.GetListAsync(x => x.Status == BoolFlag.YES);

            var result = new List<string>();
            foreach (var status in allStatuses)
            {
                // 留空不限制
                if (string.IsNullOrWhiteSpace(status.AllowedFromStatuses))
                {
                    result.Add(status.StatusCode);
                    continue;
                }

                var allowedList = status.AllowedFromStatuses
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim());

                if (allowedList.Contains(fromStatus))
                    result.Add(status.StatusCode);
            }

            return result;
        }
    }
}
