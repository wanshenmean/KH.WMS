using System.Reflection;
using System.Linq.Expressions;
using System.Text.Json;
using System.Text.Json.Nodes;
using KH.WMS.Core.Api.Responses;
using KH.WMS.Core.Attributes;
using KH.WMS.Core.Database.Repositories;
using KH.WMS.Core.Database.UnitOfWorks;
using KH.WMS.Core.DependencyInjection.ServiceLifetimes;
using KH.WMS.Core.Exceptions;
using KH.WMS.Core.Helpers;
using KH.WMS.Core.ImportExport;
using KH.WMS.Core.Models.Dtos;
using KH.WMS.Core.Models.Entities;
using Microsoft.Extensions.Logging;
using SqlSugar;

namespace KH.WMS.Core.Services;

/// <summary>
/// CRUD 应用服务基类
/// 提供：分页查询（高级过滤+多字段排序）、增删改、导入导出（MiniExcel）、模板下载
/// 主从表保存通过 IDetailSaveService 实现
/// 子类通过重写钩子方法实现业务定制
/// </summary>
/// <typeparam name="TEntity">实体类型</typeparam>
[RegisteredService(ServiceType = typeof(ICrudService<>)),
 LogInterceptor(LogParameters = true, LogReturnValue = true, LogLevel = LogLevel.Information)]
public class CrudService<TEntity>
    : ApplicationService,
    ICrudService<TEntity>
    where TEntity : BaseEntity<long>, new()
{
    protected readonly IRepository<TEntity, long> _repository;
    protected readonly IUnitOfWork _unitOfWork;
    protected readonly IDetailSaveService? _detailSaveService;

    protected CrudService(
        IRepository<TEntity, long> repository,
        IUnitOfWork unitOfWork,
        IDetailSaveService? detailSaveService = null)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _detailSaveService = detailSaveService;
    }

    #region 查询

    /// <inheritdoc />
    public virtual async Task<ApiResponse> GetByIdAsync(long id)
    {
        var entity = await _repository.GetByIdWithNavAsync(id);
        if (entity == null)
            return ApiResponse.NotFound($"{typeof(TEntity).Name}不存在");

        return ApiResponse.Ok(entity);
    }

    /// <inheritdoc />
    public virtual async Task<ApiResponse> GetPagedListAsync(AdvancedQueryRequestDto query)
    {
        var expression = BuildQueryExpression(query);

        var filterExpression = ExpressionHelper.BuildFilter<TEntity>(query.Filters);
        var combinedExpression = expression.And(filterExpression);

        var queryable = _repository.AsQueryable().Where(combinedExpression);
        queryable = ApplyAdditionalQuery(queryable, query);
        queryable = ApplySorting(queryable, query.SortConditions);

        RefAsync<int> total = 0;
        var items = await queryable.ToPageListAsync(query.PageIndex, query.PageSize, total);

        items = await AfterQueryAsync(query, items);

        return ApiResponse.Ok(new { items, total = total.Value });
    }

    /// <inheritdoc />
    public virtual async Task<ApiResponse> GetListAsync()
    {
        var expression = BuildListExpression();
        var entities = await _repository.GetListAsync(expression);
        return ApiResponse.Ok(entities);
    }

    #endregion

    #region 增删改

    /// <inheritdoc />
    public virtual async Task<ApiResponse> CreateAsync(TEntity entity)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            await BeforeCreateAsync(entity);

            FillAuditFields(entity, isCreate: true);

            // 插入主表（返回主键ID）
            var id = await _repository.AddAsync(entity);
            entity.Id = id;

            // OneToMany：通过 DetailSaveService 逐条保存（支持增量比对）
            // OneToOne：通过 DetailSaveService 保存单一子实体
            if (_detailSaveService != null)
            {
                await _detailSaveService.SaveDetailsAsync(entity, isCreate: true);
                await _detailSaveService.SaveOneToOneAsync(entity, isCreate: true);
            }

            await AfterCreateAsync(entity);

            await _unitOfWork.CommitAsync();
            return ApiResponse.Ok(id, "新增成功");
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    /// <inheritdoc />
    public virtual async Task<ApiResponse> UpdateAsync(TEntity entity)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var existing = await GetEntityOrThrowAsync(entity.Id);

            await BeforeUpdateAsync(entity);

            CopyProperties(entity, existing);
            CopyDetailProperties(entity, existing);
            FillAuditFields(existing, isCreate: false);

            // 更新主表
            await _repository.UpdateAsync(existing);

            // OneToMany：通过 DetailSaveService 逐条保存（支持增量比对：新增/修改/删除）
            // OneToOne：通过 DetailSaveService 保存单一子实体
            if (_detailSaveService != null)
            {
                await _detailSaveService.SaveDetailsAsync(existing, isCreate: false);
                await _detailSaveService.SaveOneToOneAsync(existing, isCreate: false);
            }

            await AfterUpdateAsync(existing);

            await _unitOfWork.CommitAsync();
            return ApiResponse.Ok(message: "更新成功");
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }
    }

    /// <inheritdoc />
    public virtual async Task<ApiResponse> DeleteAsync(long id)
    {
        var hasCascadeNav = HasCascadeNavigationProperties();

        // 有级联导航属性时需要加载完整实体（含 OneToMany/OneToOne 子数据），以便级联删除
        var entity = hasCascadeNav
            ? await _repository.GetByIdWithNavAsync(id)
            : await _repository.GetByIdAsync(id);

        if (entity == null)
            return ApiResponse.NotFound($"{typeof(TEntity).Name}不存在");

        // 删除（含级联子表、BeforeDeleteAsync 钩子里的引用清理）需在同一事务，避免半删导致脏数据
        await _unitOfWork.BeginTransactionAsync();
        try
        {
            await BeforeDeleteAsync(id, entity);

            // 有级联导航属性时使用 DeleteNav 级联删除，否则只删主表
            if (hasCascadeNav)
                await _repository.DeleteWithNavAsync(entity);
            else
                await _repository.DeleteAsync(id);

            await AfterDeleteAsync(id, entity);

            await _unitOfWork.CommitAsync();
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }

        return ApiResponse.Ok(message: "删除成功");
    }

    /// <inheritdoc />
    public virtual async Task<ApiResponse> BatchDeleteAsync(List<long> ids)
    {
        if (ids == null || ids.Count == 0)
            return ApiResponse.Fail(ResponseCode.BAD_REQUEST, "请选择要删除的数据");

        await _unitOfWork.BeginTransactionAsync();
        try
        {
            await BeforeBatchDeleteAsync(ids);

            await _repository.DeleteAsync(ids);

            await AfterBatchDeleteAsync(ids);

            await _unitOfWork.CommitAsync();
        }
        catch
        {
            await _unitOfWork.RollbackAsync();
            throw;
        }

        return ApiResponse.Ok(message: "删除成功");
    }

    #endregion

    #region 启用禁用

    /// <inheritdoc />
    public virtual async Task<ApiResponse> SetStatusAsync(long id, byte status)
    {
        if (!typeof(IEnableDisableEntity).IsAssignableFrom(typeof(TEntity)))
            return ApiResponse.Fail(ResponseCode.BAD_REQUEST, "该实体不支持启用禁用操作");

        var entity = await GetEntityOrThrowAsync(id);

        await BeforeSetStatusAsync(entity, status);

        // 优先级：[StatusFieldName] 特性 > Status > IsActive
        var statusProp = ResolveStatusProperty();
        if (statusProp == null)
            return ApiResponse.Fail(ResponseCode.INTERNAL_SERVER_ERROR, "未找到状态字段");

        var currentValue = statusProp.GetValue(entity);
        if (currentValue?.ToString() == status.ToString())
            return ApiResponse.Fail(ResponseCode.BAD_REQUEST, status == 1 ? "当前已启用" : "当前已禁用");

        if (statusProp.PropertyType == typeof(byte))
            statusProp.SetValue(entity, status);
        else if (statusProp.PropertyType == typeof(int))
            statusProp.SetValue(entity, (int)status);

        await _repository.UpdateAsync(entity);

        await AfterSetStatusAsync(entity, status);

        return ApiResponse.Ok(message: status == 1 ? "已启用" : "已禁用");
    }

    /// <summary>
    /// 查找实体的状态属性
    /// 优先级：[StatusFieldName] 特性 > StatusFieldNames.Status > StatusFieldNames.IsActive
    /// </summary>
    private static PropertyInfo? ResolveStatusProperty()
    {
        var entityType = typeof(TEntity);

        // 1. [StatusFieldName] 特性指定
        var attr = entityType.GetCustomAttribute<StatusFieldNameAttribute>();
        if (attr != null)
            return entityType.GetProperty(attr.FieldName);

        // 2. 默认约定：Status > IsActive
        return entityType.GetProperty(StatusFieldNames.Status)
            ?? entityType.GetProperty(StatusFieldNames.IsActive);
    }

    #endregion

    #region 启用禁用钩子

    /// <summary>
    /// 设置状态前处理（子类可重写做业务校验，如：有库存不能禁用仓库）
    /// </summary>
    protected virtual Task BeforeSetStatusAsync(TEntity entity, byte status)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// 设置状态后处理
    /// </summary>
    protected virtual Task AfterSetStatusAsync(TEntity entity, byte status)
    {
        return Task.CompletedTask;
    }

    #endregion

    #region 导入导出

    /// <inheritdoc />
    public virtual async Task<ApiResponse> ExportAsync(AdvancedQueryRequestDto query, List<ExportColumnDto>? columns = null, bool exportAll = true)
    {
        await BeforeExportAsync(query);

        var expression = BuildQueryExpression(query);
        var filterExpression = ExpressionHelper.BuildFilter<TEntity>(query.Filters);
        var combinedExpression = expression.And(filterExpression);

        var queryable = _repository.AsQueryable().Where(combinedExpression);
        queryable = ApplyAdditionalQuery(queryable, query);
        queryable = ApplySorting(queryable, query.SortConditions);

        List<TEntity> items;
        if (!exportAll)
        {
            RefAsync<int> total = 0;
            items = await queryable.ToPageListAsync(query.PageIndex, query.PageSize, total);
        }
        else
        {
            items = await queryable.ToListAsync();
        }

        // 有列配置时：生成中文表头 + 展开ExtData + 翻译字典值
        if (columns != null && columns.Count > 0)
        {
            var rows = TransformToExportRows(items, columns);
            var bytes = await ExcelHelper.ExportAsync(rows, GetExportFileName());
            return ApiResponse.Ok(Convert.ToBase64String(bytes), "导出成功");
        }

        // 无列配置时：保持原有行为，直接导出实体
        items = TransformExportData(items);
        var entityBytes = await ExcelHelper.ExportAsync(items, GetExportFileName());
        return ApiResponse.Ok(Convert.ToBase64String(entityBytes), "导出成功");
    }

    /// <inheritdoc />
    public virtual async Task<ApiResponse> ImportAsync(Stream fileStream, string fileName)
    {
        var rows = ExcelHelper.ImportAsync<TEntity>(fileStream);

        rows = await BeforeImportAsync(rows);

        var successCount = 0;
        var errors = new List<string>();

        for (int i = 0; i < rows.Count; i++)
        {
            try
            {
                await CreateAsync(rows[i]);
                successCount++;
            }
            catch (Exception ex)
            {
                errors.Add($"第{i + 2}行: {ex.Message}");
            }
        }

        await AfterImportAsync(successCount, errors);

        return ApiResponse.Ok(new { successCount, failCount = errors.Count, errors }, "导入完成");
    }

    /// <inheritdoc />
    public virtual async Task<ApiResponse> DownloadTemplateAsync()
    {
        var bytes = await ExcelHelper.GenerateTemplateAsync<TEntity>(GetExportFileName());
        return ApiResponse.Ok(Convert.ToBase64String(bytes), "模板下载成功");
    }

    #endregion

    #region 查询钩子

    /// <summary>
    /// 构建分页查询表达式（子类重写以添加业务过滤条件）
    /// </summary>
    protected virtual Expression<Func<TEntity, bool>> BuildQueryExpression(AdvancedQueryRequestDto query)
    {
        return ExpressionHelper.True<TEntity>();
    }

    protected virtual Expression<Func<T, bool>> BuildQueryExpression<T>(AdvancedQueryRequestDto query)
    {
        return ExpressionHelper.True<T>();
    }

    /// <summary>
    /// 构建 GetList 查询表达式
    /// </summary>
    protected virtual Expression<Func<TEntity, bool>> BuildListExpression()
    {
        return ExpressionHelper.True<TEntity>();
    }

    protected virtual Expression<Func<T, bool>> BuildListExpression<T>()
    {
        return ExpressionHelper.True<T>();
    }

    /// <summary>
    /// 对 queryable 做额外处理（联表查询、附加条件等）
    /// </summary>
    protected virtual ISugarQueryable<TEntity> ApplyAdditionalQuery(
        ISugarQueryable<TEntity> queryable, AdvancedQueryRequestDto query)
    {
        return queryable;
    }

    protected virtual ISugarQueryable<T> ApplyAdditionalQuery<T>(
       ISugarQueryable<T> queryable, AdvancedQueryRequestDto query)
    {
        return queryable;
    }

    /// <summary>
    /// 查询结果后处理
    /// </summary>
    protected virtual Task<List<TEntity>> AfterQueryAsync(AdvancedQueryRequestDto query, List<TEntity> items)
    {
        return Task.FromResult(items);
    }

    #endregion

    #region 增删改钩子

    /// <summary>
    /// 新增前处理（校验唯一性、数据预处理等）
    /// </summary>
    protected virtual Task BeforeCreateAsync(TEntity entity)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// 新增后处理
    /// </summary>
    protected virtual Task AfterCreateAsync(TEntity entity)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// 更新前处理（校验等）
    /// </summary>
    protected virtual Task BeforeUpdateAsync(TEntity entity)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// 更新后处理
    /// </summary>
    protected virtual Task AfterUpdateAsync(TEntity entity)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// 删除前处理（关联检查等）
    /// </summary>
    protected virtual Task BeforeDeleteAsync(long id, TEntity entity)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// 删除后处理
    /// </summary>
    protected virtual Task AfterDeleteAsync(long id, TEntity entity)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// 批量删除前处理
    /// </summary>
    protected virtual Task BeforeBatchDeleteAsync(List<long> ids)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// 批量删除后处理
    /// </summary>
    protected virtual Task AfterBatchDeleteAsync(List<long> ids)
    {
        return Task.CompletedTask;
    }

    #endregion

    #region 导入导出钩子

    /// <summary>
    /// 导出前处理
    /// </summary>
    protected virtual Task BeforeExportAsync(AdvancedQueryRequestDto query)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// 对导出数据做转换
    /// </summary>
    protected virtual List<TEntity> TransformExportData(List<TEntity> exportData)
    {
        return exportData;
    }

    /// <summary>
    /// 将实体列表按列配置转换为 Dictionary 行列表（中文表头 + ExtData展开 + 字典值翻译）
    /// MiniExcel 导出 Dictionary 时以 Key 作为表头
    /// </summary>
    protected virtual List<Dictionary<string, object?>> TransformToExportRows(
        List<TEntity> items, List<ExportColumnDto> columns)
    {
        var entityType = typeof(TEntity);
        var props = entityType.GetProperties()
            .Where(p => p.CanRead)
            .ToDictionary(p => p.Name, p => p, StringComparer.OrdinalIgnoreCase);

        // 预解析每列的 propertyInfo（含 ExtData 子属性）
        var colMeta = columns.Select(col =>
        {
            PropertyInfo? propInfo = null;
            var extKey = (string?)null;

            // 先从实体属性中查找
            if (!props.TryGetValue(col.Prop, out propInfo))
            {
                // 若实体无此属性，可能是 ExtData 展开字段
                var extProp = props.GetValueOrDefault("ExtData");
                if (extProp != null) extKey = col.Prop;
            }

            return (col, propInfo, extKey);
        }).ToList();

        var rows = new List<Dictionary<string, object?>>();

        foreach (var entity in items)
        {
            var row = new Dictionary<string, object?>();

            // 解析 ExtData JSON（整个循环只解析一次）
            JsonObject? extObj = null;
            var extValue = props.GetValueOrDefault("ExtData")?.GetValue(entity) as string;
            if (!string.IsNullOrEmpty(extValue))
            {
                try { extObj = JsonNode.Parse(extValue) as JsonObject; } catch { }
            }

            foreach (var (col, propInfo, extKey) in colMeta)
            {
                object? val = null;

                if (propInfo != null)
                {
                    val = propInfo.GetValue(entity);
                }
                else if (extKey != null && extObj != null && extObj.TryGetPropertyValue(extKey, out var node))
                {
                    val = node?.GetValueKind() switch
                    {
                        JsonValueKind.String => node.GetValue<string>(),
                        JsonValueKind.Number => node.AsValue().TryGetValue(out int i) ? (object)i : node.AsValue().TryGetValue(out decimal d) ? d : node.ToString(),
                        JsonValueKind.True => "是",
                        JsonValueKind.False => "否",
                        JsonValueKind.Null => null,
                        _ => node?.ToString(),
                    };
                }

                // 字典值翻译
                if (val != null && col.DictMap != null && col.DictMap.TryGetValue(val.ToString()!, out var label))
                {
                    val = label;
                }

                row[col.Label] = val;
            }

            rows.Add(row);
        }

        return rows;
    }

    /// <summary>
    /// 导入前处理
    /// </summary>
    protected virtual Task<List<TEntity>> BeforeImportAsync(List<TEntity> rows)
    {
        return Task.FromResult(rows);
    }

    /// <summary>
    /// 导入后处理
    /// </summary>
    protected virtual Task AfterImportAsync(int successCount, List<string> errors)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// 获取导出文件的工作表名称
    /// </summary>
    protected virtual string GetExportFileName()
    {
        return typeof(TEntity).Name;
    }

    #endregion

    #region 内部辅助方法

    /// <summary>
    /// 获取实体上所有需要级联处理的导航属性（OneToMany + OneToOne，排除 ManyToOne）
    /// <para>ManyToOne 是对外部实体的引用（如仓库、物料），不参与级联增删改</para>
    /// <para>OneToMany：主实体拥有子集合，需要级联保存/删除</para>
    /// <para>OneToOne：主实体拥有单一子实体，需要级联保存/删除</para>
    /// </summary>
    private static List<PropertyInfo> GetCascadeNavigateProperties()
    {
        return typeof(TEntity).GetProperties()
            .Where(p =>
            {
                var attr = p.GetCustomAttribute<Navigate>();
                return attr != null && attr.GetNavigateType() != NavigateType.ManyToOne;
            })
            .ToList();
    }

    /// <summary>
    /// 静态判断：实体类型是否定义了需要级联处理的导航属性（OneToMany + OneToOne）
    /// </summary>
    private static bool HasCascadeNavigationProperties()
    {
        return GetCascadeNavigateProperties().Count > 0;
    }

    /// <summary>
    /// 运行时判断：级联导航属性是否有实际数据
    /// <para>OneToMany → 集合不为空且 Count > 0</para>
    /// <para>OneToOne → 值不为 null</para>
    /// </summary>
    private static bool HasNavigationWithData(TEntity entity)
    {
        foreach (var prop in GetCascadeNavigateProperties())
        {
            var attr = prop.GetCustomAttribute<Navigate>()!;
            var navType = attr.GetNavigateType();

            if (navType == NavigateType.OneToMany)
            {
                var collection = prop.GetValue(entity) as System.Collections.ICollection;
                if (collection != null && collection.Count > 0)
                    return true;
            }
            else if (navType == NavigateType.OneToOne)
            {
                var value = prop.GetValue(entity);
                if (value != null)
                    return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 填充审计字段
    /// </summary>
    protected virtual void FillAuditFields(TEntity entity, bool isCreate)
    {
        if (isCreate)
        {
            entity.CreatedTime = DateTime.Now;
        }
        else
        {
            entity.LastModifiedTime = DateTime.Now;
        }
    }

    /// <summary>
    /// 根据 ID 获取实体，不存在时抛出异常
    /// </summary>
    protected async Task<TEntity> GetEntityOrThrowAsync(long id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
            throw new NotFoundException(typeof(TEntity).Name, id);

        return entity;
    }

    /// <summary>
    /// 将源实体的属性复制到目标实体（跳过主键和所有级联导航属性）
    /// </summary>
    protected virtual void CopyProperties(TEntity source, TEntity target)
    {
        var type = typeof(TEntity);
        var cascadeNavPropNames = GetCascadeNavigateProperties().Select(p => p.Name).ToHashSet();
        foreach (var prop in type.GetProperties())
        {
            if (!prop.CanRead || !prop.CanWrite) continue;
            if (prop.Name == nameof(BaseEntity<long>.Id)) continue;
            if (cascadeNavPropNames.Contains(prop.Name)) continue;

            var value = prop.GetValue(source);
            if (value != null)
            {
                prop.SetValue(target, value);
            }
        }
    }

    /// <summary>
    /// 将源实体的级联导航属性（OneToMany + OneToOne）复制到目标实体
    /// </summary>
    protected virtual void CopyDetailProperties(TEntity source, TEntity target)
    {
        var cascadeNavProps = GetCascadeNavigateProperties();
        foreach (var prop in cascadeNavProps)
        {
            var value = prop.GetValue(source);
            if (value != null)
                prop.SetValue(target, value);
        }
    }

    /// <summary>
    /// 应用多字段排序（自动过滤无效字段）
    /// </summary>
    protected virtual ISugarQueryable<TEntity> ApplySorting(
        ISugarQueryable<TEntity> queryable,
        List<SortCondition>? sortConditions)
    {
        if (sortConditions != null && sortConditions.Count > 0)
        {
            var validProperties = ExpressionHelper.GetValidPropertyNames(typeof(TEntity));
            var hasSort = false;

            foreach (var sort in sortConditions)
            {
                if (string.IsNullOrWhiteSpace(sort.Field))
                    continue;
                if (!validProperties.Contains(sort.Field))
                    continue;

                var desc = sort.Direction.Equals("desc", StringComparison.OrdinalIgnoreCase);
                queryable = queryable.OrderByPropertyName(sort.Field, desc ? OrderByType.Desc : OrderByType.Asc);
                hasSort = true;
            }

            if (hasSort)
                return queryable;
        }

        // 无有效排序时使用子类定义的默认排序
        return ApplyDefaultSorting(queryable);
    }

    protected virtual ISugarQueryable<T> ApplySorting<T>(
        ISugarQueryable<T> queryable,
        List<SortCondition>? sortConditions)
    {
        if (sortConditions != null && sortConditions.Count > 0)
        {
            var validProperties = ExpressionHelper.GetValidPropertyNames(typeof(TEntity));
            var hasSort = false;

            foreach (var sort in sortConditions)
            {
                if (string.IsNullOrWhiteSpace(sort.Field))
                    continue;
                if (!validProperties.Contains(sort.Field))
                    continue;

                var desc = sort.Direction.Equals("desc", StringComparison.OrdinalIgnoreCase);
                queryable = queryable.OrderByPropertyName(sort.Field, desc ? OrderByType.Desc : OrderByType.Asc);
                hasSort = true;
            }

            if (hasSort)
                return queryable;
        }

        // 无有效排序时使用子类定义的默认排序
        return ApplyDefaultSorting(queryable);
    }

    /// <summary>
    /// 默认排序（子类可重写，如按 CreatedTime 降序）
    /// </summary>
    protected virtual ISugarQueryable<TEntity> ApplyDefaultSorting(ISugarQueryable<TEntity> queryable)
    {
        return queryable.OrderByPropertyNameIF(typeof(TEntity).GetProperties().FirstOrDefault(x => x.Name == nameof(BaseEntity<long>.CreatedTime)) != null, nameof(BaseEntity<long>.CreatedTime), OrderByType.Asc);
    }

    protected virtual ISugarQueryable<T> ApplyDefaultSorting<T>(ISugarQueryable<T> queryable)
    {
        return queryable.OrderByPropertyNameIF(typeof(T).GetProperties().FirstOrDefault(x => x.Name == nameof(BaseEntity<long>.CreatedTime)) != null, nameof(BaseEntity<long>.CreatedTime), OrderByType.Asc);
    }

    #endregion
}
