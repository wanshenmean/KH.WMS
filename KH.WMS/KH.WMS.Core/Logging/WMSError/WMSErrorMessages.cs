using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KH.WMS.Core.Logging.WMSError
{

    /// <summary>
    /// WMS 错误代码说明
    /// </summary>
    public static class WMSErrorMessages
    {
        private static readonly Dictionary<string, string> _messages = new()
        {
            #region 通用错误
            // 通用错误
            [WMSErrorCodes.SUCCESS] = "操作成功",
            [WMSErrorCodes.SYSTEM_ERROR] = "系统错误，请稍后重试",
            [WMSErrorCodes.PARAM_ERROR] = "参数错误",
            [WMSErrorCodes.DATA_NOT_EXIST] = "数据不存在",
            [WMSErrorCodes.DATA_ALREADY_EXIST] = "数据已存在",
            [WMSErrorCodes.DATA_STATUS_ERROR] = "数据状态异常",
            [WMSErrorCodes.PERMISSION_DENIED] = "权限不足",
            [WMSErrorCodes.OPERATION_TIMEOUT] = "操作超时",
            [WMSErrorCodes.CONCURRENCY_CONFLICT] = "数据被其他用户修改，请刷新后重试",
            #endregion

            #region 入库管理
            // 入库管理
            [WMSErrorCodes.INBOUND_NOT_EXIST] = "入库单不存在",
            [WMSErrorCodes.INBOUND_STATUS_ERROR] = "入库单状态不允许该操作",
            [WMSErrorCodes.INBOUND_DETAIL_NOT_EXIST] = "入库单明细不存在",
            [WMSErrorCodes.RECEIVE_QTY_MISMATCH] = "收货数量与ASN不符",
            [WMSErrorCodes.SKU_NOT_IN_ASN] = "商品未在ASN中",
            [WMSErrorCodes.RECEIVE_LOCATION_NOT_EXIST] = "收货库位不存在",
            [WMSErrorCodes.RECEIVE_LOCATION_TYPE_ERROR] = "收货库位类型错误",
            [WMSErrorCodes.PUT_AWAY_NOT_EXIST] = "上架指令不存在",
            [WMSErrorCodes.PUT_AWAY_LOCATION_NOT_EXIST] = "上架库位不存在",
            [WMSErrorCodes.PUT_AWAY_LOCATION_FULL] = "上架库位容量不足",
            [WMSErrorCodes.QC_FAILED] = "质检不合格",
            [WMSErrorCodes.QC_TASK_NOT_EXIST] = "质检任务不存在",
            [WMSErrorCodes.ASN_NOT_EXIST] = "ASN不存在",
            [WMSErrorCodes.ASN_ALREADY_EXIST] = "ASN已存在",
            [WMSErrorCodes.ASN_STATUS_ERROR] = "ASN状态不允许该操作",
            #endregion

            #region 出库管理
            // 出库管理
            [WMSErrorCodes.OUTBOUND_NOT_EXIST] = "出库单不存在",
            [WMSErrorCodes.OUTBOUND_STATUS_ERROR] = "出库单状态不允许该操作",
            [WMSErrorCodes.OUTBOUND_DETAIL_NOT_EXIST] = "出库单明细不存在",
            [WMSErrorCodes.ALLOCATION_FAILED] = "库存分配失败",
            [WMSErrorCodes.INVENTORY_SHORTAGE] = "库存不足",
            [WMSErrorCodes.WAVE_NOT_EXIST] = "波次不存在",
            [WMSErrorCodes.WAVE_STATUS_ERROR] = "波次状态不允许该操作",
            [WMSErrorCodes.PICK_TASK_NOT_EXIST] = "拣货任务不存在",
            [WMSErrorCodes.PICK_TASK_STATUS_ERROR] = "拣货任务状态不允许该操作",
            [WMSErrorCodes.PICK_LOCATION_NOT_EXIST] = "拣货库位不存在",
            [WMSErrorCodes.PICK_QTY_ERROR] = "拣货数量异常",
            [WMSErrorCodes.PACK_TASK_NOT_EXIST] = "复核任务不存在",
            [WMSErrorCodes.PACK_QTY_MISMATCH] = "复核数量与订单不符",
            [WMSErrorCodes.SHIPMENT_FAILED] = "发货失败",
            [WMSErrorCodes.TRACKING_NO_ALREADY_EXIST] = "运单号已存在",
            #endregion

            #region 库存管理
            // 库存管理
            [WMSErrorCodes.INVENTORY_NOT_EXIST] = "库存不存在",
            [WMSErrorCodes.INVENTORY_LOCKED] = "库存已被锁定",
            [WMSErrorCodes.INVENTORY_QTY_INSUFFICIENT] = "库存数量不足",
            [WMSErrorCodes.BATCH_NOT_EXIST] = "批次号不存在",
            [WMSErrorCodes.BATCH_EXPIRED] = "批次已过期",
            [WMSErrorCodes.BATCH_EXPIRING_SOON] = "批次即将过期",
            [WMSErrorCodes.SERIAL_NO_ALREADY_EXIST] = "序列号已存在",
            [WMSErrorCodes.SERIAL_NO_NOT_EXIST] = "序列号不存在",
            [WMSErrorCodes.SERIAL_NO_ALREADY_ASSIGNED] = "序列号已分配",
            [WMSErrorCodes.INVENTORY_FREEZE_FAILED] = "库存冻结失败",
            [WMSErrorCodes.INVENTORY_UNFREEZE_FAILED] = "库存解冻失败",
            [WMSErrorCodes.INVENTORY_ADJUST_FAILED] = "库存调整失败",
            #endregion

            #region 库位管理
            // 库位管理
            [WMSErrorCodes.WAREHOUSE_NOT_EXIST] = "仓库不存在",
            [WMSErrorCodes.ZONE_NOT_EXIST] = "库区不存在",
            [WMSErrorCodes.LOCATION_NOT_EXIST] = "库位不存在",
            [WMSErrorCodes.LOCATION_ALREADY_EXIST] = "库位已存在",
            [WMSErrorCodes.LOCATION_OCCUPIED] = "库位已占用",
            [WMSErrorCodes.LOCATION_FULL] = "库位已满",
            [WMSErrorCodes.LOCATION_TYPE_ERROR] = "库位类型错误",
            [WMSErrorCodes.LOCATION_STATUS_ERROR] = "库位状态错误",
            [WMSErrorCodes.LOCATION_CAPACITY_INSUFFICIENT] = "库位容量不足",
            [WMSErrorCodes.LOCATION_WEIGHT_EXCEEDED] = "库位重量超限",
            [WMSErrorCodes.LOCATION_VOLUME_EXCEEDED] = "库位体积超限",
            [WMSErrorCodes.LOCATION_MIXING_RULE_CONFLICT] = "库位混合存储规则冲突",
            [WMSErrorCodes.LOCATION_SKU_LIMIT_CONFLICT] = "库位商品限制冲突",
            #endregion

            #region 盘点管理
            // 盘点管理
            [WMSErrorCodes.COUNTING_NOT_EXIST] = "盘点单不存在",
            [WMSErrorCodes.COUNTING_STATUS_ERROR] = "盘点单状态不允许该操作",
            [WMSErrorCodes.COUNTING_DETAIL_NOT_EXIST] = "盘点明细不存在",
            [WMSErrorCodes.COUNTING_DIFFERENCE_TOO_LARGE] = "盘点差异过大，需主管审核",
            [WMSErrorCodes.COUNTING_TASK_NOT_EXIST] = "盘点任务不存在",
            [WMSErrorCodes.COUNTING_TASK_ALREADY_COMPLETED] = "盘点任务已完成",
            [WMSErrorCodes.COUNTING_ADJUST_FAILED] = "盘点盈亏处理失败",
            [WMSErrorCodes.CYCLE_COUNT_PLAN_NOT_EXIST] = "循环盘点计划不存在",
            [WMSErrorCodes.COUNTING_FREEZE_FAILED] = "盘点冻结失败",
            #endregion

            #region 移库管理
            // 移库管理
            [WMSErrorCodes.TRANSFER_NOT_EXIST] = "移库单不存在",
            [WMSErrorCodes.TRANSFER_STATUS_ERROR] = "移库单状态不允许该操作",
            [WMSErrorCodes.TRANSFER_TASK_NOT_EXIST] = "移库指令不存在",
            [WMSErrorCodes.SOURCE_LOCATION_NOT_EXIST] = "源库位不存在",
            [WMSErrorCodes.TARGET_LOCATION_NOT_EXIST] = "目标库位不存在",
            [WMSErrorCodes.SOURCE_LOCATION_EMPTY] = "源库位无库存",
            [WMSErrorCodes.TARGET_LOCATION_FULL] = "目标库位已满",
            [WMSErrorCodes.TRANSFER_QTY_ERROR] = "移库数量异常",
            [WMSErrorCodes.INVENTORY_MOVE_FAILED] = "库存移动失败",
            #endregion

            #region 物料管理
            // 物料管理
            [WMSErrorCodes.SKU_NOT_EXIST] = "物料不存在",
            [WMSErrorCodes.SKU_ALREADY_EXIST] = "物料已存在",
            [WMSErrorCodes.BARCODE_NOT_EXIST] = "物料条码不存在",
            [WMSErrorCodes.BARCODE_ALREADY_EXIST] = "物料条码已存在",
            [WMSErrorCodes.SKU_MASTER_NOT_EXIST] = "物料主数据不存在",
            [WMSErrorCodes.SKU_CATEGORY_NOT_EXIST] = "物料分类不存在",
            [WMSErrorCodes.SKU_ATTRIBUTE_NOT_EXIST] = "物料属性不存在",
            [WMSErrorCodes.SKU_PACKAGING_NOT_EXIST] = "物料包装不存在",
            [WMSErrorCodes.SKU_STRATEGY_NOT_EXIST] = "物料策略不存在",
            #endregion

            #region 货主管理
            // 货主管理
            [WMSErrorCodes.OWNER_NOT_EXIST] = "货主不存在",
            [WMSErrorCodes.OWNER_ALREADY_EXIST] = "货主已存在",
            [WMSErrorCodes.OWNER_STATUS_ERROR] = "货主状态不允许该操作",
            [WMSErrorCodes.SUPPLIER_NOT_EXIST] = "供应商不存在",
            [WMSErrorCodes.SUPPLIER_ALREADY_EXIST] = "供应商已存在",
            [WMSErrorCodes.CUSTOMER_NOT_EXIST] = "客户不存在",
            [WMSErrorCodes.CUSTOMER_ALREADY_EXIST] = "客户已存在",
            #endregion

            #region 策略管理
            // 策略管理
            [WMSErrorCodes.STRATEGY_NOT_EXIST] = "策略不存在",
            [WMSErrorCodes.STRATEGY_CONFLICT] = "策略冲突",
            [WMSErrorCodes.PUT_AWAY_STRATEGY_NOT_EXIST] = "上架策略不存在",
            [WMSErrorCodes.PICK_STRATEGY_NOT_EXIST] = "拣货策略不存在",
            [WMSErrorCodes.ALLOCATION_STRATEGY_NOT_EXIST] = "分配策略不存在",
            [WMSErrorCodes.WAVE_STRATEGY_NOT_EXIST] = "波次策略不存在",
            [WMSErrorCodes.ROUTE_STRATEGY_NOT_EXIST] = "路径策略不存在",
            [WMSErrorCodes.ABC_STRATEGY_NOT_EXIST] = "ABC策略不存在",
            #endregion

            #region 系统集成
            // 系统集成
            [WMSErrorCodes.ERP_API_FAILED] = "ERP接口调用失败",
            [WMSErrorCodes.TMS_API_FAILED] = "TMS接口调用失败",
            [WMSErrorCodes.INTERFACE_DATA_FORMAT_ERROR] = "接口数据格式错误",
            [WMSErrorCodes.INTERFACE_DATA_VALIDATION_FAILED] = "接口数据验证失败",
            [WMSErrorCodes.INTERFACE_TIMEOUT] = "接口超时",
            [WMSErrorCodes.INTERFACE_CONNECTION_FAILED] = "接口连接失败",
            #endregion

            #region 任务管理
            // 任务管理
            [WMSErrorCodes.TASK_NOT_EXIST] = "任务不存在: {0}",
            [WMSErrorCodes.TASK_STATUS_ERROR] = "任务 {0} 状态为 {1}，只有待执行(PENDING)状态的任务可取消",
            [WMSErrorCodes.TASK_NOT_PUTAWAY] = "上架任务 {0} 尚未分配实际目标库位，请先调用货位分配接口",
            [WMSErrorCodes.TASK_CANCEL_FAILED] = "任务取消处理失败: {0}",
            [WMSErrorCodes.TASK_COMPLETE_FAILED] = "任务完成处理失败: {0}",
            [WMSErrorCodes.TASK_NOT_LINKED_OUTBOUND] = "拣货任务 {0} 未关联出库单",
            [WMSErrorCodes.TASK_PICK_COMPLETE_FAILED] = "拣货完成处理失败",
            [WMSErrorCodes.TASK_LOCATION_NOT_ALLOCATED] = "任务 {0} 不是上架任务，无法分配货位",
            [WMSErrorCodes.TASK_LOCATION_ALLOCATE_FAILED] = "货位分配失败: {0}",
            [WMSErrorCodes.TASK_LOCATION_NO_AVAILABLE] = "库区无可用空闲库位，任务 {0} 无法分配货位",
            [WMSErrorCodes.TASK_LOCATION_STRATEGY_FAILED] = "货位分配策略执行失败（任务 {0}）: {1}",
            [WMSErrorCodes.TASK_AT_LEAST_ONE_ID] = "TaskNo 和 WcsTaskNo 至少提供一个",
            [WMSErrorCodes.TASK_EXCEPTION_OCCURRED] = "任务 {0} 执行异常: {1}",
            #endregion

            #region 组盘管理
            // 组盘管理
            [WMSErrorCodes.CONTAINER_BIND_DATA_EMPTY] = "组盘数据不能为空",
            [WMSErrorCodes.CONTAINER_BIND_DETAIL_NOT_EXIST] = "入库单明细不存在: {0}",
            [WMSErrorCodes.CONTAINER_BIND_FAILED] = "组盘失败: {0}",
            [WMSErrorCodes.CONTAINER_BIND_RECORD_NOT_EXIST] = "组盘记录不存在: {0}",
            [WMSErrorCodes.CONTAINER_BIND_STATUS_ERROR] = "容器 {0} 状态为 {1}，仅已绑定状态可请求上架",
            [WMSErrorCodes.CONTAINER_BIND_PUTAWAY_FAILED] = "请求上架失败: {0}",
            [WMSErrorCodes.CONTAINER_CODE_EMPTY] = "容器编号不能为空",
            [WMSErrorCodes.INBOUND_STATION_CODE_EMPTY] = "入库口编号不能为空",
            [WMSErrorCodes.CONTAINER_NO_BOUND_RECORD] = "容器 {0} 没有处于已组盘状态的记录",
            [WMSErrorCodes.CONTAINER_BIND_WCS_PUTAWAY_FAILED] = "WCS申请上架失败: {0}",
            [WMSErrorCodes.CONTAINER_BIND_SELECT_REQUIRED] = "请选择要上架的组盘记录",
            [WMSErrorCodes.CONTAINER_BIND_NO_WAREHOUSE] = "容器 {0} 未关联仓库，无法上架",
            [WMSErrorCodes.CONTAINER_BIND_PUTAWAY_TASK_FAILED] = "创建上架任务失败（容器 {0}）: {1}",
            #endregion

            #region 分配管理
            // 分配管理
            [WMSErrorCodes.ALLOCATION_NO_DETAIL] = "出库单无明细行",
            [WMSErrorCodes.ALLOCATION_SINGLE_LINE_FAILED] = "单行分配失败",
            [WMSErrorCodes.ALLOCATION_DETAIL_NOT_EXIST] = "库存明细不存在: {0}",
            [WMSErrorCodes.ALLOCATION_INVENTORY_SHORTAGE_DETAIL] = "物料 {0} 库存不足，需求 {1}，可用 {2}",
            [WMSErrorCodes.ALLOCATION_TASK_GENERATE_FAILED] = "生成出库任务失败: {0}",
            [WMSErrorCodes.ALLOCATION_HEAD_NOT_EXIST] = "分配头不存在",
            [WMSErrorCodes.ALLOCATION_NO_NEED_PROCESS] = "任务 {0} 无需处理的分配明细",
            [WMSErrorCodes.ALLOCATION_PICK_COMPLETE_INFO] = "拣货任务 {0} 完成，已处理 {1} 条分配明细",
            [WMSErrorCodes.ALLOCATE_FAILED] = "分配失败: {0}",
            #endregion

            #region 入库/出库操作
            // 入库/出库操作
            [WMSErrorCodes.RECEIVE_DATA_EMPTY] = "收货数据不能为空",
            [WMSErrorCodes.RECEIVE_FAILED] = "收货失败: {0}",
            [WMSErrorCodes.RECEIVE_AND_BIND_FAILED] = "收货并组盘失败: {0}",
            [WMSErrorCodes.RECEIVE_INBOUND_FAILED] = "接收失败: {0}",
            [WMSErrorCodes.OUTBOUND_NO_DETAIL_FOR_CONFIRM] = "出库单无明细行，无法确认",
            [WMSErrorCodes.TASK_STATUS_CANNOT_COMPLETE] = "任务 {0} 状态为 {1}，只有待执行或执行中的任务可完成",
            [WMSErrorCodes.TASK_ALREADY_ALLOCATED] = "任务 {0} 已分配目标库位 {1}，不能重复分配",
            [WMSErrorCodes.TASK_STATUS_CANNOT_ALLOCATE] = "任务 {0} 状态为 {1}，只有待执行或执行中的任务可分配货位",
            #endregion

            #region 配置管理
            // 配置管理
            [WMSErrorCodes.CONFIG_NOT_FOUND] = "配置项不存在: {0}",
            [WMSErrorCodes.CONTAINER_MIXED_BATCH] = "容器 {0} 不允许绑定不同批次的物料，当前配置不允许混批",
            [WMSErrorCodes.CONTAINER_MIXED_MATERIAL] = "容器 {0} 不允许绑定不同物料，当前配置不允许混料",
            [WMSErrorCodes.BATCH_NO_REQUIRED] = "已启用批次管理，批次号不能为空",
            [WMSErrorCodes.EXPIRY_DATE_REQUIRED] = "已启用有效期管理，生产日期或有效期至少填写一项",
            [WMSErrorCodes.OVER_PICK_NOT_ALLOWED] = "不允许拣货数量大于计划数量，计划 {0}，实际 {1}",
            #endregion

            #region 成功消息
            // 成功消息
            [WMSErrorCodes.RECEIVE_SUCCESS] = "接收成功",
            [WMSErrorCodes.INBOUND_RECEIVE_SUCCESS] = "收货成功",
            [WMSErrorCodes.RECEIVE_AND_BIND_SUCCESS] = "收货并组盘成功",
            [WMSErrorCodes.CONTAINER_BIND_CREATED] = "成功创建 {0} 条组盘头记录，{1} 条组盘明细记录",
            [WMSErrorCodes.PUTAWAY_TASK_CREATED] = "成功创建 {0} 条上架任务",
            [WMSErrorCodes.OUTBOUND_CONFIRM_SUCCESS] = "出库单确认成功",
            [WMSErrorCodes.ALLOCATION_SUCCESS] = "分配成功，共 {0} 条分配明细",
            [WMSErrorCodes.OUTBOUND_TASK_CREATED] = "成功生成 {0} 个出库任务",
            [WMSErrorCodes.TASK_COMPLETE_SUCCESS] = "任务 {0} 完成成功",
            [WMSErrorCodes.TASK_CANCEL_SUCCESS] = "任务 {0} 已取消",
            [WMSErrorCodes.TASK_LOCATION_ALLOCATE_SUCCESS] = "货位分配成功: {0}",
            [WMSErrorCodes.PUTAWAY_TASK_CREATED_WITH_NO] = "上架任务创建成功，任务号: {0}",
            #endregion
        };

        /// <summary>
        /// 获取错误消息
        /// </summary>
        public static string GetMessage(string errorCode)
        {
            return _messages.TryGetValue(errorCode, out var message) ? message : "未知错误";
        }

        /// <summary>
        /// 获取错误消息（带参数）
        /// </summary>
        public static string GetMessage(string errorCode, params object[] args)
        {
            var message = GetMessage(errorCode);
            return args.Length > 0 ? string.Format(message, args) : message;
        }
    }

}
