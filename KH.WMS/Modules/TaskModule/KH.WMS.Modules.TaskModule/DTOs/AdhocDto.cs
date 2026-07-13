namespace KH.WMS.Modules.TaskModule.DTOs;

/// <summary>
/// 无单据组盘再入库请求（组盘 → 创建上架任务，不创建入库单）
/// </summary>
public class AdhocInboundRequest
{
    /// <summary>
    /// 仓库ID
    /// </summary>
    public long WarehouseId { get; set; }

    /// <summary>
    /// 容器编号
    /// </summary>
    public string ContainerCode { get; set; } = string.Empty;

    /// <summary>
    /// 组盘物料行
    /// </summary>
    public List<AdhocInboundLineRequest> Lines { get; set; } = [];
}

/// <summary>
/// 无单据入库行请求
/// </summary>
public class AdhocInboundLineRequest
{
    /// <summary>
    /// 物料ID
    /// </summary>
    public long MaterialId { get; set; }

    /// <summary>
    /// 物料编码
    /// </summary>
    public string? MaterialCode { get; set; }

    /// <summary>
    /// 物料名称
    /// </summary>
    public string? MaterialName { get; set; }

    /// <summary>
    /// 数量
    /// </summary>
    public decimal Qty { get; set; }

    /// <summary>
    /// 批次号
    /// </summary>
    public string? BatchNo { get; set; }

    /// <summary>
    /// 生产日期
    /// </summary>
    public DateOnly? ProductionDate { get; set; }

    /// <summary>
    /// 过期日期
    /// </summary>
    public DateOnly? ExpiryDate { get; set; }
}

/// <summary>
/// 无单据出库请求（按库存明细ID列表）
/// </summary>
public class AdhocOutboundRequest
{
    /// <summary>
    /// 仓库ID
    /// </summary>
    public long WarehouseId { get; set; }

    /// <summary>
    /// 容器编号（可选）
    /// </summary>
    public string? ContainerCode { get; set; }

    /// <summary>
    /// 起始库位ID（可选）
    /// </summary>
    public long? FromLocationId { get; set; }

    /// <summary>
    /// 起始库位编码（可选）
    /// </summary>
    public string? FromLocationCode { get; set; }

    /// <summary>
    /// 出库物料行（库存明细ID + 数量）
    /// </summary>
    public List<AdhocOutboundLineRequest> Lines { get; set; } = [];
}

/// <summary>
/// 无单据出库行请求
/// </summary>
public class AdhocOutboundLineRequest
{
    /// <summary>
    /// 库存明细ID
    /// </summary>
    public long InventoryDetailId { get; set; }

    /// <summary>
    /// 出库数量
    /// </summary>
    public decimal Qty { get; set; }
}

/// <summary>
/// 指定托盘号出库请求
/// </summary>
public class AdhocOutboundByContainerRequest
{
    /// <summary>
    /// 仓库ID
    /// </summary>
    public long WarehouseId { get; set; }

    /// <summary>
    /// 容器编号
    /// </summary>
    public string ContainerCode { get; set; } = string.Empty;

    /// <summary>
    /// 是否全部出库
    /// </summary>
    public bool AllOutbound { get; set; } = false;

    /// <summary>
    /// 选择出库的物料行（AllOutbound=false 时使用）
    /// </summary>
    public List<AdhocOutboundLineRequest>? SelectedLines { get; set; }
}

/// <summary>
/// 指定货位出库请求
/// </summary>
public class AdhocOutboundByLocationRequest
{
    /// <summary>
    /// 仓库ID
    /// </summary>
    public long WarehouseId { get; set; }

    /// <summary>
    /// 货位编码
    /// </summary>
    public string LocationCode { get; set; } = string.Empty;

    /// <summary>
    /// 是否全部出库
    /// </summary>
    public bool AllOutbound { get; set; } = false;

    /// <summary>
    /// 选择出库的物料行（AllOutbound=false 时使用）
    /// </summary>
    public List<AdhocOutboundLineRequest>? SelectedLines { get; set; }
}

/// <summary>
/// 起始地址→目的地址上架请求
/// </summary>
public class AdhocPutawayFromToRequest : IAdhocPutawayRequest
{
    /// <summary>
    /// 仓库ID
    /// </summary>
    public long WarehouseId { get; set; }

    /// <summary>
    /// 容器编号（可选）
    /// </summary>
    public string? ContainerCode { get; set; }

    /// <summary>
    /// 起始位置编码
    /// </summary>
    public string FromLocationCode { get; set; } = string.Empty;

    /// <summary>
    /// 目的位置编码
    /// </summary>
    public string ToLocationCode { get; set; } = string.Empty;

    /// <summary>
    /// 物料行（用于任务完成后生成库存）
    /// </summary>
    public List<AdhocInboundLineRequest>? Lines { get; set; }
}

/// <summary>
/// 直接上架到指定地址请求
/// </summary>
public class AdhocPutawayToRequest : IAdhocPutawayRequest
{
    /// <summary>
    /// 仓库ID
    /// </summary>
    public long WarehouseId { get; set; }

    /// <summary>
    /// 容器编号（可选）
    /// </summary>
    public string? ContainerCode { get; set; }

    /// <summary>
    /// 目的位置编码
    /// </summary>
    public string ToLocationCode { get; set; } = string.Empty;

    /// <summary>
    /// 起始位置编码（未使用，接口要求）
    /// </summary>
    string? IAdhocPutawayRequest.FromLocationCode => null;

    /// <summary>
    /// 物料行（用于任务完成后生成库存）
    /// </summary>
    public List<AdhocInboundLineRequest>? Lines { get; set; }
}

/// <summary>
/// 无单据上架请求公共接口
/// </summary>
internal interface IAdhocPutawayRequest
{
    long WarehouseId { get; }
    string? ContainerCode { get; }
    string? FromLocationCode { get; }
    List<AdhocInboundLineRequest>? Lines { get; }
}

// ==================== 新增 DTO ====================

/// <summary>
/// 无单据库存查询请求（用于出库页筛选）
/// </summary>
public class AdhocInventoryQueryRequest
{
    /// <summary>
    /// 仓库ID（必选）
    /// </summary>
    public long WarehouseId { get; set; }

    /// <summary>
    /// 容器编号（可选）
    /// </summary>
    public string? ContainerCode { get; set; }

    /// <summary>
    /// 货位编码（可选）
    /// </summary>
    public string? LocationCode { get; set; }

    /// <summary>
    /// 库区编码（可选）
    /// </summary>
    public string? ZoneCode { get; set; }

    /// <summary>
    /// 巷道编码（可选）
    /// </summary>
    public string? AisleCode { get; set; }

    /// <summary>
    /// 出库口编码（可选）
    /// </summary>
    public string? PortCode { get; set; }

    /// <summary>
    /// 物料编码（可选，模糊搜索）
    /// </summary>
    public string? MaterialCode { get; set; }

    /// <summary>
    /// 物料ID（可选）
    /// </summary>
    public long? MaterialId { get; set; }
}

/// <summary>
/// 按库区出库请求
/// </summary>
public class AdhocOutboundByZoneRequest
{
    public long WarehouseId { get; set; }
    public string ZoneCode { get; set; } = string.Empty;
    public bool AllOutbound { get; set; } = false;
    public List<AdhocOutboundLineRequest>? SelectedLines { get; set; }
}

/// <summary>
/// 按巷道出库请求
/// </summary>
public class AdhocOutboundByAisleRequest
{
    public long WarehouseId { get; set; }
    public string AisleCode { get; set; } = string.Empty;
    public bool AllOutbound { get; set; } = false;
    public List<AdhocOutboundLineRequest>? SelectedLines { get; set; }
}

/// <summary>
/// 按出库口出库请求
/// </summary>
public class AdhocOutboundByPortRequest
{
    public long WarehouseId { get; set; }
    public string PortCode { get; set; } = string.Empty;
    public bool AllOutbound { get; set; } = false;
    public List<AdhocOutboundLineRequest>? SelectedLines { get; set; }
}

/// <summary>
/// 路线校验请求（起点能否到达目标）
/// </summary>
public class AdhocRouteCheckRequest
{
    public long WarehouseId { get; set; }
    public string FromLocationCode { get; set; } = string.Empty;
    public string ToLocationCode { get; set; } = string.Empty;
}

/// <summary>
/// 路线校验结果
/// </summary>
public class AdhocRouteCheckResult
{
    public bool Reachable { get; set; }
    public string Message { get; set; } = string.Empty;
}
