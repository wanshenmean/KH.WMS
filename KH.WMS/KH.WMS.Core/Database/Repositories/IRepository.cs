using System.Linq.Expressions;
using SqlSugar;

namespace KH.WMS.Core.Database.Repositories;

/// <summary>
/// 仓储接口
/// </summary>
/// <typeparam name="T">实体类型</typeparam>
public interface IRepository<T, TKey> 
    where T : class
    where TKey : struct
{
    /// <summary>
    /// 根据ID获取实体
    /// </summary>
    Task<T?> GetByIdAsync(TKey id);

    /// <summary>
    /// 获取所有实体
    /// </summary>
    Task<List<T>> GetAllAsync();

    /// <summary>
    /// 根据条件查询实体
    /// </summary>
    Task<List<T>> GetListAsync(Expression<Func<T, bool>> expression);

    /// <summary>
    /// 根据条件查询单个实体
    /// </summary>
    Task<T?> GetFirstOrDefaultAsync(Expression<Func<T, bool>> expression);

    /// <summary>
    /// 分页查询
    /// </summary>
    Task<(List<T> Items, int Total)> GetPagedListAsync(int pageIndex, int pageSize, Expression<Func<T, bool>>? expression = null);

    /// <summary>
    /// 插入实体
    /// </summary>
    Task<TKey> AddAsync(T entity);

    /// <summary>
    /// 批量插入实体
    /// </summary>
    Task<List<TKey>> AddAsync(List<T> entities);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    Task<T> AddReturnEntityAsync(T entity);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="entities"></param>
    /// <returns></returns>
    Task<List<T>> AddReturnEntityAsync(List<T> entities);

    /// <summary>
    /// 更新实体
    /// </summary>
    Task<bool> UpdateAsync(T entity);

    /// <summary>
    /// 批量更新实体
    /// </summary>
    Task<bool> UpdateAsync(List<T> entities);

    /// <summary>
    /// 删除实体
    /// </summary>
    Task<bool> DeleteAsync(TKey id);

    /// <summary>
    /// 批量删除实体
    /// </summary>
    Task<bool> DeleteAsync(List<TKey> ids);

    /// <summary>
    /// 根据条件删除实体
    /// </summary>
    Task<bool> DeleteAsync(Expression<Func<T, bool>> expression);

    /// <summary>
    /// 检查实体是否存在
    /// </summary>
    Task<bool> ExistsAsync(Expression<Func<T, bool>> expression);

    /// <summary>
    /// 获取实体数量
    /// </summary>
    Task<int> CountAsync(Expression<Func<T, bool>>? expression = null);

    /// <summary>
    /// 导航插入（主表 + 从表一起插入）
    /// </summary>
    Task<bool> AddWithNavAsync(T entity);

    /// <summary>
    /// 导航更新（主表 + 从表一起更新，自动处理从表的增删改）
    /// </summary>
    Task<bool> UpdateWithNavAsync(T entity);

    /// <summary>
    /// 导航删除（主表 + 从表一起删除）
    /// </summary>
    Task<bool> DeleteWithNavAsync(T entity);

    /// <summary>
    /// 根据ID获取实体（包含导航属性，主表 + 从表一起查询）
    /// </summary>
    Task<T?> GetByIdWithNavAsync(TKey id);

    /// <summary>
    /// 获取所有实体（包含导航属性，主表 + 从表一起查询）
    /// </summary>
    Task<List<T>> GetAllWithNavAsync();

    /// <summary>
    /// 根据条件查询实体（包含导航属性，主表 + 从表一起查询）
    /// </summary>
    Task<List<T>> GetListWithNavAsync(Expression<Func<T, bool>> expression);

    /// <summary>
    /// 根据条件查询单个实体（包含导航属性，主表 + 从表一起查询）
    /// </summary>
    Task<T?> GetFirstOrDefaultWithNavAsync(Expression<Func<T, bool>> expression);

    /// <summary>
    /// 根据从表条件查询主表（主表 + 从表一起查询）
    /// </summary>
    /// <typeparam name="TDetail">从表实体类型</typeparam>
    /// <param name="navProperty">导航属性，如 x => x.OrderLines</param>
    /// <param name="detailFilter">从表过滤条件，如 d => d.MaterialId == 123</param>
    /// <returns>满足条件的主表列表（包含从表数据）</returns>
    Task<List<T>> GetListByDetailAsync<TDetail>(
        Expression<Func<T, IEnumerable<TDetail>>> navProperty,
        Expression<Func<TDetail, bool>> detailFilter)
        where TDetail : class, new();

    /// <summary>
    /// 根据从表条件查询单个主表（主表 + 从表一起查询）
    /// </summary>
    /// <typeparam name="TDetail">从表实体类型</typeparam>
    /// <param name="navProperty">导航属性，如 x => x.OrderLines</param>
    /// <param name="detailFilter">从表过滤条件，如 d => d.MaterialId == 123</param>
    /// <returns>满足条件的单个主表（包含从表数据）</returns>
    Task<T?> GetFirstOrDefaultByDetailAsync<TDetail>(
        Expression<Func<T, IEnumerable<TDetail>>> navProperty,
        Expression<Func<TDetail, bool>> detailFilter)
        where TDetail : class, new();

    /// <summary>
    /// 获取基础可查询对象，用于需要跨表 join、聚合等仓储预定义方法无法覆盖的复杂查询场景
    /// </summary>
    /// <returns>SqlSugar 的 ISugarQueryable 查询对象</returns>
    ISugarQueryable<T> AsQueryable();
}
