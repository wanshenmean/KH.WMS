using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KH.WMS.Core.Constants
{
    public class AppSettingsConstants
    {
        /// <summary>
        /// 数据库连接字符串配置节点
        /// </summary>
        public const string DbConnection = "DbConnection";

        public const string DbType_MySql = "mysql";

        public const string DbType_SqlServer = "sqlserver";

        public const string DbType_PostgreSql = "postgresql";

        public const string DbType_Oracle = "oracle";

        public const string DbType_Sqlite = "sqlite";

        //
        public const string MiniProfiler = "MiniProfiler";

        public const string Swagger = "Swagger";
    }
}
