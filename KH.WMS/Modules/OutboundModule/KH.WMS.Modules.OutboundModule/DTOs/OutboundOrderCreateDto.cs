using KH.WMS.Entities.Outbound;

namespace KH.WMS.Modules.OutboundModule.DTOs
{
    public class OutboundOrderCreateDto
    {
        public OutboundOrder Order { get; set; } = new();
        public List<OutboundOrderLine> Lines { get; set; } = new();

        /// <summary>
        /// 前端提交的扩展字段原始数据
        /// </summary>
        public Dictionary<string, object?>? ExtDataRaw { get; set; }

        /// <summary>
        /// 前端提交的行级扩展字段
        /// </summary>
        public Dictionary<string, Dictionary<string, object?>?>? LineExtDataRaw { get; set; }

        /// <summary>
        /// 查询结果：反序列化后的单据头扩展字段
        /// </summary>
        public Dictionary<string, object?>? ExtDataFlattened { get; set; }

        /// <summary>
        /// 查询结果：反序列化后的行级扩展字段
        /// </summary>
        public Dictionary<long, Dictionary<string, object?>>? LineExtDataFlattened { get; set; }
    }
}
