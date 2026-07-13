namespace KH.WMS.Algorithms.Strategy.Constants
{
    /// <summary>
    /// 策略上下文参数键常量
    /// <para>
    /// 所有策略之间通过 IPolicyContext 的 GetData/SetData 传递参数，
    /// 此类集中定义所有键名，避免硬编码字符串。
    /// </para>
    /// <para>参数分为三类：</para>
    /// <list type="bullet">
    ///   <item>【服务注入键】- 由调用方注入查询服务实例，供策略内部使用</item>
    ///   <item>【策略输入键】- 由调用方或上游策略设置，当前策略读取</item>
    ///   <item>【策略输出键】- 由策略基类在执行成功后写入，供下游策略读取</item>
    /// </list>
    /// </summary>
    public static class StrategyParams
    {
        // =====================================================================
        //  服务注入键 — 由调用方通过 context.SetData 注入，策略内部 GetData 获取
        //  所有策略共用，调用方必须注入策略所需的服务
        // =====================================================================

        /// <summary>
        /// 服务注入：SqlSugar 数据库客户端
        /// <para>类型: ISqlSugarClient</para>
        /// <para>使用策略: 需要直接查询数据库的策略</para>
        /// </summary>
        public const string SVC_SQL_SUGAR_CLIENT = "ISqlSugarClient";

        /// <summary>
        /// 服务注入：库存查询服务
        /// <para>类型: IInventoryQueryService</para>
        /// <para>使用策略: InventoryAllocation (FIFO/FEFO/Batch)</para>
        /// <para>必须: 是</para>
        /// </summary>
        public const string SVC_INVENTORY_QUERY = "IInventoryQueryService";

        /// <summary>
        /// 服务注入：仓库查询服务（库区、巷道、站台等）
        /// <para>类型: IWarehouseQueryService</para>
        /// <para>使用策略: Putaway, Picking, LocationAllocation (ABC/Category)</para>
        /// <para>必须: 是</para>
        /// </summary>
        public const string SVC_WAREHOUSE_QUERY = "IWarehouseQueryService";

        /// <summary>
        /// 服务注入：库位查询服务（空闲货位、附近货位等）
        /// <para>类型: ILocationQueryService</para>
        /// <para>使用策略: Putaway, LocationAllocation (ABC/Category/Centralized)</para>
        /// <para>必须: 是</para>
        /// </summary>
        public const string SVC_LOCATION_QUERY = "ILocationQueryService";

        // =====================================================================
        //  上架策略参数 (Putaway)
        //  策略编码: DEFAULT_PUTAWAY
        //  流程: 确定入库口 → 确定目标库区 → 确定巷道 → 请求货位分配
        // =====================================================================

        /// <summary>
        /// 上架策略：策略输出键 — 上架策略执行结果
        /// <para>类型: PutawayResult</para>
        /// <para>由 PutawayStrategyBase.ExecuteAsync 写入</para>
        /// </summary>
        public static class PutawayOutput
        {
            /// <summary>上架策略执行结果（写入上下文供下游读取）</summary>
            public const string RESULT = "PutawayResult";
        }

        /// <summary>
        /// 上架策略：输入参数键 — 由调用方或上游设置
        /// </summary>
        public static class PutawayInput
        {
            /// <summary>
            /// 单据类型ID，用于匹配入库口站台
            /// <para>类型: long?</para>
            /// <para>可选。为空时跳过站台匹配</para>
            /// <para>来源: 调用方从单据配置中获取</para>
            /// </summary>
            public const string DOC_TYPE_ID = "DocTypeId";

            /// <summary>
            /// 入库口站台ID（优先使用，跳过自动匹配）
            /// <para>类型: string</para>
            /// <para>可选。设置后跳过通过 DocTypeId 自动匹配</para>
            /// </summary>
            public const string INBOUND_STATION_ID = "InboundStationId";

            /// <summary>
            /// 入库口站台编码（优先使用，跳过自动匹配）
            /// <para>类型: string</para>
            /// <para>可选。设置后跳过通过 DocTypeId 自动匹配</para>
            /// </summary>
            public const string INBOUND_STATION_CODE = "InboundStationCode";

            /// <summary>
            /// 物料分类ID，用于匹配物料分类级库区策略
            /// <para>类型: long?</para>
            /// <para>可选。配合库区分配策略使用</para>
            /// </summary>
            public const string MATERIAL_CATEGORY_ID = "MaterialCategoryId";

            /// <summary>
            /// 是否启用库区分区（按物料分类匹配库区）
            /// <para>类型: bool?，默认 true</para>
            /// <para>可选。关闭后跳过库区匹配，直接在仓库内查找</para>
            /// </summary>
            public const string ENABLE_ZONE_PARTITION = "EnableZonePartition";

            /// <summary>
            /// 是否启用ABC分类库区推荐（按物料周转率匹配库区）
            /// <para>类型: bool?，默认 true</para>
            /// <para>可选。关闭后跳过ABC分类匹配</para>
            /// </summary>
            public const string ENABLE_ABC_CLASS = "EnableAbcClass";

            /// <summary>
            /// 是否启用就近巷道推荐（选择负载最低的巷道）
            /// <para>类型: bool?，默认 true</para>
            /// <para>可选。关闭后随机分配巷道</para>
            /// </summary>
            public const string ENABLE_NEARBY = "EnableNearby";

            /// <summary>
            /// 目标库区ID（由上架策略写入 AllocationParameters，供下游货位分配策略读取）
            /// <para>类型: long</para>
            /// <para>可选。指定货位分配的目标库区范围</para>
            /// </summary>
            public const string TARGET_ZONE_ID = "TargetZoneId";

            /// <summary>
            /// 目标巷道ID（由上架策略写入 AllocationParameters，供下游货位分配策略读取）
            /// <para>类型: long</para>
            /// <para>可选。指定货位分配的目标巷道范围</para>
            /// </summary>
            public const string TARGET_AISLE_ID = "TargetAisleId";
        }

        // =====================================================================
        //  货位分配策略参数 (LocationAllocation)
        //  策略编码: ABC_CLASS / CATEGORY_ZONE / CENTRALIZED / DOUBLE_DEEP
        //  流程: 根据策略规则筛选并排序候选货位，输出推荐货位列表
        // =====================================================================

        /// <summary>
        /// 货位分配策略：策略输出键
        /// </summary>
        public static class LocationAllocationOutput
        {
            /// <summary>货位分配策略执行结果</summary>
            public const string RESULT = "LocationAllocationResult";
        }

        /// <summary>
        /// 货位分配策略：输入参数键
        /// </summary>
        public static class LocationAllocationInput
        {
            /// <summary>
            /// 物料ABC分类编码（如 A/B/C）
            /// <para>类型: string</para>
            /// <para>ABC分类策略必须。由上架策略写入上下文</para>
            /// <para>来源: 通过 IWarehouseQueryService.GetMaterialTurnoverClassAsync 查询</para>
            /// </summary>
            public const string MATERIAL_ABC_CLASS = "MaterialAbcClass";

            /// <summary>
            /// 目标批次号，用于集中存储策略匹配同批次货位
            /// <para>类型: string</para>
            /// <para>可选。设置后集中存储策略优先推荐同批次附近的货位</para>
            /// </summary>
            public const string TARGET_BATCH_NO = "TargetBatchNo";

            /// <summary>
            /// 最大查找附近货位数量
            /// <para>类型: int?，默认 10</para>
            /// <para>可选。控制集中存储策略在每个已有库存位置附近查找的货位数</para>
            /// </summary>
            public const string MAX_NEARBY_COUNT = "MaxNearbyCount";

            /// <summary>
            /// 策略配置参数（JSON格式，从 CfgStrategyConfig.StrategyParams 读取）
            /// <para>类型: string</para>
            /// <para>可选。包含策略级别的自定义参数</para>
            /// </summary>
            public const string STRATEGY_PARAMS = "StrategyParams";

            /// <summary>
            /// 步骤参数（JSON格式，从 CfgStrategyChainStep.StepParams 读取）
            /// <para>类型: string</para>
            /// <para>可选。可覆盖策略配置中的默认参数</para>
            /// </summary>
            public const string STEP_PARAMS = "StepParams";

            /// <summary>
            /// 最大推荐货位数量（限制返回结果数量，避免数据量过大）
            /// <para>类型: int?，默认 20</para>
            /// <para>可选。通过 StepParams 中的 MaxRecommendCount 配置</para>
            /// </summary>
            public const string MAX_RECOMMEND_COUNT = "MaxRecommendCount";

            /// <summary>
            /// 排序规则配置（JSON数组，用于基类 PostProcess 中的可配置排序）
            /// <para>类型: string (JSON)</para>
            /// <para>可选。筛选策略内部对推荐列表进行排序，无需额外配置排序步骤</para>
            /// <para>格式: [{ "Field": "LayerNo", "Direction": "ASC" }, ...]</para>
            /// </summary>
            public const string SORT_RULES = "SortRules";

            /// <summary>
            /// 是否启用双深货位约束
            /// <para>类型: bool?，默认 false</para>
            /// <para>启用后，分配策略会检查双深货位的前后排约束关系</para>
            /// </summary>
            public const string ENABLE_DOUBLE_DEEP = "EnableDoubleDeep";

            /// <summary>
            /// 双深货位分配模式
            /// <para>类型: string，可选值: FRONT_FIRST / BACK_FIRST / PAIR</para>
            /// <para>FRONT_FIRST: 优先分配前排，前排满后再分配后排</para>
            /// <para>BACK_FIRST: 优先分配后排（适用于先进后出场景）</para>
            /// <para>PAIR: 前后配对分配，同一地址前后排一起使用</para>
            /// </summary>
            public const string DOUBLE_DEEP_MODE = "DoubleDeepMode";
        }

        // =====================================================================
        //  出库分配策略参数 (OutboundAllocation)
        //  策略编码: WHOLE_PALLET_FIRST / ZONE_PRIORITY / SCATTERED_PICK
        //  流程: 决定出库时从哪个逻辑分区/库位类型取货，整托/散件策略
        //  出库流程: OutboundAllocation(选分区) → InventoryAllocation(选批次)
        // =====================================================================

        /// <summary>
        /// 出库分配策略：策略输出键
        /// </summary>
        public static class OutboundAllocationOutput
        {
            /// <summary>出库分配策略执行结果</summary>
            public const string RESULT = "OutboundAllocationResult";
        }

        /// <summary>
        /// 出库分配策略：输入参数键
        /// </summary>
        public static class OutboundAllocationInput
        {
            /// <summary>
            /// 目标分区ID（由出库分配策略写入，供库存分配策略读取以限定分区范围）
            /// <para>类型: long?</para>
            /// <para>可选。设置后库存分配仅在指定分区内查找</para>
            /// </summary>
            public const string TARGET_ZONE_ID = "OutboundTargetZoneId";

            /// <summary>
            /// 目标分区编码
            /// <para>类型: string</para>
            /// <para>可选。</para>
            /// </summary>
            public const string TARGET_ZONE_CODE = "OutboundTargetZoneCode";

            /// <summary>
            /// 出库分配模式（FULL_PALLET / SCATTERED / MIXED）
            /// <para>类型: string</para>
            /// <para>FULL_PALLET: 优先整托出库</para>
            /// <para>SCATTERED: 仅散件拣选</para>
            /// <para>MIXED: 整托+散件混合</para>
            /// </summary>
            public const string ALLOCATION_MODE = "AllocationMode";

            /// <summary>
            /// 是否启用整托优先
            /// <para>类型: bool?，默认 true</para>
            /// </summary>
            public const string ENABLE_WHOLE_PALLET_FIRST = "EnableWholePalletFirst";

            /// <summary>
            /// 分区优先级列表（JSON数组，ZonePriorityStrategy 使用）
            /// <para>格式: [{ "ZoneId": 1, "ZoneCode": "ZONE_A", "Priority": 1 }]</para>
            /// </summary>
            public const string ZONE_PRIORITY_LIST = "ZonePriorityList";

            /// <summary>
            /// 策略配置参数（JSON格式）
            /// </summary>
            public const string STRATEGY_PARAMS = "OutboundStrategyParams";

            /// <summary>
            /// 步骤参数（JSON格式）
            /// </summary>
            public const string STEP_PARAMS = "OutboundStepParams";
        }

        // =====================================================================
        //  库存分配策略参数 (InventoryAllocation)
        //  策略编码: FIFO / FEFO / BATCH
        //  流程: 根据规则选择最优库存，输出分配明细
        // =====================================================================

        /// <summary>
        /// 库存分配策略：策略输出键
        /// </summary>
        public static class InventoryAllocationOutput
        {
            /// <summary>库存分配策略执行结果</summary>
            public const string RESULT = "InventoryAllocationResult";
        }

        /// <summary>
        /// 库存分配策略：输入参数键
        /// </summary>
        public static class InventoryAllocationInput
        {
            /// <summary>
            /// 仓库编码
            /// <para>类型: string</para>
            /// <para>必须。用于限定库存查询范围</para>
            /// <para>优先从上下文 ContextData 获取，回退到 IPolicyContext.WarehouseCode</para>
            /// </summary>
            public const string WAREHOUSE_CODE = "WarehouseCode";

            /// <summary>
            /// 物料编码
            /// <para>类型: string</para>
            /// <para>必须。指定需要分配的物料</para>
            /// </summary>
            public const string MATERIAL_CODE = "MaterialCode";

            /// <summary>
            /// 需求数量
            /// <para>类型: decimal</para>
            /// <para>必须。需分配的总数量，策略会按规则分配到满足需求为止</para>
            /// </summary>
            public const string REQUIRED_QTY = "RequiredQty";

            /// <summary>
            /// 指定批次号（仅 BatchStrategy 使用）
            /// <para>类型: string</para>
            /// <para>可选。设置后优先分配指定批次的库存，不足时按 FIFO 补充</para>
            /// </summary>
            public const string TARGET_BATCH_NO = "TargetBatchNo";
        }

        // =====================================================================
        //  下架策略参数 (Picking)
        //  策略编码: DEFAULT_PICKING
        //  流程: 调度库存分配 → 确定取货路径 → 确定目标出口 → 生成送达路径
        // =====================================================================

        /// <summary>
        /// 下架策略：策略输出键
        /// </summary>
        public static class PickingOutput
        {
            /// <summary>下架策略执行结果</summary>
            public const string RESULT = "PickingResult";
        }

        /// <summary>
        /// 下架策略：输入参数键
        /// </summary>
        public static class PickingInput
        {
            /// <summary>
            /// 目标出口/站台ID（优先使用，跳过自动匹配）
            /// <para>类型: long</para>
            /// <para>可选。设置后跳过通过 DocTypeId 自动匹配出口</para>
            /// </summary>
            public const string DESTINATION_STATION_ID = "DestinationStationId";

            /// <summary>
            /// 目标出口/站台编码（优先使用，跳过自动匹配）
            /// <para>类型: string</para>
            /// <para>可选。</para>
            /// </summary>
            public const string DESTINATION_STATION_CODE = "DestinationStationCode";

            /// <summary>
            /// 目标区域ID
            /// <para>类型: long</para>
            /// <para>可选。出库暂存区、出库站台所在区域</para>
            /// </summary>
            public const string DESTINATION_ZONE_ID = "DestinationZoneId";

            /// <summary>
            /// 目标区域编码
            /// <para>类型: string</para>
            /// <para>可选。</para>
            /// </summary>
            public const string DESTINATION_ZONE_CODE = "DestinationZoneCode";

            /// <summary>
            /// 单据类型ID，用于自动匹配出库口站台
            /// <para>类型: long?</para>
            /// <para>可选。未指定目标出口时通过此ID匹配</para>
            /// </summary>
            public const string DOC_TYPE_ID = "DocTypeId";

            /// <summary>
            /// 是否启用整托优先出库
            /// <para>类型: bool?，默认 true</para>
            /// <para>可选。启用后优先分配整托出库</para>
            /// </summary>
            public const string ENABLE_PALLET_FIRST = "EnablePalletFirst";

            /// <summary>
            /// 源巷道号，货物所在巷道，用于按巷道匹配出库口
            /// <para>类型: int</para>
            /// <para>可选。未配置 DocTypePort 时通过巷道→接驳口→输送线→站台链路查找出库口</para>
            /// </summary>
            public const string SOURCE_AISLE_NO = "SourceAisleNo";
        }

        // =====================================================================
        //  通用上下文数据键 — 多个策略共用的数据
        // =====================================================================

        /// <summary>
        /// 通用上下文数据键
        /// </summary>
        public static class Common
        {
            /// <summary>
            /// 仓库ID（备选，当 IPolicyContext.WarehouseId 为空时从 ContextData 读取）
            /// <para>类型: long</para>
            /// <para>使用策略: Putaway, Picking, LocationAllocation</para>
            /// <para>优先使用 IPolicyContext.WarehouseId 属性，此键为备用</para>
            /// </summary>
            public const string WAREHOUSE_ID = "WarehouseId";

            /// <summary>
            /// 物料ID（备选，当 IPolicyContext.MaterialId 为空时从 ContextData 读取）
            /// <para>类型: long</para>
            /// <para>使用策略: Putaway, LocationAllocation (ABC/Centralized)</para>
            /// <para>优先使用 IPolicyContext.MaterialId 属性，此键为备用</para>
            /// </summary>
            public const string MATERIAL_ID = "MaterialId";

            /// <summary>
            /// 路径优化方式（S_SHAPE / Z_SHAPE / U_SHAPE）
            /// <para>类型: string，默认 "S_SHAPE"（见 AlgoConstants.PathOptimization）</para>
            /// <para>使用策略: Putaway, Picking</para>
            /// <para>可选。控制堆垛机/AGV的行走路径优化算法</para>
            /// </summary>
            public const string PATH_OPTIMIZATION = "PathOptimization";
        }
    }
}
