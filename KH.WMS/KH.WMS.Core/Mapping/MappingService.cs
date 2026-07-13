using AutoMapper;
using KH.WMS.Core.DependencyInjection.ServiceLifetimes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace KH.WMS.Core.Mapping;

/// <summary>
/// 对象映射服务实现（基于AutoMapper）
/// </summary>
[RegisteredService(ServiceType = typeof(IMappingService), WithoutInterceptor = true)]
public class MappingService : IMappingService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MappingService> _logger;

    public MappingService(IServiceProvider serviceProvider, ILogger<MappingService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public TDestination Map<TDestination>(object source)
    {
        var mapper = GetMapper();
        return mapper.Map<TDestination>(source);
    }

    public TDestination Map<TSource, TDestination>(TSource source)
    {
        var mapper = GetMapper();
        return mapper.Map<TSource, TDestination>(source);
    }

    public TDestination Map<TSource, TDestination>(TSource source, TDestination destination)
    {
        var mapper = GetMapper();
        return mapper.Map(source, destination);
    }

    public List<TDestination> MapList<TSource, TDestination>(IEnumerable<TSource> source)
    {
        var mapper = GetMapper();
        return mapper.Map<IEnumerable<TSource>, List<TDestination>>(source);
    }

    public List<TDestination> MapList<TDestination>(IEnumerable<object> source)
    {
        var mapper = GetMapper();
        return mapper.Map<IEnumerable<object>, List<TDestination>>(source);
    }

    private IMapper GetMapper()
    {
        try
        {
            return _serviceProvider.GetRequiredService<IMapper>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取 AutoMapper 失败");
            throw;
        }
    }
}
