using KH.WMS.Algorithms.Strategy.Configuration;
using KH.WMS.Core.Services;

namespace KH.WMS.Algorithms.Strategy.Services
{
    /// <summary>
    /// 策略链配置服务接口
    /// </summary>
    public interface IStrategyChainService : ICrudService<CfgStrategyChainConfig>
    {
        /// <summary>获取指定类型的已启用策略链</summary>
        Task<List<CfgStrategyChainConfig>> GetByTypeAsync(string chainType, long? warehouseId = null);

        /// <summary>根据编码获取</summary>
        Task<CfgStrategyChainConfig?> GetByCodeAsync(string chainCode);

        /// <summary>获取策略链的所有步骤</summary>
        Task<List<CfgStrategyChainStep>> GetStepsByChainIdAsync(long chainId);

        /// <summary>获取策略链完整详情（含步骤）</summary>
        Task<StrategyChainDetail?> GetChainDetailAsync(long chainId);

        /// <summary>创建策略链（含步骤）</summary>
        Task<long> CreateAsync(StrategyChainCreateRequest request);

        /// <summary>更新策略链（含步骤，全量替换）</summary>
        Task<bool> UpdateAsync(StrategyChainUpdateRequest request);

        /// <summary>检查编码唯一</summary>
        Task<bool> IsCodeUniqueAsync(string chainCode, long? excludeId = null);

        /// <summary>
        /// 校验指定类型策略链的组成是否包含所有必需的策略类型步骤。
        /// 返回不合规链的警告信息列表（空列表表示全部合规）。
        /// </summary>
        Task<List<string>> ValidateChainCompositionAsync(string chainType, params string[] requiredStrategyTypes);
    }

    /// <summary>
    /// 策略链创建请求
    /// </summary>
    public class StrategyChainCreateRequest
    {
        public CfgStrategyChainConfig Chain { get; set; } = new();
        public List<CfgStrategyChainStep> Steps { get; set; } = new();
    }

    /// <summary>
    /// 策略链更新请求
    /// </summary>
    public class StrategyChainUpdateRequest
    {
        public CfgStrategyChainConfig Chain { get; set; } = new();
        public List<CfgStrategyChainStep> Steps { get; set; } = new();
    }

    /// <summary>
    /// 策略链详情
    /// </summary>
    public class StrategyChainDetail
    {
        public CfgStrategyChainConfig Chain { get; set; } = new();
        public List<CfgStrategyChainStep> Steps { get; set; } = new();
    }
}
