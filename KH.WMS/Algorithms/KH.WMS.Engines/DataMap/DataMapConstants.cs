namespace KH.WMS.Engines.DataMap
{
    /// <summary>
    /// 数据映射 / 接口集成相关常量。
    /// 对应 IntInterfaceConfig、IntSyncLog、IntDataMappingConfig 等实体的状态/方向/类型字段。
    /// </summary>
    public static class DataMapConstants
    {
        /// <summary>
        /// 接口状态（IntInterfaceConfig.Status）
        /// </summary>
        public static class InterfaceStatus
        {
            /// <summary>正常</summary>
            public const string ACTIVE = "ACTIVE";

            /// <summary>停用</summary>
            public const string INACTIVE = "INACTIVE";
        }

        /// <summary>
        /// 接口方向（IntInterfaceConfig.Direction）
        /// </summary>
        public static class InterfaceDirection
        {
            /// <summary>入站：外部系统 → WMS</summary>
            public const string INBOUND = "INBOUND";

            /// <summary>出站：WMS → 外部系统</summary>
            public const string OUTBOUND = "OUTBOUND";

            /// <summary>双向</summary>
            public const string BIDIRECTIONAL = "BIDIRECTIONAL";
        }

        /// <summary>
        /// 接口类型（IntInterfaceConfig.InterfaceType）
        /// </summary>
        public static class InterfaceType
        {
            /// <summary>同步</summary>
            public const string SYNC = "SYNC";

            /// <summary>异步</summary>
            public const string ASYNC = "ASYNC";
        }

        /// <summary>
        /// 接口调用日志状态（IntSyncLog.Status）
        /// </summary>
        public static class CallLogStatus
        {
            /// <summary>成功</summary>
            public const string SUCCESS = "SUCCESS";

            /// <summary>失败</summary>
            public const string FAIL = "FAIL";

            /// <summary>超时</summary>
            public const string TIMEOUT = "TIMEOUT";
        }

        /// <summary>
        /// 转换脚本语言（IntDataMappingConfig.ScriptType）
        /// </summary>
        public static class ScriptLanguage
        {
            /// <summary>C# 脚本</summary>
            public const string CSHARP = "CSHARP";

            /// <summary>Lua 脚本</summary>
            public const string LUA = "LUA";

            /// <summary>JavaScript 脚本</summary>
            public const string JAVASCRIPT = "JAVASCRIPT";
        }
    }
}
