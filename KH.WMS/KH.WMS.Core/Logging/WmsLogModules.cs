using System.Collections.Generic;

namespace KH.WMS.Core.Logging
{
    /// <summary>
    /// WMS 业务模块日志编码（ModuleCode）常量。
    /// 用于 Serilog 按模块分文件动态路由日志，编码与系统模块定义保持一致。
    /// </summary>
    public static class WmsLogModules
    {
        /// <summary>WMS 模块编码下限（含）</summary>
        public const int CodeMin = 2001;

        /// <summary>WMS 模块编码上限（含）</summary>
        public const int CodeMax = 2021;

        public const string Inbound = "2001";
        public const string Outbound = "2002";
        public const string Inventory = "2003";
        public const string Product = "2005";
        public const string Warehouse = "2006";
        public const string Transfer = "2010";
        public const string Strategy = "2017";
        public const string Order = "2018";

        /// <summary>模块编码 → 日志文件名段（用于按模块分文件路由）</summary>
        public static readonly Dictionary<string, string> CodeToName = new()
        {
            { Inbound, "Inbound" },
            { Outbound, "Outbound" },
            { Inventory, "Inventory" },
            { Product, "Product" },
            { Warehouse, "Warehouse" },
            { Transfer, "Transfer" },
            { Strategy, "Strategy" },
            { Order, "Order" },
        };

        /// <summary>判断是否为 WMS 模块编码</summary>
        public static bool IsWmsModuleCode(int code) => code >= CodeMin && code <= CodeMax;
    }
}
