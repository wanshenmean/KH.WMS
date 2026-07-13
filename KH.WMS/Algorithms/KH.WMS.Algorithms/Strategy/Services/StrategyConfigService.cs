using System.Text.Json;
using KH.WMS.Algorithms.Strategy.Configuration;
using KH.WMS.Algorithms.Strategy.Constants;
using KH.WMS.Algorithms.Strategy.Enums;
using KH.WMS.Core.Database.Repositories;
using KH.WMS.Core.Database.UnitOfWorks;
using KH.WMS.Core.DependencyInjection.ServiceLifetimes;
using KH.WMS.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using SqlSugar;

namespace KH.WMS.Algorithms.Strategy.Services
{
    /// <summary>
    /// 策略配置服务实现
    /// </summary>
    [RegisteredService(Lifetime = ServiceLifetime.Scoped, ServiceType = typeof(IStrategyConfigService))]
    public class StrategyConfigService(
        IRepository<CfgStrategyConfig, long> repository,
        IUnitOfWork unitOfWork,
        IDetailSaveService detailSaveService) : CrudService<CfgStrategyConfig>(repository, unitOfWork, detailSaveService), IStrategyConfigService
    {
        private readonly IRepository<CfgStrategyConfig, long> _configRepository = repository;

        private static readonly HashSet<string> ValidStrategyTypes = new(StringComparer.OrdinalIgnoreCase)
        {
              PolicyType.Putaway.ToString(), PolicyType.LocationAllocation.ToString(), PolicyType.Picking.ToString(), PolicyType.InventoryAllocation.ToString(), PolicyType.Wave.ToString()
        };


        public async Task<(List<CfgStrategyConfig> Items, int Total)> GetPagedListAsync(
            int pageIndex, int pageSize,
            string? strategyType = null,
            string? keyword = null,
            long? warehouseId = null,
            byte? status = null)
        {
            return await _configRepository.GetPagedListAsync(pageIndex, pageSize, c =>
                (string.IsNullOrEmpty(strategyType) || c.StrategyType == strategyType)
                && (string.IsNullOrEmpty(keyword)
                    || c.StrategyCode.Contains(keyword)
                    || c.StrategyName.Contains(keyword)
                    || c.RuleCode.Contains(keyword))
                && (!warehouseId.HasValue || c.WarehouseId == null || c.WarehouseId == warehouseId.Value)
                && (!status.HasValue || c.Status == status.Value));
        }

        public async Task<List<CfgStrategyConfig>> GetByTypeAsync(string strategyType, long? warehouseId = null)
        {
            // #33：按优先级降序（接口契约要求"按优先级降序"）
            var query = _configRepository.AsQueryable()
                .Where(c => c.StrategyType == strategyType && c.Status == AlgoConstants.Status.ENABLED);

            if (warehouseId.HasValue && warehouseId.Value > 0)
                query = query.Where(c => c.WarehouseId == null || c.WarehouseId == warehouseId.Value);

            return await query.OrderByDescending(c => c.Priority).ToListAsync();
        }

        public async Task<CfgStrategyConfig?> GetByCodeAsync(string strategyCode)
        {
            return await _configRepository.GetFirstOrDefaultAsync(c => c.StrategyCode == strategyCode);
        }

        /// <summary>
        /// 新增前校验（#10 恢复）：策略类型合法 + 编码唯一 + 参数JSON格式正确。
        /// 通过重写 CrudService 钩子，使 CrudController.Create 端点自动走校验。
        /// </summary>
        protected override async Task BeforeCreateAsync(CfgStrategyConfig entity)
        {
            ValidateStrategyType(entity.StrategyType);

            if (!await IsCodeUniqueAsync(entity.StrategyCode))
                throw new InvalidOperationException($"策略编码 {entity.StrategyCode} 已存在");

            if (!ValidateParams(entity.StrategyParams, out var error))
                throw new ArgumentException(error);
        }

        /// <summary>
        /// 更新前校验（#10 恢复）：策略类型合法 + 编码唯一(排除自身) + 参数JSON格式正确。
        /// </summary>
        protected override async Task BeforeUpdateAsync(CfgStrategyConfig entity)
        {
            ValidateStrategyType(entity.StrategyType);

            if (!await IsCodeUniqueAsync(entity.StrategyCode, entity.Id))
                throw new InvalidOperationException($"策略编码 {entity.StrategyCode} 已被使用");

            if (!ValidateParams(entity.StrategyParams, out var error))
                throw new ArgumentException(error);
        }

        public async Task<bool> DeleteAsync(List<long> ids)
        {
            return await _configRepository.DeleteAsync(ids);
        }

        public bool ValidateParams(string? paramsJson, out string errorMessage)
        {
            errorMessage = string.Empty;
            if (string.IsNullOrWhiteSpace(paramsJson))
                return true;

            try
            {
                JsonDocument.Parse(paramsJson);
                return true;
            }
            catch (JsonException ex)
            {
                errorMessage = $"策略参数JSON格式不正确: {ex.Message}";
                return false;
            }
        }

        public async Task<bool> IsCodeUniqueAsync(string strategyCode, long? excludeId = null)
        {
            return !await _configRepository.ExistsAsync(c =>
                c.StrategyCode == strategyCode
                && (!excludeId.HasValue || c.Id != excludeId.Value));
        }

        private static void ValidateStrategyType(string strategyType)
        {
            if (!ValidStrategyTypes.Contains(strategyType))
                throw new ArgumentException($"无效的策略类型: {strategyType}，有效值: {string.Join(", ", ValidStrategyTypes)}");
        }
    }
}
