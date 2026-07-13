namespace KH.WMS.Core.Logging.WMSError;

/// <summary>
/// WMS 错误代码定义
/// </summary>
public static class WMSErrorCodes
{
    #region 通用错误 (0001-0099)

    /// <summary>
    /// 操作成功
    /// </summary>
    public const string SUCCESS = "0000";

    /// <summary>
    /// 系统错误
    /// </summary>
    public const string SYSTEM_ERROR = "0001";

    /// <summary>
    /// 参数错误
    /// </summary>
    public const string PARAM_ERROR = "0002";

    /// <summary>
    /// 数据不存在
    /// </summary>
    public const string DATA_NOT_EXIST = "0003";

    /// <summary>
    /// 数据已存在
    /// </summary>
    public const string DATA_ALREADY_EXIST = "0004";

    /// <summary>
    /// 数据状态异常
    /// </summary>
    public const string DATA_STATUS_ERROR = "0005";

    /// <summary>
    /// 权限不足
    /// </summary>
    public const string PERMISSION_DENIED = "0006";

    /// <summary>
    /// 操作超时
    /// </summary>
    public const string OPERATION_TIMEOUT = "0007";

    /// <summary>
    /// 并发冲突
    /// </summary>
    public const string CONCURRENCY_CONFLICT = "0008";

    #endregion

    #region 入库管理 (0101-0199)

    /// <summary>
    /// 入库单不存在
    /// </summary>
    public const string INBOUND_NOT_EXIST = "0101";

    /// <summary>
    /// 入库单状态不允许操作
    /// </summary>
    public const string INBOUND_STATUS_ERROR = "0102";

    /// <summary>
    /// 入库单明细不存在
    /// </summary>
    public const string INBOUND_DETAIL_NOT_EXIST = "0103";

    /// <summary>
    /// 收货数量与ASN不符
    /// </summary>
    public const string RECEIVE_QTY_MISMATCH = "0104";

    /// <summary>
    /// 物料未在ASN中
    /// </summary>
    public const string SKU_NOT_IN_ASN = "0105";

    /// <summary>
    /// 收货库位不存在
    /// </summary>
    public const string RECEIVE_LOCATION_NOT_EXIST = "0106";

    /// <summary>
    /// 收货库位类型错误
    /// </summary>
    public const string RECEIVE_LOCATION_TYPE_ERROR = "0107";

    /// <summary>
    /// 上架指令不存在
    /// </summary>
    public const string PUT_AWAY_NOT_EXIST = "0108";

    /// <summary>
    /// 上架库位不存在
    /// </summary>
    public const string PUT_AWAY_LOCATION_NOT_EXIST = "0109";

    /// <summary>
    /// 上架库位容量不足
    /// </summary>
    public const string PUT_AWAY_LOCATION_FULL = "0110";

    /// <summary>
    /// 质检不合格
    /// </summary>
    public const string QC_FAILED = "0111";

    /// <summary>
    /// 待质检任务不存在
    /// </summary>
    public const string QC_TASK_NOT_EXIST = "0112";

    /// <summary>
    /// ASN不存在
    /// </summary>
    public const string ASN_NOT_EXIST = "0113";

    /// <summary>
    /// ASN已存在
    /// </summary>
    public const string ASN_ALREADY_EXIST = "0114";

    /// <summary>
    /// ASN状态不允许操作
    /// </summary>
    public const string ASN_STATUS_ERROR = "0115";

    #endregion

    #region 出库管理 (0201-0299)

    /// <summary>
    /// 出库单不存在
    /// </summary>
    public const string OUTBOUND_NOT_EXIST = "0201";

    /// <summary>
    /// 出库单状态不允许操作
    /// </summary>
    public const string OUTBOUND_STATUS_ERROR = "0202";

    /// <summary>
    /// 出库单明细不存在
    /// </summary>
    public const string OUTBOUND_DETAIL_NOT_EXIST = "0203";

    /// <summary>
    /// 分配失败
    /// </summary>
    public const string ALLOCATION_FAILED = "0204";

    /// <summary>
    /// 库存不足
    /// </summary>
    public const string INVENTORY_SHORTAGE = "0205";

    /// <summary>
    /// 波次不存在
    /// </summary>
    public const string WAVE_NOT_EXIST = "0206";

    /// <summary>
    /// 波次状态不允许操作
    /// </summary>
    public const string WAVE_STATUS_ERROR = "0207";

    /// <summary>
    /// 拣货任务不存在
    /// </summary>
    public const string PICK_TASK_NOT_EXIST = "0208";

    /// <summary>
    /// 拣货任务状态不允许操作
    /// </summary>
    public const string PICK_TASK_STATUS_ERROR = "0209";

    /// <summary>
    /// 拣货库位不存在
    /// </summary>
    public const string PICK_LOCATION_NOT_EXIST = "0210";

    /// <summary>
    /// 拣货数量异常
    /// </summary>
    public const string PICK_QTY_ERROR = "0211";

    /// <summary>
    /// 复核任务不存在
    /// </summary>
    public const string PACK_TASK_NOT_EXIST = "0212";

    /// <summary>
    /// 复核数量与订单不符
    /// </summary>
    public const string PACK_QTY_MISMATCH = "0213";

    /// <summary>
    /// 发货失败
    /// </summary>
    public const string SHIPMENT_FAILED = "0214";

    /// <summary>
    /// 运单号已存在
    /// </summary>
    public const string TRACKING_NO_ALREADY_EXIST = "0215";

    #endregion

    #region 库存管理 (0301-0399)

    /// <summary>
    /// 库存不存在
    /// </summary>
    public const string INVENTORY_NOT_EXIST = "0301";

    /// <summary>
    /// 库存被锁定
    /// </summary>
    public const string INVENTORY_LOCKED = "0302";

    /// <summary>
    /// 库存数量不足
    /// </summary>
    public const string INVENTORY_QTY_INSUFFICIENT = "0303";

    /// <summary>
    /// 批次号不存在
    /// </summary>
    public const string BATCH_NOT_EXIST = "0304";

    /// <summary>
    /// 批次已过期
    /// </summary>
    public const string BATCH_EXPIRED = "0305";

    /// <summary>
    /// 批次即将过期
    /// </summary>
    public const string BATCH_EXPIRING_SOON = "0306";

    /// <summary>
    /// 序列号已存在
    /// </summary>
    public const string SERIAL_NO_ALREADY_EXIST = "0307";

    /// <summary>
    /// 序列号不存在
    /// </summary>
    public const string SERIAL_NO_NOT_EXIST = "0308";

    /// <summary>
    /// 序列号已分配
    /// </summary>
    public const string SERIAL_NO_ALREADY_ASSIGNED = "0309";

    /// <summary>
    /// 库存冻结失败
    /// </summary>
    public const string INVENTORY_FREEZE_FAILED = "0310";

    /// <summary>
    /// 库存解冻失败
    /// </summary>
    public const string INVENTORY_UNFREEZE_FAILED = "0311";

    /// <summary>
    /// 库存调整失败
    /// </summary>
    public const string INVENTORY_ADJUST_FAILED = "0312";

    #endregion

    #region 库位管理 (0401-0499)

    /// <summary>
    /// 仓库不存在
    /// </summary>
    public const string WAREHOUSE_NOT_EXIST = "0401";

    /// <summary>
    /// 库区不存在
    /// </summary>
    public const string ZONE_NOT_EXIST = "0402";

    /// <summary>
    /// 库位不存在
    /// </summary>
    public const string LOCATION_NOT_EXIST = "0403";

    /// <summary>
    /// 库位已存在
    /// </summary>
    public const string LOCATION_ALREADY_EXIST = "0404";

    /// <summary>
    /// 库位已占用
    /// </summary>
    public const string LOCATION_OCCUPIED = "0405";

    /// <summary>
    /// 库位已满
    /// </summary>
    public const string LOCATION_FULL = "0406";

    /// <summary>
    /// 库位类型错误
    /// </summary>
    public const string LOCATION_TYPE_ERROR = "0407";

    /// <summary>
    /// 库位状态错误
    /// </summary>
    public const string LOCATION_STATUS_ERROR = "0408";

    /// <summary>
    /// 库位容量不足
    /// </summary>
    public const string LOCATION_CAPACITY_INSUFFICIENT = "0409";

    /// <summary>
    /// 库位重量超限
    /// </summary>
    public const string LOCATION_WEIGHT_EXCEEDED = "0410";

    /// <summary>
    /// 库位体积超限
    /// </summary>
    public const string LOCATION_VOLUME_EXCEEDED = "0411";

    /// <summary>
    /// 库位混合存储规则冲突
    /// </summary>
    public const string LOCATION_MIXING_RULE_CONFLICT = "0412";

    /// <summary>
    /// 库位物料限制冲突
    /// </summary>
    public const string LOCATION_SKU_LIMIT_CONFLICT = "0413";

    #endregion

    #region 盘点管理 (0501-0599)

    /// <summary>
    /// 盘点单不存在
    /// </summary>
    public const string COUNTING_NOT_EXIST = "0501";

    /// <summary>
    /// 盘点单状态不允许操作
    /// </summary>
    public const string COUNTING_STATUS_ERROR = "0502";

    /// <summary>
    /// 盘点明细不存在
    /// </summary>
    public const string COUNTING_DETAIL_NOT_EXIST = "0503";

    /// <summary>
    /// 盘点差异过大
    /// </summary>
    public const string COUNTING_DIFFERENCE_TOO_LARGE = "0504";

    /// <summary>
    /// 盘点任务不存在
    /// </summary>
    public const string COUNTING_TASK_NOT_EXIST = "0505";

    /// <summary>
    /// 盘点任务已完成
    /// </summary>
    public const string COUNTING_TASK_ALREADY_COMPLETED = "0506";

    /// <summary>
    /// 盘点盈亏处理失败
    /// </summary>
    public const string COUNTING_ADJUST_FAILED = "0507";

    /// <summary>
    /// 循环盘点计划不存在
    /// </summary>
    public const string CYCLE_COUNT_PLAN_NOT_EXIST = "0508";

    /// <summary>
    /// 盘点冻结失败
    /// </summary>
    public const string COUNTING_FREEZE_FAILED = "0509";

    #endregion

    #region 移库管理 (0601-0699)

    /// <summary>
    /// 移库单不存在
    /// </summary>
    public const string TRANSFER_NOT_EXIST = "0601";

    /// <summary>
    /// 移库单状态不允许操作
    /// </summary>
    public const string TRANSFER_STATUS_ERROR = "0602";

    /// <summary>
    /// 移库指令不存在
    /// </summary>
    public const string TRANSFER_TASK_NOT_EXIST = "0603";

    /// <summary>
    /// 源库位不存在
    /// </summary>
    public const string SOURCE_LOCATION_NOT_EXIST = "0604";

    /// <summary>
    /// 目标库位不存在
    /// </summary>
    public const string TARGET_LOCATION_NOT_EXIST = "0605";

    /// <summary>
    /// 源库位无库存
    /// </summary>
    public const string SOURCE_LOCATION_EMPTY = "0606";

    /// <summary>
    /// 目标库位已满
    /// </summary>
    public const string TARGET_LOCATION_FULL = "0607";

    /// <summary>
    /// 移库数量异常
    /// </summary>
    public const string TRANSFER_QTY_ERROR = "0608";

    /// <summary>
    /// 库存移动失败
    /// </summary>
    public const string INVENTORY_MOVE_FAILED = "0609";

    #endregion

    #region 物料管理 (0701-0799)

    /// <summary>
    /// 物料不存在
    /// </summary>
    public const string SKU_NOT_EXIST = "0701";

    /// <summary>
    /// 物料已存在
    /// </summary>
    public const string SKU_ALREADY_EXIST = "0702";

    /// <summary>
    /// 物料条码不存在
    /// </summary>
    public const string BARCODE_NOT_EXIST = "0703";

    /// <summary>
    /// 物料条码已存在
    /// </summary>
    public const string BARCODE_ALREADY_EXIST = "0704";

    /// <summary>
    /// 物料主数据不存在
    /// </summary>
    public const string SKU_MASTER_NOT_EXIST = "0705";

    /// <summary>
    /// 物料分类不存在
    /// </summary>
    public const string SKU_CATEGORY_NOT_EXIST = "0706";

    /// <summary>
    /// 物料属性不存在
    /// </summary>
    public const string SKU_ATTRIBUTE_NOT_EXIST = "0707";

    /// <summary>
    /// 物料包装不存在
    /// </summary>
    public const string SKU_PACKAGING_NOT_EXIST = "0708";

    /// <summary>
    /// 物料策略不存在
    /// </summary>
    public const string SKU_STRATEGY_NOT_EXIST = "0709";

    #endregion

    #region 货主管理 (0801-0899)

    /// <summary>
    /// 货主不存在
    /// </summary>
    public const string OWNER_NOT_EXIST = "0801";

    /// <summary>
    /// 货主已存在
    /// </summary>
    public const string OWNER_ALREADY_EXIST = "0802";

    /// <summary>
    /// 货主状态不允许操作
    /// </summary>
    public const string OWNER_STATUS_ERROR = "0803";

    /// <summary>
    /// 供应商不存在
    /// </summary>
    public const string SUPPLIER_NOT_EXIST = "0804";

    /// <summary>
    /// 供应商已存在
    /// </summary>
    public const string SUPPLIER_ALREADY_EXIST = "0805";

    /// <summary>
    /// 客户不存在
    /// </summary>
    public const string CUSTOMER_NOT_EXIST = "0806";

    /// <summary>
    /// 客户已存在
    /// </summary>
    public const string CUSTOMER_ALREADY_EXIST = "0807";

    #endregion

    #region 策略管理 (901-999)

    /// <summary>
    /// 策略不存在
    /// </summary>
    public const string STRATEGY_NOT_EXIST = "901";

    /// <summary>
    /// 策略冲突
    /// </summary>
    public const string STRATEGY_CONFLICT = "902";

    /// <summary>
    /// 上架策略不存在
    /// </summary>
    public const string PUT_AWAY_STRATEGY_NOT_EXIST = "903";

    /// <summary>
    /// 拣货策略不存在
    /// </summary>
    public const string PICK_STRATEGY_NOT_EXIST = "904";

    /// <summary>
    /// 分配策略不存在
    /// </summary>
    public const string ALLOCATION_STRATEGY_NOT_EXIST = "905";

    /// <summary>
    /// 波次策略不存在
    /// </summary>
    public const string WAVE_STRATEGY_NOT_EXIST = "907";

    /// <summary>
    /// 路径策略不存在
    /// </summary>
    public const string ROUTE_STRATEGY_NOT_EXIST = "908";

    /// <summary>
    /// ABC策略不存在
    /// </summary>
    public const string ABC_STRATEGY_NOT_EXIST = "909";

    #endregion

    #region 系统集成 (1001-1099)

    /// <summary>
    /// ERP接口调用失败
    /// </summary>
    public const string ERP_API_FAILED = "1001";

    /// <summary>
    /// TMS接口调用失败
    /// </summary>
    public const string TMS_API_FAILED = "1002";

    /// <summary>
    /// 接口数据格式错误
    /// </summary>
    public const string INTERFACE_DATA_FORMAT_ERROR = "1003";

    /// <summary>
    /// 接口数据验证失败
    /// </summary>
    public const string INTERFACE_DATA_VALIDATION_FAILED = "1004";

    /// <summary>
    /// 接口超时
    /// </summary>
    public const string INTERFACE_TIMEOUT = "1005";

    /// <summary>
    /// 接口连接失败
    /// </summary>
    public const string INTERFACE_CONNECTION_FAILED = "1006";

    #endregion

    #region 任务管理 (1101-1199)

    /// <summary>
    /// 任务不存在
    /// </summary>
    public const string TASK_NOT_EXIST = "1101";

    /// <summary>
    /// 任务状态不允许操作
    /// </summary>
    public const string TASK_STATUS_ERROR = "1102";

    /// <summary>
    /// 上架任务未分配货位
    /// </summary>
    public const string TASK_NOT_PUTAWAY = "1103";

    /// <summary>
    /// 任务取消失败
    /// </summary>
    public const string TASK_CANCEL_FAILED = "1104";

    /// <summary>
    /// 任务完成处理失败
    /// </summary>
    public const string TASK_COMPLETE_FAILED = "1105";

    /// <summary>
    /// 拣货任务未关联出库单
    /// </summary>
    public const string TASK_NOT_LINKED_OUTBOUND = "1106";

    /// <summary>
    /// 拣货完成处理失败
    /// </summary>
    public const string TASK_PICK_COMPLETE_FAILED = "1107";

    /// <summary>
    /// 任务不是上架任务，无法分配货位
    /// </summary>
    public const string TASK_LOCATION_NOT_ALLOCATED = "1108";

    /// <summary>
    /// 货位分配失败
    /// </summary>
    public const string TASK_LOCATION_ALLOCATE_FAILED = "1109";

    /// <summary>
    /// 库区无可用空闲库位
    /// </summary>
    public const string TASK_LOCATION_NO_AVAILABLE = "1110";

    /// <summary>
    /// 货位分配策略执行失败
    /// </summary>
    public const string TASK_LOCATION_STRATEGY_FAILED = "1111";

    /// <summary>
    /// TaskNo和WcsTaskNo至少提供一个
    /// </summary>
    public const string TASK_AT_LEAST_ONE_ID = "1112";

    /// <summary>
    /// 任务执行异常
    /// </summary>
    public const string TASK_EXCEPTION_OCCURRED = "1113";

    #endregion

    #region 组盘管理 (1201-1299)

    /// <summary>
    /// 组盘数据不能为空
    /// </summary>
    public const string CONTAINER_BIND_DATA_EMPTY = "1201";

    /// <summary>
    /// 入库单明细不存在
    /// </summary>
    public const string CONTAINER_BIND_DETAIL_NOT_EXIST = "1202";

    /// <summary>
    /// 组盘失败
    /// </summary>
    public const string CONTAINER_BIND_FAILED = "1203";

    /// <summary>
    /// 组盘记录不存在
    /// </summary>
    public const string CONTAINER_BIND_RECORD_NOT_EXIST = "1204";

    /// <summary>
    /// 容器状态不允许上架
    /// </summary>
    public const string CONTAINER_BIND_STATUS_ERROR = "1205";

    /// <summary>
    /// 请求上架失败
    /// </summary>
    public const string CONTAINER_BIND_PUTAWAY_FAILED = "1206";

    /// <summary>
    /// 容器编号不能为空
    /// </summary>
    public const string CONTAINER_CODE_EMPTY = "1207";

    /// <summary>
    /// 入库口编号不能为空
    /// </summary>
    public const string INBOUND_STATION_CODE_EMPTY = "1208";

    /// <summary>
    /// 容器没有处于已组盘状态的记录
    /// </summary>
    public const string CONTAINER_NO_BOUND_RECORD = "1209";

    /// <summary>
    /// WCS申请上架失败
    /// </summary>
    public const string CONTAINER_BIND_WCS_PUTAWAY_FAILED = "1210";

    /// <summary>
    /// 请选择要上架的组盘记录
    /// </summary>
    public const string CONTAINER_BIND_SELECT_REQUIRED = "1211";

    /// <summary>
    /// 容器未关联仓库
    /// </summary>
    public const string CONTAINER_BIND_NO_WAREHOUSE = "1212";

    /// <summary>
    /// 创建上架任务失败
    /// </summary>
    public const string CONTAINER_BIND_PUTAWAY_TASK_FAILED = "1213";

    /// <summary>
    /// 容器已有活跃组盘记录（BOUND/PUTTING_AWAY）
    /// </summary>
    public const string CONTAINER_HAS_ACTIVE_BIND = "1214";

    /// <summary>
    /// 容器已有库存记录，不允许重复组盘
    /// </summary>
    public const string CONTAINER_HAS_INVENTORY = "1215";

    /// <summary>
    /// 容器状态不允许操作
    /// </summary>
    public const string CONTAINER_STATUS_ERROR = "1216";

    /// <summary>
    /// 容器已有活跃任务（PENDING/IN_PROGRESS）
    /// </summary>
    public const string CONTAINER_HAS_ACTIVE_TASK = "1217";

    /// <summary>
    /// 库存已冻结，不允许操作
    /// </summary>
    public const string INVENTORY_FROZEN = "1218";

    /// <summary>
    /// 上架任务创建成功（含任务号）
    /// </summary>
    public const string PUTAWAY_TASK_CREATED_WITH_NO = "9012";

    #endregion

    #region 分配管理 (1301-1399)

    /// <summary>
    /// 出库单无明细行
    /// </summary>
    public const string ALLOCATION_NO_DETAIL = "1301";

    /// <summary>
    /// 单行分配失败
    /// </summary>
    public const string ALLOCATION_SINGLE_LINE_FAILED = "1302";

    /// <summary>
    /// 库存明细不存在
    /// </summary>
    public const string ALLOCATION_DETAIL_NOT_EXIST = "1303";

    /// <summary>
    /// 物料库存不足（含明细）
    /// </summary>
    public const string ALLOCATION_INVENTORY_SHORTAGE_DETAIL = "1304";

    /// <summary>
    /// 生成出库任务失败
    /// </summary>
    public const string ALLOCATION_TASK_GENERATE_FAILED = "1305";

    /// <summary>
    /// 分配头不存在
    /// </summary>
    public const string ALLOCATION_HEAD_NOT_EXIST = "1306";

    /// <summary>
    /// 任务无需处理的分配明细
    /// </summary>
    public const string ALLOCATION_NO_NEED_PROCESS = "1307";

    /// <summary>
    /// 拣货任务完成处理信息
    /// </summary>
    public const string ALLOCATION_PICK_COMPLETE_INFO = "1308";

    /// <summary>
    /// 分配失败（通用）
    /// </summary>
    public const string ALLOCATE_FAILED = "1309";

    #endregion

    #region 无单据操作 (1401-1499)

    /// <summary>
    /// 无单据操作失败
    /// </summary>
    public const string ADHOC_OPERATION_FAILED = "1401";

    /// <summary>
    /// 起始库位不存在
    /// </summary>
    public const string ADHOC_FROM_LOCATION_NOT_EXIST = "1402";

    /// <summary>
    /// 起始库位状态不允许操作
    /// </summary>
    public const string ADHOC_FROM_LOCATION_STATUS_ERROR = "1403";

    /// <summary>
    /// 目的库位状态不允许操作（已占用或已预留）
    /// </summary>
    public const string ADHOC_TO_LOCATION_STATUS_ERROR = "1404";

    /// <summary>
    /// 容器无库存记录
    /// </summary>
    public const string ADHOC_CONTAINER_NO_INVENTORY = "1405";

    /// <summary>
    /// 货位无库存记录
    /// </summary>
    public const string ADHOC_LOCATION_NO_INVENTORY = "1406";

    /// <summary>
    /// 无可用库存可出库
    /// </summary>
    public const string ADHOC_NO_AVAILABLE_INVENTORY = "1407";

    /// <summary>
    /// 库存锁定失败
    /// </summary>
    public const string ADHOC_INVENTORY_LOCK_FAILED = "1408";

    /// <summary>
    /// 起始位置编码不能为空
    /// </summary>
    public const string ADHOC_FROM_LOCATION_CODE_EMPTY = "1409";

    /// <summary>
    /// 目的位置编码不能为空
    /// </summary>
    public const string ADHOC_TO_LOCATION_CODE_EMPTY = "1410";

    /// <summary>
    /// 货位编码不能为空
    /// </summary>
    public const string ADHOC_LOCATION_CODE_EMPTY = "1411";

    /// <summary>
    /// 无单据入库组盘物料不能为空
    /// </summary>
    public const string ADHOC_INBOUND_LINES_EMPTY = "1412";

    /// <summary>
    /// 无单据出库物料不能为空
    /// </summary>
    public const string ADHOC_OUTBOUND_LINES_EMPTY = "1413";

    #endregion

    #region 配置管理 (1501-1599)

    /// <summary>
    /// 配置项不存在
    /// </summary>
    public const string CONFIG_NOT_FOUND = "1501";

    /// <summary>
    /// 托盘不允许绑定不同批次
    /// </summary>
    public const string CONTAINER_MIXED_BATCH = "1502";

    /// <summary>
    /// 托盘不允许绑定不同物料
    /// </summary>
    public const string CONTAINER_MIXED_MATERIAL = "1503";

    /// <summary>
    /// 启用批次管理时批次号必填
    /// </summary>
    public const string BATCH_NO_REQUIRED = "1504";

    /// <summary>
    /// 启用有效期管理时有效期必填
    /// </summary>
    public const string EXPIRY_DATE_REQUIRED = "1505";

    /// <summary>
    /// 不允许拣货数量大于计划数量
    /// </summary>
    public const string OVER_PICK_NOT_ALLOWED = "1506";

    #endregion

    #region 成功消息 (9001-9099)

    /// <summary>
    /// 接收成功
    /// </summary>
    public const string RECEIVE_SUCCESS = "9001";

    /// <summary>
    /// 收货成功
    /// </summary>
    public const string INBOUND_RECEIVE_SUCCESS = "9002";

    /// <summary>
    /// 收货并组盘成功
    /// </summary>
    public const string RECEIVE_AND_BIND_SUCCESS = "9003";

    /// <summary>
    /// 组盘创建成功
    /// </summary>
    public const string CONTAINER_BIND_CREATED = "9004";

    /// <summary>
    /// 上架任务创建成功
    /// </summary>
    public const string PUTAWAY_TASK_CREATED = "9005";

    /// <summary>
    /// 出库单确认成功
    /// </summary>
    public const string OUTBOUND_CONFIRM_SUCCESS = "9006";

    /// <summary>
    /// 分配成功
    /// </summary>
    public const string ALLOCATION_SUCCESS = "9007";

    /// <summary>
    /// 出库任务生成成功
    /// </summary>
    public const string OUTBOUND_TASK_CREATED = "9008";

    /// <summary>
    /// 任务完成成功
    /// </summary>
    public const string TASK_COMPLETE_SUCCESS = "9009";

    /// <summary>
    /// 任务已取消
    /// </summary>
    public const string TASK_CANCEL_SUCCESS = "9010";

    /// <summary>
    /// 货位分配成功
    /// </summary>
    public const string TASK_LOCATION_ALLOCATE_SUCCESS = "9011";

    #endregion

    #region 入库/出库操作（描述性编码）
    // 注意：以下编码为描述性字符串（非数字编码），与 WMSErrorMessages 字典 key 一一对应，
    // 值即键名，保持向前兼容。调用方应通过常量引用，避免裸字面量。
    /// <summary>收货数据不能为空</summary>
    public const string RECEIVE_DATA_EMPTY = "RECEIVE_DATA_EMPTY";

    /// <summary>收货失败</summary>
    public const string RECEIVE_FAILED = "RECEIVE_FAILED";

    /// <summary>收货并组盘失败</summary>
    public const string RECEIVE_AND_BIND_FAILED = "RECEIVE_AND_BIND_FAILED";

    /// <summary>接收失败</summary>
    public const string RECEIVE_INBOUND_FAILED = "RECEIVE_INBOUND_FAILED";

    /// <summary>出库单无明细行，无法确认</summary>
    public const string OUTBOUND_NO_DETAIL_FOR_CONFIRM = "OUTBOUND_NO_DETAIL_FOR_CONFIRM";

    /// <summary>任务状态不允许完成</summary>
    public const string TASK_STATUS_CANNOT_COMPLETE = "TASK_STATUS_CANNOT_COMPLETE";

    /// <summary>任务已分配目标库位，不能重复分配</summary>
    public const string TASK_ALREADY_ALLOCATED = "TASK_ALREADY_ALLOCATED";

    /// <summary>任务状态不允许分配货位</summary>
    public const string TASK_STATUS_CANNOT_ALLOCATE = "TASK_STATUS_CANNOT_ALLOCATE";
    #endregion
}
