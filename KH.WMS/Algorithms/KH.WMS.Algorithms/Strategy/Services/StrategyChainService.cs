using System.Text.Json;
using KH.WMS.Algorithms.Strategy.Configuration;
using KH.WMS.Algorithms.Strategy.Constants;
using KH.WMS.Algorithms.Strategy.Enums;
using KH.WMS.Core.Api.Responses;
using KH.WMS.Core.Database.Repositories;
using KH.WMS.Core.Database.UnitOfWorks;
using KH.WMS.Core.DependencyInjection.ServiceLifetimes;
using KH.WMS.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace KH.WMS.Algorithms.Strategy.Services
{
    /// <summary>
    /// 策略链配置服务实现
    /// </summary>
    [RegisteredService(Lifetime = ServiceLifetime.Scoped, ServiceType = typeof(IStrategyChainService))]
    public class StrategyChainService(
        IRepository<CfgStrategyChainConfig, long> chainRepository,
        IRepository<CfgStrategyChainStep, long> stepRepository,
        IStrategyConfigService strategyConfigService,
        IUnitOfWork unitOfWork,
        IDetailSaveService detailSaveService) : CrudService<CfgStrategyChainConfig>(chainRepository, unitOfWork, detailSaveService), IStrategyChainService
    {
        private readonly IRepository<CfgStrategyChainConfig, long> _chainRepository = chainRepository;
        private readonly IRepository<CfgStrategyChainStep, long> _stepRepository = stepRepository;
        private readonly IStrategyConfigService _strategyConfigService = strategyConfigService;

        private static readonly HashSet<string> ValidChainTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            PolicyType.Putaway.ToString(), PolicyType.LocationAllocation.ToString(), PolicyType.Picking.ToString(), PolicyType.InventoryAllocation.ToString(), PolicyType.OutboundAllocation.ToString(), PolicyType.Wave.ToString()
        };

        public async Task<List<CfgStrategyChainConfig>> GetByTypeAsync(string chainType, long? warehouseId = null)
        {
            if (warehouseId.HasValue && warehouseId.Value > 0)
            {
                return await _chainRepository.GetListAsync(c =>
                    c.ChainType == chainType
                    && c.Status == AlgoConstants.Status.ENABLED
                    && (c.WarehouseId == null || c.WarehouseId == warehouseId.Value));
            }
            return await _chainRepository.GetListAsync(c =>
                c.ChainType == chainType
                && c.Status == AlgoConstants.Status.ENABLED);
        }

        public async Task<CfgStrategyChainConfig?> GetByCodeAsync(string chainCode)
        {
            return await _chainRepository.GetFirstOrDefaultAsync(c => c.ChainCode == chainCode);
        }

        public async Task<List<CfgStrategyChainStep>> GetStepsByChainIdAsync(long chainId)
        {
            return await _stepRepository.GetListAsync(s => s.ChainId == chainId);
        }

        public async Task<StrategyChainDetail?> GetChainDetailAsync(long chainId)
        {
            var chain = await _chainRepository.GetByIdAsync(chainId);
            if (chain == null) return null;

            var steps = await GetStepsByChainIdAsync(chainId);
            return new StrategyChainDetail { Chain = chain, Steps = steps };
        }

        public async Task<long> CreateAsync(StrategyChainCreateRequest request)
        {
            var chain = request.Chain;
            ValidateChainType(chain.ChainType);

            if (!await IsCodeUniqueAsync(chain.ChainCode))
                throw new InvalidOperationException($"策略链编码 {chain.ChainCode} 已存在");

            // 验证步骤
            ValidateSteps(request.Steps);

            chain.CreatedTime = DateTime.Now;
            chain.LastModifiedTime = DateTime.Now;
            chain.StepCount = request.Steps.Count;

            // #32：主表+步骤表跨表写，包裹事务避免半成功脏数据
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var chainId = await _chainRepository.AddAsync(chain);

                // 创建步骤
                foreach (var step in request.Steps)
                {
                    step.Id = 0;
                    step.ChainId = chainId;
                    step.CreatedTime = DateTime.Now;
                    step.LastModifiedTime = DateTime.Now;
                }

                if (request.Steps.Count > 0)
                    await _stepRepository.AddAsync(request.Steps);

                await _unitOfWork.CommitAsync();
                return chainId;
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> UpdateAsync(StrategyChainUpdateRequest request)
        {
            var chain = request.Chain;
            ValidateChainType(chain.ChainType);

            if (!await IsCodeUniqueAsync(chain.ChainCode, chain.Id))
                throw new InvalidOperationException($"策略链编码 {chain.ChainCode} 已被使用");

            ValidateSteps(request.Steps);

            chain.LastModifiedTime = DateTime.Now;
            chain.StepCount = request.Steps.Count;

            // #32：主表更新+步骤全量替换，包裹事务避免半成功脏数据
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                await _chainRepository.UpdateAsync(chain);

                // 删除旧步骤，重新创建（全量替换）
                await _stepRepository.DeleteAsync(s => s.ChainId == chain.Id);

                foreach (var step in request.Steps)
                {
                    step.Id = 0;
                    step.ChainId = chain.Id;
                    step.CreatedTime = DateTime.Now;
                    step.LastModifiedTime = DateTime.Now;
                }

                if (request.Steps.Count > 0)
                    await _stepRepository.AddAsync(request.Steps);

                await _unitOfWork.CommitAsync();
                return true;
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public override async Task<ApiResponse> DeleteAsync(long id)
        {
            // #32：删步骤+删主表，包裹事务避免孤儿步骤
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                await _stepRepository.DeleteAsync(s => s.ChainId == id);
                var ok = await _chainRepository.DeleteAsync(id);
                await _unitOfWork.CommitAsync();
                return ok ? ApiResponse.Ok() : ApiResponse.Error("删除失败");
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> IsCodeUniqueAsync(string chainCode, long? excludeId = null)
        {
            if (excludeId.HasValue && excludeId.Value > 0)
            {
                return !await _chainRepository.ExistsAsync(c =>
                    c.ChainCode == chainCode && c.Id != excludeId.Value);
            }
            return !await _chainRepository.ExistsAsync(c =>
                c.ChainCode == chainCode);
        }

        /// <summary>
        /// 校验指定类型策略链的组成是否包含所有必需的策略类型步骤。
        /// 例如出库链(OutboundAllocation)必须同时含 OutboundAllocation(选分区) 与 InventoryAllocation(选批次) 两步，
        /// 否则出库分配读不到库存分配结果。
        /// </summary>
        public async Task<List<string>> ValidateChainCompositionAsync(string chainType, params string[] requiredStrategyTypes)
        {
            var warnings = new List<string>();
            var chains = await GetByTypeAsync(chainType);
            if (chains.Count == 0)
            {
                warnings.Add($"未配置任何 {chainType} 类型的策略链");
                return warnings;
            }

            var configRepo = _unitOfWork.GetRepository<CfgStrategyConfig, long>();
            foreach (var chain in chains)
            {
                var steps = await GetStepsByChainIdAsync(chain.Id);
                var configIds = steps.Where(s => s.IsEnabled == 1).Select(s => s.StrategyConfigId).Distinct().ToList();
                if (configIds.Count == 0)
                {
                    warnings.Add($"链 {chain.ChainCode}({chain.ChainName}) 未配置任何启用的步骤");
                    continue;
                }

                var configs = await configRepo.GetListAsync(c => configIds.Contains(c.Id));
                var types = configs.Select(c => c.StrategyType).Distinct().ToHashSet(StringComparer.OrdinalIgnoreCase);
                foreach (var req in requiredStrategyTypes)
                {
                    if (!types.Contains(req))
                        warnings.Add($"链 {chain.ChainCode}({chain.ChainName}) 缺少 {req} 类型策略步骤（当前含: {(types.Count == 0 ? "无" : string.Join("/", types))}）");
                }
            }
            return warnings;
        }

        private static void ValidateChainType(string chainType)
        {
            if (!ValidChainTypes.Contains(chainType))
                throw new ArgumentException($"无效的策略链类型: {chainType}，有效值: {string.Join(", ", ValidChainTypes)}");
        }

        private void ValidateSteps(List<CfgStrategyChainStep> steps)
        {
            // 检查步骤号唯一性
            var stepNos = steps.Select(s => s.StepNo).ToList();
            if (stepNos.Distinct().Count() != stepNos.Count)
                throw new InvalidOperationException("步骤号不能重复");

            // 检查步骤号连续性
            if (steps.Count > 0)
            {
                var sorted = steps.OrderBy(s => s.StepNo).ToList();
                for (int i = 0; i < sorted.Count; i++)
                {
                    if (sorted[i].StepNo != i + 1)
                        throw new InvalidOperationException("步骤号必须从1开始连续编号");
                }
            }

            // 验证每个步骤的参数JSON
            foreach (var step in steps)
            {
                if (!string.IsNullOrWhiteSpace(step.StepParams))
                {
                    try
                    {
                        JsonDocument.Parse(step.StepParams);
                    }
                    catch (JsonException ex)
                    {
                        throw new ArgumentException($"步骤 {step.StepNo} 的参数JSON格式不正确: {ex.Message}");
                    }
                }
            }
        }
    }
}
