using KH.WMS.Algorithms.Strategy.Configuration;
using KH.WMS.Core.Services;

namespace KH.WMS.Algorithms.Strategy.Services
{
    /// <summary>
    /// 策略配置服务接口
    /// </summary>
    public interface IStrategyConfigService : ICrudService<CfgStrategyConfig>
    {

        /// <summary>分页查询</summary>
        Task<(List<CfgStrategyConfig> Items, int Total)> GetPagedListAsync(
            int pageIndex, int pageSize,
            string? strategyType = null,
            string? keyword = null,
            long? warehouseId = null,
            byte? status = null);

        /// <summary>获取指定类型的已启用配置（按优先级降序）</summary>
        Task<List<CfgStrategyConfig>> GetByTypeAsync(string strategyType, long? warehouseId = null);

        /// <summary>根据编码获取</summary>
        Task<CfgStrategyConfig?> GetByCodeAsync(string strategyCode);

        /// <summary>批量删除</summary>
        Task<bool> DeleteAsync(List<long> ids);

        /// <summary>验证策略参数JSON</summary>
        bool ValidateParams(string? paramsJson, out string errorMessage);

        /// <summary>检查编码唯一</summary>
        Task<bool> IsCodeUniqueAsync(string strategyCode, long? excludeId = null);
    }
}
