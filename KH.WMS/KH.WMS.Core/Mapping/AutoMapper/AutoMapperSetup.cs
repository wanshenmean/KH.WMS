using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace KH.WMS.Core.Mapping.AutoMapper;

/// <summary>
/// AutoMapper 配置
/// </summary>
public static class AutoMapperSetup
{
    /// <summary>
    /// 添加 AutoMapper 服务
    /// </summary>
    public static IServiceCollection AddAutoMapper(this IServiceCollection services, params System.Reflection.Assembly[] assemblies)
    {
        if (assemblies == null || assemblies.Length == 0)
        {
            assemblies = new[] { System.Reflection.Assembly.GetCallingAssembly() };
        }

        var expression = new MapperConfigurationExpression();
        expression.AllowNullCollections = true;
        expression.AllowNullDestinationValues = true;
        expression.ShouldMapField = fi => false;
        expression.ShouldMapProperty = pi => pi.GetMethod != null && !pi.GetMethod.IsPrivate;

        // 扫描并添加所有 Profile
        foreach (var assembly in assemblies)
        {
            expression.AddMaps(assembly);
        }

        services.AddSingleton(sp =>
        {
            var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
            var config = new MapperConfiguration(expression, loggerFactory);
            config.AssertConfigurationIsValid();
            return config.CreateMapper();
        });
        //services.AddScoped<IMappingService, MappingService>();

        return services;
    }

    /// <summary>
    /// 添加 AutoMapper 服务（指定 Profile 类型）
    /// </summary>
    public static IServiceCollection AddAutoMapperProfiles(this IServiceCollection services, params Type[] profileTypes)
    {
        var expression = new MapperConfigurationExpression();
        expression.AllowNullCollections = true;
        expression.AllowNullDestinationValues = true;
        expression.ShouldMapField = fi => false;
        expression.ShouldMapProperty = pi => pi.GetMethod != null && !pi.GetMethod.IsPrivate;

        foreach (var profileType in profileTypes)
        {
            expression.AddProfile(profileType);
        }

        services.AddSingleton(sp =>
        {
            var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
            var config = new MapperConfiguration(expression, loggerFactory);
            config.AssertConfigurationIsValid();
            return config.CreateMapper();
        });
        //services.AddScoped<IMappingService, MappingService>();

        return services;
    }
}
