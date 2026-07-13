using System.Reflection;
using KH.WMS.Core.Constants;
using KH.WMS.Core.DependencyInjection;
using KH.WMS.Core.Database.UnitOfWorks;
using KH.WMS.Core.Models.Entities;
using KH.WMS.Core.UserProvide;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SqlSugar;
using StackExchange.Profiling;

namespace KH.WMS.Core.Database.SqlSugar;

/// <summary>
/// SQL Sugar 配置
/// </summary>
public static class SqlSugarSetup
{
    /// <summary>业务库 configId</summary>
    public const string MainDb = "MainDb";
    /// <summary>配置库 configId</summary>
    public const string ConfigDb = "ConfigDb";

    /// <summary>
    /// 添加 SQL Sugar 服务（主库 + 配置库双实例）
    /// </summary>
    public static IServiceCollection AddSqlSugar(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<DatabaseOptions>(configuration.GetSection(AppSettingsConstants.DbConnection));

        Console.WriteLine("SqlSugar 正在初始化数据库连接...");

        _ = services.AddScoped<ISqlSugarClient>(provider =>
        {
            var options = provider.GetRequiredService<IOptions<DatabaseOptions>>().Value;
            var logger = provider.GetService<ILogger<SqlSugarClient>>();
            var userContext = provider.GetService<IUserContext>();

            var configDbPath = Path.Combine(AppContext.BaseDirectory, options.ConfigDbPath ?? "Data/kh-wms-config.db");
            var configDir = Path.GetDirectoryName(configDbPath)!;
            if (!Directory.Exists(configDir)) Directory.CreateDirectory(configDir);

            // ---- 主库连接（SQL Server / MySQL 等业务库） ----
            var mainConfig = BuildConnectionConfig(
                MainDb, options.ConnectionString, GetDbType(options.DbType),
                options, logger, userContext);

            // ---- 配置库连接（SQLite） ----
            var configConnectionString = $"DataSource={configDbPath}";
            var configConfig = BuildConnectionConfig(
                ConfigDb, configConnectionString, DbType.Sqlite,
                options, logger, userContext);

            var sugarClient = CreateClient(
                new List<ConnectionConfig> { mainConfig, configConfig }, userContext);

            // 配置库/主库的建库与建表统一由 DatabaseInitService.InitDatabase 完成

            return sugarClient;
        });

        return services;
    }

    /// <summary>
    /// 构建单个 ConnectionConfig
    /// </summary>
    private static ConnectionConfig BuildConnectionConfig(
        string configId, string connectionString, DbType dbType,
        DatabaseOptions options, ILogger<SqlSugarClient>? logger, IUserContext? userContext)
    {
        var config = new ConnectionConfig
        {
            ConfigId = configId,
            ConnectionString = connectionString,
            DbType = dbType,
            IsAutoCloseConnection = true,
            InitKeyType = InitKeyType.Attribute,
            AopEvents = new AopEvents
            {
                OnLogExecuting = (sql, pars) =>
                {
                    foreach (var p in pars)
                    {
                        if (p.Value is Enum enumValue)
                            p.Value = enumValue.ToString();
                    }

                    var paramStr = string.Join(", ", pars.Select(p => $"{p.ParameterName}={p.Value}"));
                    Parallel.For(0, 1, e =>
                    {
                        MiniProfiler.Current.CustomTiming("SQL：", paramStr + "【SQL语句】：" + sql);
                    });

                    if (options.EnableSqlLog && logger != null)
                    {
                        logger.LogDebug("[SQL执行] {Sql} | 参数: {Params}", sql, paramStr);
                    }

                    // 追加到请求级异常缓冲区（不管 EnableSqlLog/MinimumLevel，异常时强制写入 error 日志）
                    KH.WMS.Core.Logging.ErrorLogScope.Append($"[{DateTime.Now:HH:mm:ss.fff}] SQL: {sql} | 参数: {paramStr}");

                    if (options.EnableSqlLog)
                    {
                        Console.WriteLine($"SQL[{configId}]: {sql}\nPARAMS: {paramStr}");
                    }
                },
            },
        };

        config.ConfigureExternalServices = new ConfigureExternalServices
        {
            EntityService = (property, column) =>
            {
                var propType = property.PropertyType;
                var underlyingType = Nullable.GetUnderlyingType(propType);
                var targetType = underlyingType ?? propType;

                if (targetType.IsEnum)
                {
                    // SQLite 无 nvarchar，统一用 TEXT
                    column.DataType = dbType == DbType.Sqlite ? "TEXT" : "nvarchar(20)";
                }

                // SQLite AUTOINCREMENT 只允许 INTEGER PRIMARY KEY
                if (dbType == DbType.Sqlite
                    && column.IsPrimarykey
                    && column.IsIdentity
                    && (targetType == typeof(long) || targetType == typeof(int)))
                {
                    column.DataType = "INTEGER";
                }
            }
        };

        // 注意：审计 Aop 在 SqlSugarClient 级别统一设置，见下方
        return config;
    }

    /// <summary>
    /// 创建 SqlSugarClient 并注册统一的审计 Aop
    /// </summary>
    private static SqlSugarClient CreateClient(
        List<ConnectionConfig> configs, IUserContext? userContext)
    {
        var client = new SqlSugarClient(configs);
        client.Aop.DataExecuting = (obj, model) =>
        {
            if (model.EntityValue is not RootEntity entity) return;

            switch (model.OperationType)
            {
                case DataFilterType.InsertByObject:
                    switch (model.PropertyName)
                    {
                        case nameof(RootEntity.CreatedTime):
                            entity.CreatedTime = DateTime.Now;
                            break;
                        case nameof(RootEntity.CreatedBy):
                            entity.CreatedBy = userContext?.UserId.ToString();
                            break;
                        case nameof(RootEntity.CreatedByName):
                            entity.CreatedByName = userContext?.UserName;
                            break;
                    }
                    break;
                case DataFilterType.UpdateByObject:
                    switch (model.PropertyName)
                    {
                        case nameof(RootEntity.LastModifiedTime):
                            entity.LastModifiedTime = DateTime.Now;
                            break;
                        case nameof(RootEntity.LastModifiedBy):
                            entity.LastModifiedBy = userContext?.UserId.ToString();
                            break;
                        case nameof(RootEntity.LastModifiedByName):
                            entity.LastModifiedByName = userContext?.UserName;
                            break;
                    }
                    break;
            }
        };
        return client;
    }

    private static DbType GetDbType(string dbType)
    {
        return dbType.ToLower() switch
        {
            AppSettingsConstants.DbType_MySql => DbType.MySql,
            AppSettingsConstants.DbType_SqlServer => DbType.SqlServer,
            AppSettingsConstants.DbType_PostgreSql => DbType.PostgreSQL,
            AppSettingsConstants.DbType_Oracle => DbType.Oracle,
            AppSettingsConstants.DbType_Sqlite => DbType.Sqlite,
            _ => DbType.SqlServer
        };
    }
}

/// <summary>
/// 数据库配置选项
/// </summary>
public class DatabaseOptions
{
    /// <summary>
    /// 连接字符串
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// 数据库类型
    /// </summary>
    public string DbType { get; set; } = AppSettingsConstants.DbType_SqlServer;

    /// <summary>
    /// 是否启用SQL日志
    /// </summary>
    public bool EnableSqlLog { get; set; } = true;

    /// <summary>
    /// 命令超时时间（秒）
    /// </summary>
    public int CommandTimeout { get; set; } = 30;

    /// <summary>
    /// 配置库 SQLite 文件路径（相对或绝对），默认 Data/kh-wms-config.db
    /// </summary>
    public string ConfigDbPath { get; set; } = "Data/kh-wms-config.db";
}
