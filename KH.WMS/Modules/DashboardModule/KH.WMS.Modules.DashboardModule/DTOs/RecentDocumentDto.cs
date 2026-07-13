namespace KH.WMS.Modules.DashboardModule.DTOs;

/// <summary>
/// 最近完成的出入库单据
/// </summary>
public class RecentDocumentDto
{
    /// <summary>
    /// 单据类别（INBOUND-入库 / OUTBOUND-出库）
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// 单据编号
    /// </summary>
    public string OrderNo { get; set; } = string.Empty;

    /// <summary>
    /// 单据类型
    /// </summary>
    public string OrderType { get; set; } = string.Empty;

    /// <summary>
    /// 总行数
    /// </summary>
    public int TotalLines { get; set; }

    /// <summary>
    /// 完成时间（LastModifiedTime ?? CreatedTime）
    /// </summary>
    public DateTime CompletedTime { get; set; }
}
