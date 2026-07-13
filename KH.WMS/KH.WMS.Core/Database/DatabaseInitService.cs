using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using KH.WMS.Core.Database.SqlSugar;
using KH.WMS.Core.DependencyInjection;
using KH.WMS.Core.DependencyInjection.ServiceLifetimes;
using KH.WMS.Core.Models.Entities;
using Microsoft.Extensions.Options;
using SqlSugar;

namespace KH.WMS.Core.Database
{
    [RegisteredService(Lifetime = Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton)]
    public class DatabaseInitService : IDatabaseInitService
    {
        private readonly ISqlSugarClient _db;
        private readonly DatabaseOptions _dbOptions;
        public DatabaseInitService(ISqlSugarClient db, IOptions<DatabaseOptions> dbOptions)
        {
            _db = db;
            _dbOptions = dbOptions.Value;
        }

        public void CreateDatabase(string databaseName = "")
        {
            // 主库（SQL Server / MySQL 等）：已存在时 SqlSugar 内部会跳过，无需额外处理
            if (string.IsNullOrEmpty(databaseName))
                _db.DbMaintenance.CreateDatabase();
            else
                _db.DbMaintenance.CreateDatabase(databaseName);
        }

        /// <summary>
        /// 主库自动建表（CodeFirst）
        /// 收集所有继承自 RootEntity 且未标记 [ConfigDb] 的实体，在主库上初始化表结构
        /// </summary>
        public void CreateTables()
        {
            // 获取实体基类所在程序集
            var baseEntityType = typeof(RootEntity);

            List<Assembly> assemblies = AssemblyService.GetReferencedAssemblies();

            foreach (var assembly in assemblies)
            {
                List<Type> types = assembly.GetTypes()
                    .Where(t => t.IsClass && !t.IsAbstract && !t.IsInterface
                        && baseEntityType.IsAssignableFrom(t)
                        && t != typeof(BaseEntity<>) && t != baseEntityType
                        && t.GetCustomAttribute<ConfigDbAttribute>() == null) // 排除配置实体（由 InitConfigTables 在 SQLite 上创建）
                    .ToList();
                _db.CodeFirst.InitTables(types.ToArray());
            }
        }

        /// <summary>
        /// 配置库自动建表（CodeFirst）
        /// 收集所有标记了 [ConfigDb] 的实体类型，在配置库（SQLite）上初始化表结构
        /// </summary>
        private void InitConfigTables()
        {
            var configDb = ((SqlSugarClient)_db).GetConnection(SqlSugarSetup.ConfigDb);

            // SQLite 配置库：检查文件是否存在，不存在才创建（文件已存在时 CreateDatabase 会报错）
            var configDbPath = Path.Combine(AppContext.BaseDirectory, _dbOptions.ConfigDbPath ?? "Data/kh-wms-config.db");
            if (!File.Exists(configDbPath))
            {
                var configDir = Path.GetDirectoryName(configDbPath)!;
                if (!Directory.Exists(configDir)) Directory.CreateDirectory(configDir);
                configDb.DbMaintenance.CreateDatabase();
                Console.WriteLine("SqlSugar 配置库文件已创建");
            }

            var assemblies = AssemblyService.GetReferencedAssemblies();
            var configTypes = assemblies.SelectMany(a => a.GetTypes())
                .Where(t => t.IsClass && !t.IsAbstract && t.GetCustomAttribute<ConfigDbAttribute>() != null)
                .ToArray();

            if (configTypes.Length > 0)
            {
                configDb.CodeFirst.InitTables(configTypes);
                Console.WriteLine($"SqlSugar 配置库已初始化 {configTypes.Length} 张配置表");
            }
        }

        public void InitDatabase()
        {
            try
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("开始初始化数据库...");
                Console.ResetColor();

                // 1. 主库：建库 + 建表
                CreateDatabase();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("主数据库创建成功");
                CreateTables();
                Console.WriteLine("主数据库表创建成功");

                // 2. 配置库（SQLite）：建库 + 建表
                InitConfigTables();
                Console.WriteLine("配置库表创建成功");

                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"数据库初始化失败：{ex.Message}");
                Console.ResetColor();
                return;
            }

        }
    }
}
