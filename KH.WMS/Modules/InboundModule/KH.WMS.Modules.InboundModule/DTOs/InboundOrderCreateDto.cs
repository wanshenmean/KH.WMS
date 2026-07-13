using KH.WMS.Entities.Inbound;

namespace KH.WMS.Modules.InboundModule.DTOs
{
    public class InboundOrderCreateDto
    {
        public InboundOrder Order { get; set; } = new();
        public List<InboundOrderLine> Lines { get; set; } = new();

        /// <summary>
        /// 前端提交的扩展字段原始数据（由 Controller 提取后序列化为 ExtData JSON）
        /// </summary>
        public Dictionary<string, object?>? ExtDataRaw { get; set; }

        /// <summary>
        /// 前端提交的行级扩展字段（行索引 → 字段键值对）
        /// </summary>
        public Dictionary<string, Dictionary<string, object?>?>? LineExtDataRaw { get; set; }

        /// <summary>
        /// 查询结果：反序列化后的单据头扩展字段
        /// </summary>
        public Dictionary<string, object?>? ExtDataFlattened { get; set; }

        /// <summary>
        /// 查询结果：反序列化后的行级扩展字段（行Id → 字段键值对）
        /// </summary>
        public Dictionary<long, Dictionary<string, object?>>? LineExtDataFlattened { get; set; }
    }
}
