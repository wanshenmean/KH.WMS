using KH.WMS.Core.DependencyInjection.ServiceLifetimes;
using KH.WMS.Core.Validation;
using KH.WMS.Entities.Constants;
using KH.WMS.Entities.Inbound;
using KH.WMS.Modules.InboundModule.DTOs;
using SqlSugar;

namespace KH.WMS.Modules.InboundModule.Validation;

/// <summary>
/// 组盘数量校验器
/// 校验每组盘数量不超过订单行可组盘数量（已收货 - 已组盘）
/// </summary>
[RegisteredService(WithoutInterceptor = true, ServiceType = typeof(IValidator))]
public class BindQuantityValidator : IValidator
{
    private readonly ISqlSugarClient _db;

    public BindQuantityValidator(ISqlSugarClient db)
    {
        _db = db;
    }

    public string Code => ValidatorCodes.BIND_QUANTITY;

    public async Task<string?> ValidateAsync(object?[] args, Dictionary<string, object>? services = null)
    {
        var binds = args.OfType<List<ContainerBindDto>>().FirstOrDefault();
        if (binds == null || binds.Count == 0)
            return null;

        var lineIds = binds.Select(b => b.InboundOrderLineId).Distinct().ToList();
        var lines = await _db.Queryable<InboundOrderLine>()
            .Where(l => lineIds.Contains(l.Id)).ToListAsync();

            // 查询各明细行所属订单类型，确定组盘基准量（需收货用 ReceivedQty，否则用 OrderedQty）
            var orderIds = lines.Select(l => l.OrderId).Distinct().ToList();
            var orderTypeDict = (await _db.Queryable<InboundOrder>()
                    .Where(o => orderIds.Contains(o.Id))
                    .Select(o => new { o.Id, o.OrderType })
                    .ToListAsync())
                .ToDictionary(o => o.Id, o => o.OrderType);

        foreach (var bind in binds)
        {
            var line = lines.First(l => l.Id == bind.InboundOrderLineId);
            var needReceive = BizConstants.OrderTypes.IsReceiveRequired(orderTypeDict[line.OrderId]);
            var baseQty = needReceive ? line.ReceivedQty : (line.OrderedQty ?? 0);

            // 查询该订单行已组盘数量
            var boundQty = await _db.Queryable<InboundContainerBindDetail>()
                .InnerJoin<InboundContainerBindHeader>((d, h) => d.HeaderId == h.Id)
                .Where((d, h) => d.InboundOrderLineId == bind.InboundOrderLineId
                    && h.BindStatus == BizConstants.BindStatus.BOUND)
                .SumAsync(d => d.Qty);

            var validation = line.ValidateBindQty(bind.Qty, boundQty, baseQty);
            if (!validation.IsValid)
                return validation.Errors.First();
        }
        return null;
    }
}
