using System.ComponentModel.DataAnnotations;

namespace KH.WMS.Modules.InboundModule.DTOs
{
    /// <summary>
    /// WCS申请上架请求参数
    /// </summary>
    public class WcsPutAwayRequestDto
    {
        /// <summary>
        /// 容器编号（托盘条码，由输送线扫码获取）
        /// </summary>
        
        public string ContainerCode { get; set; } = string.Empty;

        /// <summary>
        /// 入库口编号（输送线入库口编码，由WCS传入）
        /// </summary>
        
        public string PortCode { get; set; } = string.Empty;
    }
}
