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
            var existingIds = isCreate
                ? new HashSet<long>()
                : (await QueryIdsByDynamic(detailType, fkName, entity.Id)).ToHashSet();

            if (details != null && details.Count > 0)
            {
                foreach (var item in details)
                {
                    var idProp = detailType.GetProperty(nameof(BaseEntity<long>.Id));
                    var detailId = (long)(idProp?.GetValue(item) ?? 0);

                    if (isCreate && detailId != 0)
                        throw new InvalidOperationException($"新增 {typeof(TEntity).Name} 时，{detailType.Name} 的 Id 必须为 0。");

                    if (!isCreate && detailId != 0 && !existingIds.Contains(detailId))
                        throw new InvalidOperationException(
                            $"{detailType.Name}({detailId}) 不属于当前 {typeof(TEntity).Name}({entity.Id})，禁止跨主表修改明细。");

                    fkProp.SetValue(item, entity.Id);

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
        var task = (Task)method.Invoke(repo, [item])!;
        await task;

        var result = task.GetType().GetProperty("Result")?.GetValue(task);
        if (result != null)
        {
            detailType.GetProperty(nameof(BaseEntity<long>.Id))?.SetValue(item, Convert.ToInt64(result));
        }
    }

    private async Task UpdateByDynamic(Type detailType, object item)
    {
        var repo = GetRepository(detailType);
        var method = repo.GetType().GetMethod("UpdateAsync", [detailType])!;
        await (Task)method.Invoke(repo, [item])!;
    }

    private async Task<List<long>> QueryIdsByDynamic(Type detailType, string fkName, long fkValue)
    {
        var method = GetType()
            .GetMethod(nameof(QueryIdsAsync), BindingFlags.Instance | BindingFlags.NonPublic)!
            .MakeGenericMethod(detailType);
        var task = (Task<List<long>>)method.Invoke(this, [fkName, fkValue])!;
        return await task;
    }

    private async Task<List<long>> QueryIdsAsync<TDetail>(string fkName, long fkValue)
        where TDetail : BaseEntity<long>, new()
    {
        var fkProp = typeof(TDetail).GetProperty(fkName);
        if (fkProp == null) return [];

        var parameter = Expression.Parameter(typeof(TDetail), "x");
        var body = Expression.Equal(Expression.Property(parameter, fkProp), Expression.Constant(fkValue));
        var predicate = Expression.Lambda<Func<TDetail, bool>>(body, parameter);
        var repository = _sp.GetRequiredService<IRepository<TDetail, long>>();
        return await repository.AsQueryable().Where(predicate).Select(x => x.Id).ToListAsync();
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
