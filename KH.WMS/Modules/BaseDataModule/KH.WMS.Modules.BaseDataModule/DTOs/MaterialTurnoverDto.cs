namespace KH.WMS.Modules.BaseDataModule.DTOs
{
    /// <summary>
    /// ABC分类计算请求
    /// </summary>
    public class TurnoverCalculateRequest
    {
        /// <summary>
        /// 分析周期（如 2026-Q1、2026-04）
        /// </summary>
        public string Period { get; set; } = string.Empty;

        /// <summary>
        /// 分析维度（OUTBOUND_QTY-出库数量 / OUTBOUND_FREQ-出库频次）
        /// </summary>
        public string AnalysisDimension { get; set; } = "OUTBOUND_QTY";

        /// <summary>
        /// 分析起始日期
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// 分析截止日期
        /// </summary>
        public DateTime EndDate { get; set; }
    }

    /// <summary>
    /// ABC分类结果查询返回
    /// </summary>
    public class MaterialTurnoverDto
    {
        public long Id { get; set; }

        /// <summary>
        /// 物料ID
        /// </summary>
        public long MaterialId { get; set; }

        /// <summary>
        /// 物料编码
        /// </summary>
        public string MaterialCode { get; set; } = string.Empty;

        /// <summary>
        /// 物料名称
        /// </summary>
        public string MaterialName { get; set; } = string.Empty;

        /// <summary>
        /// 分类编码（A/B/C）
        /// </summary>
        public string ClassCode { get; set; } = string.Empty;

        /// <summary>
        /// 分类名称
        /// </summary>
        public string ClassName { get; set; } = string.Empty;

        /// <summary>
        /// 分析周期
        /// </summary>
        public string Period { get; set; } = string.Empty;

        /// <summary>
        /// 出库次数
        /// </summary>
        public int OutboundCount { get; set; }

        /// <summary>
        /// 出库数量
        /// </summary>
        public decimal OutboundQty { get; set; }

        /// <summary>
        /// 累计占比（%）
        /// </summary>
        public decimal CumulativeRatio { get; set; }

        /// <summary>
        /// 计算时间
        /// </summary>
        public DateTime CalculatedAt { get; set; }
    }
}
