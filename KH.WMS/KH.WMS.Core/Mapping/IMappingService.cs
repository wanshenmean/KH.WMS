namespace KH.WMS.Core.Mapping;

/// <summary>
/// 对象映射服务接口
/// </summary>
public interface IMappingService
{
    /// <summary>
    /// 映射到目标类型
    /// </summary>
    TDestination Map<TDestination>(object source);

    /// <summary>
    /// 映射到目标类型
    /// </summary>
    TDestination Map<TSource, TDestination>(TSource source);

    /// <summary>
    /// 映射到目标类型
    /// </summary>
    TDestination Map<TSource, TDestination>(TSource source, TDestination destination);

    /// <summary>
    /// 映射集合
    /// </summary>
    List<TDestination> MapList<TSource, TDestination>(IEnumerable<TSource> source);

    /// <summary>
    /// 映射集合
    /// </summary>
    List<TDestination> MapList<TDestination>(IEnumerable<object> source);
}
