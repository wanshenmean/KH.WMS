using KH.WMS.Core.Models.Entities;

namespace KH.WMS.Core.Services;

/// <summary>
/// 主从表保存服务接口
/// 自动扫描实体上的导航属性，完成从表的增删改
/// <para>OneToMany：通过 SaveDetailsAsync 处理（增量比对：新增/修改/删除）</para>
/// <para>OneToOne：通过 SaveOneToOneAsync 处理（新增/更新单一子实体）</para>
/// </summary>
public interface IDetailSaveService
{
    /// <summary>
    /// 保存主实体的 OneToMany 从表数据（增量比对：新增/修改/删除）
    /// </summary>
    /// <typeparam name="TEntity">主实体类型</typeparam>
    /// <param name="entity">主实体实例（需已持久化，包含有效 Id）</param>
    /// <param name="isCreate">是否为新增场景（新增时跳过删除检测）</param>
    Task SaveDetailsAsync<TEntity>(TEntity entity, bool isCreate) where TEntity : BaseEntity<long>, new();

    /// <summary>
    /// 保存主实体的 OneToOne 导航属性（新增/更新单一子实体）
    /// </summary>
    /// <typeparam name="TEntity">主实体类型</typeparam>
    /// <param name="entity">主实体实例（需已持久化，包含有效 Id）</param>
    /// <param name="isCreate">是否为新增场景</param>
    Task SaveOneToOneAsync<TEntity>(TEntity entity, bool isCreate) where TEntity : BaseEntity<long>, new();
}
