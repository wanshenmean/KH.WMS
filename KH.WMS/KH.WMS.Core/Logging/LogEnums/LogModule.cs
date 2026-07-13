namespace KH.WMS.Core.Logging.LogEnums
{
    /// <summary>
    /// 日志模块枚举（仅保留 NamespaceModuleMapping 中实际使用的模块）
    /// </summary>
    public enum LogModule
    {
        // === 基础设施层 ===

        /// <summary>
        /// 系统核心
        /// </summary>
        Core = 1000,

        /// <summary>
        /// API 接口
        /// </summary>
        Api = 1001,

        /// <summary>
        /// 数据库
        /// </summary>
        Database = 1002,

        /// <summary>
        /// 认证授权
        /// </summary>
        Auth = 1004,

        /// <summary>
        /// 文件处理
        /// </summary>
        File = 1008,

        /// <summary>
        /// 仓储管理
        /// </summary>
        Repositories = 1009,

        /// <summary>
        /// 日志记录
        /// </summary>
        Logging = 1010,

        // === WMS 业务模块（按 NamespaceModuleMapping 实际映射） ===

        /// <summary>
        /// 入库管理（InboundModule）
        /// </summary>
        WMS_Inbound = 2001,

        /// <summary>
        /// 出库管理（OutboundModule）
        /// </summary>
        WMS_Outbound = 2002,

        /// <summary>
        /// 库存管理（InventoryModule）
        /// </summary>
        WMS_Inventory = 2003,

        /// <summary>
        /// 基础数据（BaseDataModule：物料/客户/供应商/容器等）
        /// </summary>
        WMS_Product = 2005,

        /// <summary>
        /// 仓库管理（WarehouseModule：仓库/库区/库位/巷道/站台等）
        /// </summary>
        WMS_Warehouse = 2006,

        /// <summary>
        /// 任务/移库管理（TaskModule）
        /// </summary>
        WMS_Transfer = 2010,

        /// <summary>
        /// 策略引擎（Algorithms）
        /// </summary>
        WMS_Strategy = 2017,

        /// <summary>
        /// 配置/单据管理（ConfigModule）
        /// </summary>
        WMS_Order = 2018,
    }
}
