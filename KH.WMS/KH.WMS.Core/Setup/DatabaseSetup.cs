using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KH.WMS.Core.Setup;

/// <summary>
/// 数据库配置
/// </summary>
public static class DatabaseSetup
{
    /// <summary>
    /// 配置 SQL Sugar
    /// </summary>
    public static IServiceCollection AddSqlSugarSetup(this IServiceCollection services, IConfiguration configuration)
    {
        return KH.WMS.Core.Database.SqlSugar.SqlSugarSetup.AddSqlSugar(services, configuration);
    }
}
