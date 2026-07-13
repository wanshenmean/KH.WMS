using SqlSugar;

namespace KH.WMS.Algorithms.Strategy.DTOs
{
    /// <summary>
    /// 单据类型站台映射DTO
    /// 映射 cfg_doc_type_port 表
    /// </summary>
    [SugarTable("cfg_doc_type_port")]
    public class CfgDocTypePortDTO
    {
        public long Id { get; set; }

        /// <summary>单据类型ID</summary>
        public long DocTypeId { get; set; }

        /// <summary>方向（INBOUND-入库 / OUTBOUND-出库）</summary>
        public string Direction { get; set; } = string.Empty;

        /// <summary>站台ID（与ZoneId二选一）</summary>
        public long? PortId { get; set; }

        /// <summary>库区ID（与PortId二选一）</summary>
        public long? ZoneId { get; set; }

        /// <summary>优先级（数字越小优先级越高）</summary>
        public int Priority { get; set; } = 100;

        /// <summary>是否启用</summary>
        public byte IsActive { get; set; } = 1;

        /// <summary>备注</summary>
        public string? Remark { get; set; }
    }
}
