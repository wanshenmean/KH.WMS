namespace KH.WMS.Algorithms.Strategy.Constants
{
    /// <summary>
    /// 算法引擎内部常量
    /// </summary>
    public static class AlgoConstants
    {
        /// <summary>
        /// 库位状态
        /// </summary>
        public static class LocationStatus
        {
            public const string EMPTY = "EMPTY";
            public const string OCCUPIED = "OCCUPIED";
        }

        /// <summary>
        /// 库位锁定状态（与 BizConstants.LocationLockStatus 对应）
        /// </summary>
        public static class LocationLockStatus
        {
            public const byte NONE = 0;
            public const byte INBOUND_RESERVED = 1;
            public const byte OUTBOUND_LOCKED = 2;
            public const byte STOCKTAKE_FREEZE = 3;
        }

        /// <summary>
        /// 库存状态
        /// </summary>
        public static class InventoryStatus
        {
            public const string AVAILABLE = "AVAILABLE";
            public const string LOCKED = "LOCKED";
            public const string QC = "QC";
            public const string FROZEN = "FROZEN";
        }

        /// <summary>
        /// 库区类型（与 cfg_warehouse_zone_type.TypeCode 保持一致）
        /// </summary>
        public static class ZoneType
        {
            // 存储类库区（可上架、有巷道/货位）
            public const string STORAGE = "STORAGE";
            public const string ASRS = "ASRS";
            public const string FLAT = "FLAT";
            public const string RAW_MATERIAL = "RAW_MATERIAL";
            public const string PICKING = "PICKING";

            // 功能类库区（功能区域，用于暂存/质检/收发等）
            public const string RECEIVING = "RECEIVING";
            public const string SHIPPING = "SHIPPING";
            public const string STAGING = "STAGING";
            public const string QC = "QC";
            public const string RETURNS = "RETURNS";

            /// <summary>
            /// 可用于上架/出库的存储类库区类型集合
            /// 只有这些类型的库区有巷道和货位，适合作为上架目标和出库来源
            /// </summary>
            public static readonly string[] StorageTypes =
            [
                STORAGE, ASRS, FLAT, RAW_MATERIAL
            ];

            /// <summary>
            /// 逻辑分区类型（用于 MdLogicalZone.ZoneType，标识分区在出库/上架流程中的角色）
            /// </summary>
            public const string LOGICAL_STORAGE = "STORAGE";
            public const string LOGICAL_PICKING = "PICKING";
            public const string LOGICAL_STAGING = "STAGING";

            /// <summary>
            /// 判断物理库区类型是否为存储类（可用于上架/出库）
            /// </summary>
            public static bool IsStorageZone(string? zoneType)
            {
                return !string.IsNullOrEmpty(zoneType) && StorageTypes.Contains(zoneType);
            }
        }

        /// <summary>
        /// 通用状态
        /// </summary>
        public static class Status
        {
            public const byte ENABLED = 1;
            public const byte DISABLED = 0;
        }

        /// <summary>
        /// 布尔标志（包内自洽，与宿主 BizConstants.BoolFlag 取值一致：YES=1/NO=0）
        /// </summary>
        public static class BoolFlag
        {
            public const byte YES = 1;
            public const byte NO = 0;
        }

        /// <summary>
        /// 接驳口类型（包内自洽，与宿主 BizConstants.TransferPointType 取值一致）
        /// </summary>
        public static class TransferPointType
        {
            public const string INBOUND = "INBOUND";
            public const string OUTBOUND = "OUTBOUND";
            public const string MIXED = "MIXED";
        }

        /// <summary>
        /// 站台方向
        /// </summary>
        public static class PortDirection
        {
            public const string INBOUND = "INBOUND";
            public const string OUTBOUND = "OUTBOUND";
        }

        /// <summary>
        /// 站台类型
        /// </summary>
        public static class PortType
        {
            public const string INBOUND = "INBOUND";
            public const string OUTBOUND = "OUTBOUND";
            public const string MIXED = "MIXED";
        }

        /// <summary>
        /// 路径优化方式
        /// </summary>
        public static class PathOptimization
        {
            public const string S_SHAPE = "S_SHAPE";
            public const string Z_SHAPE = "Z_SHAPE";
            public const string U_SHAPE = "U_SHAPE";
        }

        /// <summary>
        /// 执行模式
        /// </summary>
        public static class ExecutionMode
        {
            public const string CHAIN = "CHAIN";
            public const string PARALLEL = "PARALLEL";
            public const string STOP_ON_SUCCESS = "STOP_ON_SUCCESS";
        }

        /// <summary>
        /// 排序方向
        /// </summary>
        public static class SortDirection
        {
            public const string ASC = "ASC";
            public const string DESC = "DESC";
        }

        /// <summary>
        /// 排序字段
        /// </summary>
        public static class SortField
        {
            public const string AISLE_NO = "AisleNo";
            public const string ROW_NO = "RowNo";
            public const string COL_NO = "ColNo";
            public const string LAYER_NO = "LayerNo";
            public const string DEPTH = "Depth";
            public const string SCORE = "Score";
            public const string ZONE_CODE = "ZoneCode";
            public const string LOCATION_CODE = "LocationCode";
        }

        /// <summary>
        /// 双深货位分配模式
        /// </summary>
        public static class DoubleDeepMode
        {
            /// <summary>优先分配前排，前排满后再分配后排（推荐模式）</summary>
            public const string FRONT_FIRST = "FRONT_FIRST";

            /// <summary>优先分配后排（适用于先进后出场景）</summary>
            public const string BACK_FIRST = "BACK_FIRST";

            /// <summary>前后配对分配，同一地址前后排一起使用</summary>
            public const string PAIR = "PAIR";
        }

        /// <summary>
        /// 货位深度
        /// </summary>
        public static class DepthType
        {
            /// <summary>前排</summary>
            public const int FRONT = 1;

            /// <summary>后排</summary>
            public const int BACK = 2;
        }

        /// <summary>
        /// 默认最大推荐货位数量
        /// </summary>
        public const int DEFAULT_MAX_RECOMMEND_COUNT = 20;

        /// <summary>
        /// 策略链匹配评分权重（PolicyEngine.SelectBestChain 按此打分挑选最优链）。
        /// 数值仅用于区分匹配精度层级，权重关系：仓库 &gt; 库区 &gt; 单据类型 &gt; 订单类型。
        /// </summary>
        public static class ChainMatchScore
        {
            /// <summary>仓库匹配得分</summary>
            public const int WAREHOUSE = 1000;

            /// <summary>库区匹配得分</summary>
            public const int ZONE = 100;

            /// <summary>单据类型(DocType)精确匹配得分</summary>
            public const int DOC_TYPE = 10;

            /// <summary>订单类型(OrderType)兼容匹配得分，低于精确单据类型匹配</summary>
            public const int ORDER_TYPE = 5;
        }
    }
}
