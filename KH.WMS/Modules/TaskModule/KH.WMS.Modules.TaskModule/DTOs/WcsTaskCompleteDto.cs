using System.ComponentModel.DataAnnotations;

namespace KH.WMS.Modules.TaskModule.DTOs
{
    /// <summary>
    /// WCS任务完成回调请求参数
    /// </summary>
    public class WcsTaskCompleteDto
    {
        /// <summary>
        /// WMS任务编号（与WcsTaskNo二选一）
        /// </summary>
        public string? TaskNo { get; set; }

        /// <summary>
        /// WCS任务编号（与TaskNo二选一）
        /// </summary>
        public string? WcsTaskNo { get; set; }

        /// <summary>
        /// 执行设备编码（堆垛机/AGV/输送线）
        /// </summary>
        public string? EquipmentCode { get; set; }

        /// <summary>
        /// WCS实际到达位置
        /// </summary>
        public string? ActualPosition { get; set; }

        /// <summary>
        /// WCS行走时间(秒)
        /// </summary>
        public int? TravelTime { get; set; }

        /// <summary>
        /// WCS称重
        /// </summary>
        public decimal? Weight { get; set; }

        /// <summary>
        /// 是否异常
        /// </summary>
        public bool IsException { get; set; }

        /// <summary>
        /// 异常原因
        /// </summary>
        public string? ExceptionReason { get; set; }
    }
}
