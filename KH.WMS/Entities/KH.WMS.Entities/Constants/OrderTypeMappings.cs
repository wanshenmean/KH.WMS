namespace KH.WMS.Entities.Constants;

/// <summary>
/// 入库订单类型（OrderType，如 PURCHASE_IN）到单据类型码
/// （CfgDocumentType.TypeCode，如 INBOUND_ORDER_PURCHASE）的集中映射。
/// 作为该映射的单一真相源，替代各 Service 中散落重复的 GetDocTypeCode switch。
/// 新增入库类型时，除在 <see cref="BizConstants.OrderTypes"/> 与 <see cref="BizConstants.DocTypes"/>
/// 增加常量、并在 cfg_document_type 补充状态配置外，仅需在此补充一条映射。
/// </summary>
public static class OrderTypeMappings
{
    /// <summary>
    /// 严格映射，用于写入/校验路径（创建、收货、组盘、状态推进与回退等）。
    /// 未识别的类型（null、空串、OTHER_INBOUND 或任何未知值）抛出 <see cref="ArgumentException"/>，
    /// 以替代历史 switch 的 default 分支将未知类型静默降级为采购入库的危险行为——
    /// 后者会把错误数据写入库存并按采购流程持续运转而不报错。
    /// </summary>
    public static string ToDocTypeCode(string? orderType) => orderType switch
    {
        BizConstants.OrderTypes.PURCHASE_IN => BizConstants.DocTypes.INBOUND_ORDER_PURCHASE,
        BizConstants.OrderTypes.PRODUCTION_IN => BizConstants.DocTypes.INBOUND_ORDER_PRODUCTION,
        BizConstants.OrderTypes.RETURN_INBOUND => BizConstants.DocTypes.INBOUND_ORDER_RETURN,
        _ => throw new ArgumentException(
            $"无法识别的入库订单类型 \"{orderType}\"，未配置对应的单据类型码。已知类型：" +
            $"{BizConstants.OrderTypes.PURCHASE_IN}(采购入库)、" +
            $"{BizConstants.OrderTypes.PRODUCTION_IN}(生产入库)、" +
            $"{BizConstants.OrderTypes.RETURN_INBOUND}(退货入库)。",
            nameof(orderType)),
    };

    /// <summary>
    /// 宽松映射，用于列表查询等只读展示路径。
    /// 未识别的类型返回 null，调用方据此跳过该项，避免单条历史脏数据拖垮整个查询。
    /// 写入/校验路径必须使用严格版 <see cref="ToDocTypeCode"/>。
    /// </summary>
    public static string? TryToDocTypeCode(string? orderType) => orderType switch
    {
        BizConstants.OrderTypes.PURCHASE_IN => BizConstants.DocTypes.INBOUND_ORDER_PURCHASE,
        BizConstants.OrderTypes.PRODUCTION_IN => BizConstants.DocTypes.INBOUND_ORDER_PRODUCTION,
        BizConstants.OrderTypes.RETURN_INBOUND => BizConstants.DocTypes.INBOUND_ORDER_RETURN,
        _ => null,
    };
}
