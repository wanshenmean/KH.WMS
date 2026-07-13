using SqlSugar;

namespace KH.WMS.Algorithms.Strategy.DTOs
{
    /// <summary>
    /// 物料周转分类DTO
    /// 映射 md_material_turnover 表
    /// </summary>
    [SugarTable("md_material_turnover")]
    public class MdMaterialTurnoverDTO
    {
        public long Id { get; set; }

        /// <summary>物料ID</summary>
        public long MaterialId { get; set; }

        /// <summary>分类编码（A/B/C）</summary>
        public string ClassCode { get; set; } = string.Empty;

        /// <summary>分析周期（如 2026-Q1、2026-04）</summary>
        public string Period { get; set; } = string.Empty;

        /// <summary>出库次数</summary>
        public int OutboundCount { get; set; }

        /// <summary>出库数量</summary>
        public decimal OutboundQty { get; set; }

        /// <summary>累计占比（%）</summary>
        public decimal CumulativeRatio { get; set; }

        /// <summary>计算时间</summary>
        public DateTime CalculatedAt { get; set; }
    }
}
