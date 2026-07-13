using System.Linq.Expressions;
using System.Reflection;
using KH.WMS.Core.Database.Repositories;
using KH.WMS.Core.DependencyInjection.ServiceLifetimes;
using KH.WMS.Core.Models.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SqlSugar;

namespace KH.WMS.Core.Services;

/// <summary>
/// 主从表保存服务
/// 自动扫描实体上的 [Navigate(NavigateType.OneToMany)] 导航属性，完成从表的增删改
/// 通过 IServiceProvider 动态解析 IRepository，自动处理 ConfigDb 路由
/// </summary>
[RegisteredService(WithoutInterceptor = true)]
public class DetailSaveService : IDetailSaveService
{
    private readonly IServiceProvider _sp;
    private readonly ILogger<DetailSaveService> _logger;

    public DetailSaveService(IServiceProvider sp, ILogger<DetailSaveService> logger)
    {
        _sp = sp;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task SaveDetailsAsync<TEntity>(TEntity entity, bool isCreate) where TEntity : BaseEntity<long>, new()
    {
        CheckUnconfiguredCollectionProperties<TEntity>();

        var detailProps = GetDetailProperties<TEntity>();
        foreach (var prop in detailProps)
        {
            var navigateAttr = prop.GetCustomAttribute<Navigate>()!;
            var detailType = prop.PropertyType.GetGenericArguments()[0];
            var fkName = navigateAttr.GetName();
            var fkProp = detailType.GetProperty(fkName);
            if (fkProp == null) continue;

            var details = prop.GetValue(entity) as System.Collections.IList;
            var submittedIds = new List<long>();

            if (details != null && details.Count > 0)
            {
                foreach (var item in details)
                {
                    fkProp.SetValue(item, entity.Id);

                    var idProp = detailType.GetProperty(nameof(BaseEntity<long>.Id));
                    var detailId = (long)(idProp?.GetValue(item) ?? 0);

                    if (detailId == 0)
                    {
                        var createdProp = detailType.GetProperty(nameof(BaseEntity<long>.CreatedTime));
                        createdProp?.SetValue(item, DateTime.Now);
                        await InsertByDynamic(detailType, item);
                    }
                    else
                    {
                        var modifiedProp = detailType.GetProperty(nameof(BaseEntity<long>.LastModifiedTime));
                        modifiedProp?.SetValue(item, DateTime.Now);
                        await UpdateByDynamic(detailType, item);
                    }

                    submittedIds.Add(detailId == 0
                        ? (long)(detailType.GetProperty(nameof(BaseEntity<long>.Id))?.GetValue(item) ?? 0)
                        : detailId);
                }
            }

            if (!isCreate)
            {
                var existingIds = await QueryIdsByDynamic(detailType, fkName, entity.Id);
                var deleteIds = existingIds.Except(submittedIds).ToList();
                if (deleteIds.Count > 0)
                {
                    await DeleteByIdsByDynamic(detailType, deleteIds);
                }
            }
        }
    }

    /// <inheritdoc />
    public async Task SaveOneToOneAsync<TEntity>(TEntity entity, bool isCreate) where TEntity : BaseEntity<long>, new()
    {
        var oneToOneProps = GetOneToOneProperties<TEntity>();
        foreach (var prop in oneToOneProps)
        {
            var value = prop.GetValue(entity);
            if (value == null) continue;

            var navigateAttr = prop.GetCustomAttribute<Navigate>()!;
            var detailType = prop.PropertyType;
            var fkName = navigateAttr.GetName();
            var fkProp = detailType.GetProperty(fkName);
            if (fkProp != null) fkProp.SetValue(value, entity.Id);

            var idProp = detailType.GetProperty(nameof(BaseEntity<long>.Id));
            var childId = idProp != null ? (long)idProp.GetValue(value)! : 0;

            if (childId == 0)
            {
                // 新子实体 → 插入
                var createdProp = detailType.GetProperty(nameof(BaseEntity<long>.CreatedTime));
                createdProp?.SetValue(value, DateTime.Now);
                await InsertByDynamic(detailType, value);
            }
            else
            {
                // 已有子实体 → 更新
                var modifiedProp = detailType.GetProperty(nameof(BaseEntity<long>.LastModifiedTime));
                modifiedProp?.SetValue(value, DateTime.Now);
                await UpdateByDynamic(detailType, value);
            }
        }
    }

    #region 反射辅助方法

    private object GetRepository(Type detailType)
    {
        var repoType = typeof(IRepository<,>).MakeGenericType(detailType, typeof(long));
        return _sp.GetRequiredService(repoType);
    }

    private async Task InsertByDynamic(Type detailType, object item)
    {
        var repo = GetRepository(detailType);
        var method = repo.GetType().GetMethod("AddAsync", [detailType])!;
        await (Task)method.Invoke(repo, [item])!;
    }

    private async Task UpdateByDynamic(Type detailType, object item)
    {
        var repo = GetRepository(detailType);
        var method = repo.GetType().GetMethod("UpdateAsync", [detailType])!;
        await (Task)method.Invoke(repo, [item])!;
    }

    private async Task<List<long>> QueryIdsByDynamic(Type detailType, string fkName, long fkValue)
    {
        var repo = GetRepository(detailType);
        var fkProp = detailType.GetProperty(fkName);
        if (fkProp == null) return new List<long>();

        var exprType = typeof(Func<,>).MakeGenericType(detailType, typeof(bool));
        // 通过 AsQueryable 获取 ISugarQueryable，Where + Select
        var asQueryableMethod = repo.GetType().GetMethod("AsQueryable")!;
        var queryable = asQueryableMethod.Invoke(repo, null)!;

        var queryableType = queryable.GetType();
        var whereMethod = queryableType.GetMethod("Where", [exprType])!;

        // 构建参数表达式：x => fkProp.GetValue(x)!.Equals(fkValue)
        var param = Expression.Parameter(detailType, "x");
        var fkAccess = Expression.Property(param, fkName);
        var fkValueExpr = Expression.Constant(fkValue);
        var body = Expression.Call(
            Expression.Convert(fkAccess, typeof(object)),
            typeof(object).GetMethod("Equals", [typeof(object)])!,
            Expression.Convert(fkValueExpr, typeof(object)));
        var lambda = Expression.Lambda(exprType, body, param);

        var filteredQueryable = whereMethod.Invoke(queryable, [lambda])!;
        var selectMethod = filteredQueryable.GetType().GetMethod("Select", [typeof(Func<,>).MakeGenericType(detailType, typeof(long))])!;

        var selectParam = Expression.Parameter(detailType, "x");
        var idProp = Expression.Property(selectParam, "Id");
        var selectLambda = Expression.Lambda(
            typeof(Func<,>).MakeGenericType(detailType, typeof(long)),
            idProp, selectParam);

        var idQueryable = selectMethod.Invoke(filteredQueryable, [selectLambda])!;
        var toListMethod = idQueryable.GetType().GetMethod("ToListAsync")!;
        var task = (Task)toListMethod.Invoke(idQueryable, null)!;
        await task.ConfigureAwait(false);
        return (List<long>)task.GetType().GetProperty("Result")!.GetValue(task)!;
    }

    private async Task DeleteByIdsByDynamic(Type detailType, List<long> ids)
    {
        var repo = GetRepository(detailType);
        var method = repo.GetType().GetMethod("DeleteAsync", [typeof(List<long>)])!;
        await (Task)method.Invoke(repo, [ids])!;
    }

    #endregion

    #region 静态辅助方法

    /// <summary>
    /// 获取实体上所有标记了 [Navigate(NavigateType.OneToMany)] 的 List 属性
    /// </summary>
    internal static List<PropertyInfo> GetDetailProperties<TEntity>() where TEntity : BaseEntity<long>, new()
    {
        return typeof(TEntity).GetProperties()
            .Where(p =>
            {
                var attr = p.GetCustomAttribute<Navigate>();
                return attr != null
                    && attr.GetNavigateType() == NavigateType.OneToMany
                    && p.PropertyType.IsGenericType
                    && p.PropertyType.GetGenericTypeDefinition() == typeof(List<>);
            })
            .ToList();
    }

    /// <summary>
    /// 获取实体上所有标记了 [Navigate(NavigateType.OneToOne)] 的属性
    /// </summary>
    internal static List<PropertyInfo> GetOneToOneProperties<TEntity>() where TEntity : BaseEntity<long>, new()
    {
        return typeof(TEntity).GetProperties()
            .Where(p =>
            {
                var attr = p.GetCustomAttribute<Navigate>();
                return attr != null && attr.GetNavigateType() == NavigateType.OneToOne;
            })
            .ToList();
    }

    /// <summary>
    /// 检查实体上的集合属性是否配置了 [Navigate] 特性，未配置时输出警告提醒
    /// </summary>
    private void CheckUnconfiguredCollectionProperties<TEntity>() where TEntity : BaseEntity<long>, new()
    {
        var navigateProps = GetDetailProperties<TEntity>().Select(p => p.Name).ToHashSet();

        var unconfigured = typeof(TEntity).GetProperties()
            .Where(p => p.PropertyType.IsGenericType
                && p.PropertyType.GetGenericTypeDefinition() == typeof(List<>)
                && !navigateProps.Contains(p.Name))
            .Select(p => p.Name)
            .ToList();

        if (unconfigured.Count > 0)
        {
            _logger.LogWarning(
                "{EntityType} 包含未配置 [Navigate(NavigateType.OneToMany)] 的集合属性：{Properties}。" +
                "SaveDetailsAsync 将跳过这些属性，如需自动保存从表数据请添加 [Navigate] 特性。",
                typeof(TEntity).Name, string.Join(", ", unconfigured));
        }
    }

    #endregion
}
