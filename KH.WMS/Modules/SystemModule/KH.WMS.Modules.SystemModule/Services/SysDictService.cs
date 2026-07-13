using System.Data;
using System.Text.RegularExpressions;
using KH.WMS.Core.Api.Responses;
using KH.WMS.Core.Attributes;
using KH.WMS.Core.Constants;
using KH.WMS.Core.Database.Repositories;
using KH.WMS.Core.Database.SqlSugar;
using KH.WMS.Core.Database.UnitOfWorks;
using KH.WMS.Core.DependencyInjection.ServiceLifetimes;
using KH.WMS.Core.Helpers;
using KH.WMS.Core.Models.Dtos;
using KH.WMS.Core.Services;
using KH.WMS.Entities.Constants;
using KH.WMS.Entities.System;
using KH.WMS.Entities.System.Enums;
using KH.WMS.Modules.SystemModule.DTOs;
using KH.WMS.Modules.SystemModule.Interfaces;
using SqlSugar;
using CacheService = KH.WMS.Core.Caching.ICacheService;

namespace KH.WMS.Modules.SystemModule.Services
{
    /// <summary>
    /// 系统字典服务实现
    /// </summary>
    [RegisteredService(ServiceType = typeof(ISysDictService))]
    public class SysDictService(
        IRepository<SysDictType, long> dictTypeRepository,
        IRepository<SysDictItem, long> dictItemRepository,
        ISqlSugarClient db,
        CacheService cacheService,
        IUnitOfWork unitOfWork,
        IDetailSaveService detailSaveService) : CrudService<SysDictType>(dictTypeRepository, unitOfWork, detailSaveService), ISysDictService
    {
        private readonly ISqlSugarClient _db = db;
        private readonly IRepository<SysDictType, long> _dictTypeRepository = dictTypeRepository;
        private readonly IRepository<SysDictItem, long> _dictItemRepository = dictItemRepository;
        private readonly CacheService _cacheService = cacheService;

        /// <inheritdoc />
        [Cache]
        public async Task<ApiResponse> GetDictItemsAsync(string dictCode)
        {
            var dictType = await _dictTypeRepository.GetFirstOrDefaultAsync(
                dt => dt.DictCode == dictCode && dt.IsActive == BizConstants.BoolFlag.YES);
            if (dictType == null)
                return ApiResponse.Ok();

            return dictType.DataSourceType switch
            {
                (byte)DictDataSourceTypeEnum.Static => await GetStaticDictItemsAsync(dictType),
                (byte)DictDataSourceTypeEnum.Sql => await GetSqlDictItemsAsync(dictType),
                _ => ApiResponse.Fail(ResponseCode.BAD_REQUEST, "未知的数据来源类型")
            };
        }

        public async Task<ApiResponse> GetDictItemsList(int dictId, AdvancedQueryRequestDto query)
        {
            var dictType = await _dictTypeRepository.GetFirstOrDefaultAsync(
                dt => dt.Id == dictId && dt.IsActive == BizConstants.BoolFlag.YES);
            if (dictType == null)
                return ApiResponse.Ok();

            return dictType.DataSourceType switch
            {
                (byte)DictDataSourceTypeEnum.Static => await GetDictItemsByDictId(dictType.Id, query),
                (byte)DictDataSourceTypeEnum.Sql => await GetSqlDictItemsAsync(dictType),
                _ => ApiResponse.Fail(ResponseCode.BAD_REQUEST, "未知的数据来源类型")
            };
        }

        public override async Task<ApiResponse> CreateAsync(SysDictType dto)
        {
            // 编码唯一性检查
            var exists = await _dictTypeRepository.ExistsAsync(dt => dt.DictCode == dto.DictCode);
            if (exists)
                return ApiResponse.Fail(ResponseCode.BAD_REQUEST, "字典编码已存在");

            // SQL数据源时验证必填字段和SQL安全性
            if (dto.DataSourceType == (byte)DictDataSourceTypeEnum.Sql)
            {
                var validation = ValidateSqlConfig(dto);
                if (!validation.IsValid)
                    return ApiResponse.Fail(ResponseCode.BAD_REQUEST, validation.ErrorMessage!);
            }

            var entity = new SysDictType
            {
                DictCode = dto.DictCode,
                DictName = dto.DictName,
                DataSourceType = dto.DataSourceType,
                SqlQuery = dto.SqlQuery,
                ValueColumn = dto.ValueColumn,
                LabelColumn = dto.LabelColumn,
                CacheMinutes = dto.CacheMinutes,
                IsActive = dto.IsActive,
                Remark = dto.Remark
            };
            var id = await _dictTypeRepository.AddAsync(entity);
            return ApiResponse.Ok(id, "新增成功");
        }

        public override async Task<ApiResponse> UpdateAsync(SysDictType dto)
        {
            var entity = await _dictTypeRepository.GetByIdAsync(dto.Id);
            if (entity == null)
                return ApiResponse.NotFound("字典类型不存在");

            // 编码唯一性检查（排除自身）
            var exists = await _dictTypeRepository.ExistsAsync(
                dt => dt.DictCode == dto.DictCode && dt.Id != dto.Id);
            if (exists)
                return ApiResponse.Fail(ResponseCode.BAD_REQUEST, "字典编码已存在");

            // SQL数据源时验证必填字段和SQL安全性
            if (dto.DataSourceType == (byte)DictDataSourceTypeEnum.Sql)
            {
                var validation = ValidateSqlConfig(dto);
                if (!validation.IsValid)
                    return ApiResponse.Fail(ResponseCode.BAD_REQUEST, validation.ErrorMessage!);
            }

            entity.DictCode = dto.DictCode;
            entity.DictName = dto.DictName;
            entity.DataSourceType = dto.DataSourceType;
            entity.SqlQuery = dto.SqlQuery;
            entity.ValueColumn = dto.ValueColumn;
            entity.LabelColumn = dto.LabelColumn;
            entity.CacheMinutes = dto.CacheMinutes;
            entity.IsActive = dto.IsActive;
            entity.Remark = dto.Remark;

            await _dictTypeRepository.UpdateAsync(entity);

            // 清除该字典的缓存
            ClearDictCache(entity.Id, entity.DictCode);
            return ApiResponse.Ok(message: "更新成功");
        }

        private async Task<ApiResponse> GetDictItemsByDictId(long dictId, AdvancedQueryRequestDto query)
        {
            var expression = BuildQueryExpression<SysDictItem>(query);

            var filterExpression = ExpressionHelper.BuildFilter<SysDictItem>(query.Filters);

            var combinedExpression = expression.And(filterExpression);

            var queryable = _db.Queryable<SysDictItem>().Where(combinedExpression.And(x => x.DictTypeId == dictId));
            queryable = ApplyAdditionalQuery(queryable, query);
            queryable = ApplySorting(queryable, query.SortConditions);

            RefAsync<int> total = 0;
            var items = await queryable.ToPageListAsync(query.PageIndex, query.PageSize, total);

            return ApiResponse.Ok(new { items, total = total.Value });
        }

        /// <inheritdoc />
        public async Task<ApiResponse> DeleteDictTypeAsync(long dictTypeId)
        {
            // 检查是否存在关联的字典项
            var hasItems = await _dictItemRepository.ExistsAsync(i => i.DictTypeId == dictTypeId);
            if (hasItems)
                return ApiResponse.Fail(ResponseCode.BAD_REQUEST, "存在关联的字典项，请先删除字典项");

            var deleted = await _dictTypeRepository.DeleteAsync(dictTypeId);
            if (!deleted)
                return ApiResponse.NotFound("字典类型不存在");

            // 清除缓存
            _cacheService.Remove(CacheConstants.Config.GetDictKey($"static:{dictTypeId}"));
            _cacheService.Remove(CacheConstants.Config.GetDictKey($"sql:{dictTypeId}"));

            return ApiResponse.Ok(message: "删除成功");
        }

        /// <inheritdoc />
        public Task<ApiResponse> ClearDictCacheAsync(string? dictCode = null)
        {
            if (string.IsNullOrWhiteSpace(dictCode))
            {
                // 清除所有字典缓存（通过移除 Config 前缀下的所有缓存）
                _cacheService.RemoveMultiple(new[] { CacheConstants.Config.DICT });
            }
            else
            {
                _cacheService.Remove(CacheConstants.Config.GetDictKey(dictCode));
            }
            return Task.FromResult(ApiResponse.Ok(message: "字典缓存已清除"));
        }

        #region Private Methods

        private async Task<ApiResponse> GetStaticDictItemsAsync(SysDictType dictType)
        {
            var cacheKey = CacheConstants.Config.GetDictKey($"static:{dictType.Id}");
            var cached = _cacheService.Get<List<DictItemDto>>(cacheKey);
            if (cached != null)
                return ApiResponse.Ok(cached);

            var items = await _dictItemRepository.GetListAsync(
                i => i.DictTypeId == dictType.Id && i.IsActive == BizConstants.BoolFlag.YES);

            var result = items.OrderBy(i => i.SortOrder).Select(i => new DictItemDto
            {
                ItemValue = i.ItemValue,
                ItemLabel = i.ItemLabel,
                TagColor = i.TagColor,
                SortOrder = i.SortOrder
            }).ToList();

            _cacheService.Set(cacheKey, result, TimeSpan.FromMinutes(30));
            return ApiResponse.Ok(result);
        }

        private async Task<ApiResponse> GetSqlDictItemsAsync(SysDictType dictType)
        {
            // SQL注入防护验证
            var validation = ValidateSqlQuery(dictType);
            if (!validation.IsValid)
                return ApiResponse.Fail(ResponseCode.BAD_REQUEST, validation.ErrorMessage!);

            // 缓存策略
            var cacheKey = CacheConstants.Config.GetDictKey($"sql:{dictType.Id}");
            if (dictType.CacheMinutes > 0)
            {
                var cached = _cacheService.Get<List<DictItemDto>>(cacheKey);
                if (cached != null)
                    return ApiResponse.Ok(cached);
            }

            // 执行SQL查询
            try
            {
                // 自动检测 SQL 中引用的表名，配置表（cfg_/log_）路由到配置库
                var queryDb = ContainsConfigTable(dictType.SqlQuery!)
                    ? ((SqlSugarClient)db).GetConnection(SqlSugarSetup.ConfigDb)
                    : _db;
                queryDb.Ado.CommandTimeOut = 10;
                var dataTable = await queryDb.Ado.GetDataTableAsync(dictType.SqlQuery!);

                // 列映射
                var result = new List<DictItemDto>();
                foreach (DataRow row in dataTable.Rows)
                {
                    var value = row[dictType.ValueColumn!]?.ToString() ?? "";
                    var label = row[dictType.LabelColumn!]?.ToString() ?? "";
                    result.Add(new DictItemDto { ItemValue = value, ItemLabel = label });
                }

                // 缓存结果
                if (dictType.CacheMinutes > 0)
                {
                    _cacheService.Set(cacheKey, result, TimeSpan.FromMinutes(dictType.CacheMinutes));
                }

                return ApiResponse.Ok(result);
            }
            catch (Exception ex)
            {
                return ApiResponse.Error($"执行字典SQL查询失败: {ex.Message}");
            }
        }

        /// <summary>
        /// SQL注入防护验证
        /// </summary>
        private static (bool IsValid, string? ErrorMessage) ValidateSqlQuery(SysDictType dictType)
        {
            if (string.IsNullOrWhiteSpace(dictType.SqlQuery))
                return (false, "SQL查询语句不能为空");
            if (string.IsNullOrWhiteSpace(dictType.ValueColumn))
                return (false, "值列名不能为空");
            if (string.IsNullOrWhiteSpace(dictType.LabelColumn))
                return (false, "标签列名不能为空");

            var sql = dictType.SqlQuery.Trim().ToUpperInvariant();

            // 规则1：只允许 SELECT 开头
            if (!sql.StartsWith("SELECT"))
                return (false, "只允许 SELECT 查询语句");

            // 规则2：禁止危险关键字
            var forbiddenKeywords = new[]
            {
                "INSERT", "UPDATE", "DELETE", "DROP", "TRUNCATE",
                "ALTER", "CREATE", "EXEC", "EXECUTE", "XP_", "SP_",
                "GRANT", "REVOKE", "INTO ", "SET ", "DECLARE", "CURSOR"
            };

            foreach (var keyword in forbiddenKeywords)
            {
                if (sql.Contains(keyword))
                    return (false, $"SQL中不允许包含关键字: {keyword.Trim()}");
            }

            // 规则3：禁止分号（防止多语句注入）
            if (dictType.SqlQuery.Contains(';'))
                return (false, "SQL中不允许包含分号");

            // 规则4：禁止注释符号
            if (dictType.SqlQuery.Contains("--") || dictType.SqlQuery.Contains("/*"))
                return (false, "SQL中不允许包含注释");

            // 规则5：列名只允许字母、数字、下划线
            var columnPattern = @"^[a-zA-Z_][a-zA-Z0-9_]*$";
            if (!Regex.IsMatch(dictType.ValueColumn, columnPattern))
                return (false, "值列名格式不合法，只允许字母、数字和下划线");
            if (!Regex.IsMatch(dictType.LabelColumn, columnPattern))
                return (false, "标签列名格式不合法，只允许字母、数字和下划线");

            return (true, null);
        }

        /// <summary>
        /// 验证保存请求中的SQL配置
        /// </summary>
        private static (bool IsValid, string? ErrorMessage) ValidateSqlConfig(SysDictType dto)
        {
            if (string.IsNullOrWhiteSpace(dto.SqlQuery))
                return (false, "SQL查询语句不能为空");
            if (string.IsNullOrWhiteSpace(dto.ValueColumn))
                return (false, "值列名不能为空");
            if (string.IsNullOrWhiteSpace(dto.LabelColumn))
                return (false, "标签列名不能为空");

            var sql = dto.SqlQuery.Trim().ToUpperInvariant();
            if (!sql.StartsWith("SELECT"))
                return (false, "只允许 SELECT 查询语句");

            var forbiddenKeywords = new[]
            {
                "INSERT", "UPDATE", "DELETE", "DROP", "TRUNCATE",
                "ALTER", "CREATE", "EXEC", "EXECUTE", "XP_", "SP_",
                "GRANT", "REVOKE", "INTO ", "SET ", "DECLARE", "CURSOR"
            };
            foreach (var keyword in forbiddenKeywords)
            {
                if (sql.Contains(keyword))
                    return (false, $"SQL中不允许包含关键字: {keyword.Trim()}");
            }

            if (dto.SqlQuery.Contains(';'))
                return (false, "SQL中不允许包含分号");
            if (dto.SqlQuery.Contains("--") || dto.SqlQuery.Contains("/*"))
                return (false, "SQL中不允许包含注释");

            var columnPattern = @"^[a-zA-Z_][a-zA-Z0-9_]*$";
            if (!Regex.IsMatch(dto.ValueColumn, columnPattern))
                return (false, "值列名格式不合法，只允许字母、数字和下划线");
            if (!Regex.IsMatch(dto.LabelColumn, columnPattern))
                return (false, "标签列名格式不合法，只允许字母、数字和下划线");

            return (true, null);
        }

        private void ClearDictCache(long dictTypeId, string dictCode)
        {
            _cacheService.Remove(CacheConstants.Config.GetDictKey($"static:{dictTypeId}"));
            _cacheService.Remove(CacheConstants.Config.GetDictKey($"sql:{dictTypeId}"));
            _cacheService.Remove(CacheConstants.Config.GetDictKey(dictCode));
        }

        /// <summary>
        /// 检测 SQL 中是否引用了配置表（cfg_ 或 log_ 开头的表名）
        /// </summary>
        private static readonly Regex ConfigTableRegex = new(@"\b(cfg_|log_)\w+", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static bool ContainsConfigTable(string sql)
        {
            return ConfigTableRegex.IsMatch(sql);
        }

        /// <inheritdoc />
        public async Task<ApiResponse> GetDictItemListAsync(long dictTypeId)
        {
            var items = await _dictItemRepository.GetListAsync(i => i.DictTypeId == dictTypeId);
            var result = items.OrderBy(i => i.SortOrder).Select(i => new
            {
                i.Id,
                i.DictTypeId,
                i.ItemValue,
                i.ItemLabel,
                i.TagColor,
                i.SortOrder,
                i.IsActive,
                i.Remark
            });
            return ApiResponse.Ok(result);
        }

        /// <inheritdoc />
        public async Task<ApiResponse> SaveDictItemAsync(SaveDictItemDto dto)
        {
            // 验证字典类型存在
            var dictType = await _dictTypeRepository.GetByIdAsync(dto.DictTypeId);
            if (dictType == null)
                return ApiResponse.NotFound("字典类型不存在");

            if (dto.Id == null)
            {
                var entity = new SysDictItem
                {
                    DictTypeId = dto.DictTypeId,
                    ItemValue = dto.ItemValue,
                    ItemLabel = dto.ItemLabel,
                    TagColor = dto.TagColor,
                    SortOrder = dto.SortOrder,
                    IsActive = dto.IsActive,
                    Remark = dto.Remark
                };
                var id = await _dictItemRepository.AddAsync(entity);
                ClearDictCache(dto.DictTypeId, dictType.DictCode);
                return ApiResponse.Ok(id, "新增成功");
            }
            else
            {
                var entity = await _dictItemRepository.GetByIdAsync(dto.Id.Value);
                if (entity == null)
                    return ApiResponse.NotFound("字典项不存在");

                var oldDictTypeId = entity.DictTypeId;
                entity.DictTypeId = dto.DictTypeId;
                entity.ItemValue = dto.ItemValue;
                entity.ItemLabel = dto.ItemLabel;
                entity.TagColor = dto.TagColor;
                entity.SortOrder = dto.SortOrder;
                entity.IsActive = dto.IsActive;
                entity.Remark = dto.Remark;

                await _dictItemRepository.UpdateAsync(entity);
                ClearDictCache(dto.DictTypeId, dictType.DictCode);
                if (oldDictTypeId != dto.DictTypeId)
                {
                    var oldDictType = await _dictTypeRepository.GetByIdAsync(oldDictTypeId);
                    if (oldDictType != null)
                        ClearDictCache(oldDictTypeId, oldDictType.DictCode);
                }
                return ApiResponse.Ok(message: "更新成功");
            }
        }

        /// <inheritdoc />
        public async Task<ApiResponse> DeleteDictItemAsync(long dictItemId)
        {
            var item = await _dictItemRepository.GetByIdAsync(dictItemId);
            if (item == null)
                return ApiResponse.NotFound("字典项不存在");

            await _dictItemRepository.DeleteAsync(dictItemId);

            var dictType = await _dictTypeRepository.GetByIdAsync(item.DictTypeId);
            if (dictType != null)
                ClearDictCache(item.DictTypeId, dictType.DictCode);

            return ApiResponse.Ok(message: "删除成功");
        }

        #endregion
    }
}
