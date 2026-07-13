using KH.WMS.Core.Api.Responses;
using KH.WMS.Core.Caching;
using KH.WMS.Core.Constants;
using KH.WMS.Core.Database.Repositories;
using KH.WMS.Core.Database.UnitOfWorks;
using KH.WMS.Core.DependencyInjection.ServiceLifetimes;
using KH.WMS.Core.Services;
using KH.WMS.Entities.Constants;
using KH.WMS.Entities.System;
using KH.WMS.Modules.SystemModule.DTOs;
using KH.WMS.Modules.SystemModule.Interfaces;
using ICacheService = KH.WMS.Core.Caching.ICacheService;

namespace KH.WMS.Modules.SystemModule.Services
{
    /// <summary>
    /// 系统参数服务实现
    /// </summary>
    [RegisteredService(ServiceType = typeof(ISysParameterService))]
    public class SysParameterService(
        IRepository<SysParameter, long> parameterRepository,
        ICacheService cacheService,
        IUnitOfWork unitOfWork,
        IDetailSaveService detailSaveService) : CrudService<SysParameter>(parameterRepository, unitOfWork, detailSaveService), ISysParameterService
    {
        private readonly IRepository<SysParameter, long> _parameterRepository = parameterRepository;
        private readonly ICacheService _cacheService = cacheService;

        /// <inheritdoc />
        public async Task WarmUpAsync()
        {
            var allParams = await _parameterRepository.GetAllAsync();
            var enabledParams = allParams.Where(p => p.Status == BizConstants.BoolFlag.YES);
            var existingCodes = allParams.Select(p => p.ParamCode).ToHashSet();

            // 启动时验证数据库是否存在必需的系统内置参数，缺失则自动创建
            foreach (var (paramCode, (paramName, defaultValue)) in BizConstants.ParamCodes.RequiredParams)
            {
                if (!existingCodes.Contains(paramCode))
                {
                    await _parameterRepository.AddAsync(new SysParameter
                    {
                        ParamCode = paramCode,
                        ParamName = paramName,
                        ParamValue = defaultValue,
                        SystemFlag = 1,
                        Status = BizConstants.BoolFlag.YES,
                    });
                }
            }

            // 重新加载（含刚创建的参数）
            allParams = await _parameterRepository.GetAllAsync();
            enabledParams = allParams.Where(p => p.Status == BizConstants.BoolFlag.YES);

            foreach (var param in enabledParams)
            {
                var cacheKey = CacheConstants.SysParam.GetKey(param.ParamCode);
                _cacheService.Set(cacheKey, param.ParamValue, null); // 不过期
            }

            // 用默认值填充缓存中仍缺失的参数
            foreach (var (paramCode, defaultValue) in CacheConstants.SysParam.Defaults)
            {
                var cacheKey = CacheConstants.SysParam.GetKey(paramCode);
                if (!_cacheService.Exists(cacheKey))
                {
                    _cacheService.Set(cacheKey, defaultValue, null);
                }
            }
        }

        public async Task<ApiResponse> GetParameterGroupsAsync()
        {
            var all = await _parameterRepository.GetAllAsync();
            var groups = all
                .Where(p => !string.IsNullOrWhiteSpace(p.ParamGroup))
                .Select(p => p.ParamGroup!)
                .Distinct()
                .OrderBy(g => g)
                .ToList();
            return ApiResponse.Ok(groups);
        }

        public async Task<ApiResponse> SaveParameterAsync(SaveParameterDto dto)
        {
            var exists = await _parameterRepository.ExistsAsync(
                p => p.ParamCode == dto.ParamCode &&
                     (dto.Id == null || p.Id != dto.Id.Value));
            if (exists)
                return ApiResponse.Fail(ResponseCode.BAD_REQUEST, "参数编码已存在");

            if (dto.Id == null)
            {
                var entity = new SysParameter
                {
                    ParamCode = dto.ParamCode,
                    ParamName = dto.ParamName,
                    ParamValue = dto.ParamValue,
                    ParamGroup = dto.ParamGroup,
                    ParamType = dto.ParamType,
                    Remark = dto.Remark,
                    SystemFlag = 0,
                    Status = BizConstants.BoolFlag.YES
                };
                var id = await _parameterRepository.AddAsync(entity);
                // 同步到缓存
                _cacheService.Set(CacheConstants.SysParam.GetKey(entity.ParamCode), entity.ParamValue, null);
                return ApiResponse.Ok(id, "新增成功");
            }
            else
            {
                var entity = await _parameterRepository.GetByIdAsync(dto.Id.Value);
                if (entity == null)
                    return ApiResponse.NotFound("参数不存在");

                // 编辑时只允许修改参数键值，其他字段不可修改
                entity.ParamValue = dto.ParamValue;

                await _parameterRepository.UpdateAsync(entity);
                // 同步到缓存
                _cacheService.Set(CacheConstants.SysParam.GetKey(entity.ParamCode), entity.ParamValue, null);
                return ApiResponse.Ok(message: "更新成功");
            }
        }

        public async Task<ApiResponse> DeleteParameterAsync(long id)
        {
            var param = await _parameterRepository.GetByIdAsync(id);
            if (param == null)
                return ApiResponse.NotFound("参数不存在");

            // 系统内置参数不允许删除
            if (param.SystemFlag == 1)
                return ApiResponse.Fail(ResponseCode.BAD_REQUEST, "系统内置参数不允许删除");

            await _parameterRepository.DeleteAsync(id);
            // 从缓存移除
            _cacheService.Remove(CacheConstants.SysParam.GetKey(param.ParamCode));
            return ApiResponse.Ok(message: "删除成功");
        }

        public async Task<ApiResponse> GetParameterByCodeAsync(string paramCode)
        {
            var param = await _parameterRepository.GetFirstOrDefaultAsync(p => p.ParamCode == paramCode);
            if (param == null)
                return ApiResponse.NotFound($"参数 {paramCode} 不存在");

            return ApiResponse.Ok(new { param.ParamCode, param.ParamName, param.ParamValue });
        }
    }
}
