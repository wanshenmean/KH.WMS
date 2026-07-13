using KH.WMS.Algorithms.Strategy.Constants;
using KH.WMS.Algorithms.Strategy.DTOs;
using KH.WMS.Core.DependencyInjection.ServiceLifetimes;
using SqlSugar;

namespace KH.WMS.Algorithms.Strategy.QueryServices
{
    /// <summary>
    /// 仓库查询服务实现
    /// </summary>
    [RegisteredService(ServiceType = typeof(IWarehouseQueryService))]
    public class WarehouseQueryService : IWarehouseQueryService
    {
        private readonly ISqlSugarClient _db;

        public WarehouseQueryService(ISqlSugarClient db)
        {
            _db = db;
        }

        /// <summary>
        /// 获取物料ABC周转分类（取最近一个分析周期的结果）
        /// </summary>
        public async Task<string?> GetMaterialTurnoverClassAsync(long materialId,
            CancellationToken cancellationToken = default)
        {
            var turnover = await _db.Queryable<MdMaterialTurnoverDTO>()
                .Where(t => t.MaterialId == materialId)
                .OrderByDescending(t => t.CalculatedAt)
                .FirstAsync(cancellationToken);

            return turnover?.ClassCode;
        }

        /// <summary>
        /// 根据 ABC 分类获取匹配的库区列表（#1 修复：真正按库区 AbcClass 过滤）。
        /// 仅返回存储类库区；若没有任何库区配置 AbcClass，回退返回全部存储类库区（避免无库区可选）。
        /// 货位级 AbcClass 更细粒度，可由调用方在货位层面进一步过滤（loc.AbcClass ?? 所属库区 AbcClass）。
        /// </summary>
        public async Task<List<MdWarehouseZoneDTO>> GetZonesByAbcClassAsync(long warehouseId, string abcClass,
            CancellationToken cancellationToken = default)
        {
            var zones = await GetStorageZonesAsync(warehouseId, cancellationToken);
            if (string.IsNullOrWhiteSpace(abcClass))
                return zones;

            var matched = zones
                .Where(z => !string.IsNullOrWhiteSpace(z.AbcClass)
                            && string.Equals(z.AbcClass, abcClass, StringComparison.OrdinalIgnoreCase))
                .ToList();
            return matched.Count > 0 ? matched : zones;
        }

        /// <summary>
        /// 获取库区下可用巷道列表
        /// </summary>
        public async Task<List<MdAisleDTO>> GetAislesByZoneAsync(long zoneId,
            CancellationToken cancellationToken = default)
        {
            return await _db.Queryable<MdAisleDTO>()
                .Where(a => a.ZoneId == zoneId && a.Status == AlgoConstants.Status.ENABLED)
                .OrderBy(a => a.SortNo)
                .OrderBy(a => a.AisleNo)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// 获取巷道负载
        /// </summary>
        public async Task<(int Total, int Occupied)> GetAisleLoadAsync(int aisleNo, long warehouseId,
            CancellationToken cancellationToken = default)
        {
            var total = await _db.Queryable<MdLocationDTO>()
                .Where(l => l.AisleNo == aisleNo && l.WarehouseId == warehouseId && l.IsDisabled == AlgoConstants.BoolFlag.NO)
                .CountAsync(cancellationToken);

            var occupied = await _db.Queryable<MdLocationDTO>()
                .Where(l => l.AisleNo == aisleNo && l.WarehouseId == warehouseId
                            && l.Status == AlgoConstants.LocationStatus.OCCUPIED && l.IsDisabled == AlgoConstants.BoolFlag.NO)
                .CountAsync(cancellationToken);

            return (total, occupied);
        }

        /// <summary>
        /// 获取负载最低的巷道（占用率最低）。
        /// #12 修复：一次性按巷道聚合货位 总数/占用数，避免逐巷道 2N 次 COUNT 查询。
        /// </summary>
        public async Task<MdAisleDTO?> GetLeastLoadedAisleAsync(long warehouseId, long? zoneId = null,
            CancellationToken cancellationToken = default)
        {
            var aisles = await _db.Queryable<MdAisleDTO>()
                .Where(a => a.WarehouseId == warehouseId && a.Status == AlgoConstants.Status.ENABLED)
                .WhereIF(zoneId.HasValue, a => a.ZoneId == zoneId)
                .OrderBy(a => a.SortNo)
                .ToListAsync(cancellationToken);

            if (aisles.Count == 0) return null;

            var aisleNos = aisles.Select(a => a.AisleNo).ToList();

            var totals = await _db.Queryable<MdLocationDTO>()
                .Where(l => l.WarehouseId == warehouseId && aisleNos.Contains(l.AisleNo) && l.IsDisabled == AlgoConstants.BoolFlag.NO)
                .GroupBy(l => l.AisleNo)
                .Select(l => new { AisleNo = l.AisleNo, Total = SqlFunc.AggregateCount(l.Id) })
                .ToListAsync(cancellationToken);

            var occupied = await _db.Queryable<MdLocationDTO>()
                .Where(l => l.WarehouseId == warehouseId && aisleNos.Contains(l.AisleNo)
                            && l.Status == AlgoConstants.LocationStatus.OCCUPIED && l.IsDisabled == AlgoConstants.BoolFlag.NO)
                .GroupBy(l => l.AisleNo)
                .Select(l => new { AisleNo = l.AisleNo, Occupied = SqlFunc.AggregateCount(l.Id) })
                .ToListAsync(cancellationToken);

            var totalDict = totals.ToDictionary(x => x.AisleNo, x => x.Total);
            var occupiedDict = occupied.ToDictionary(x => x.AisleNo, x => x.Occupied);

            MdAisleDTO? bestAisle = null;
            decimal bestLoadRate = decimal.MaxValue;

            foreach (var aisle in aisles)
            {
                var total = totalDict.GetValueOrDefault(aisle.AisleNo);
                var occ = occupiedDict.GetValueOrDefault(aisle.AisleNo);
                var loadRate = total > 0 ? (decimal)occ / total : 0;

                if (loadRate < bestLoadRate)
                {
                    bestLoadRate = loadRate;
                    bestAisle = aisle;
                }
            }

            return bestAisle;
        }

        /// <summary>
        /// 根据单据类型获取推荐站台
        /// 通过 CfgDocTypePort 配置匹配，按优先级排序
        /// </summary>
        public async Task<MdPortDTO?> GetPortByDocTypeAsync(long docTypeId, string direction, long warehouseId,
            CancellationToken cancellationToken = default)
        {
            var mapping = await _db.Queryable<CfgDocTypePortDTO>()
                .Where(m => m.DocTypeId == docTypeId
                            && m.Direction == direction
                            && m.IsActive == AlgoConstants.Status.ENABLED)
                .OrderBy(m => m.Priority)
                .FirstAsync(cancellationToken);

            if (mapping == null) return null;

            if (mapping.PortId.HasValue)
            {
                return await _db.Queryable<MdPortDTO>()
                    .Where(p => p.Id == mapping.PortId.Value
                                && p.WarehouseId == warehouseId
                                && p.Status == AlgoConstants.Status.ENABLED)
                    .FirstAsync(cancellationToken);
            }

            if (mapping.ZoneId.HasValue)
            {
                return await _db.Queryable<MdPortDTO>()
                    .Where(p => p.ZoneId == mapping.ZoneId.Value
                                && p.WarehouseId == warehouseId
                                && p.Status == AlgoConstants.Status.ENABLED)
                    .FirstAsync(cancellationToken);
            }

            return null;
        }

        /// <summary>
        /// 获取仓库下所有启用的库区
        /// </summary>
        public async Task<List<MdWarehouseZoneDTO>> GetZonesByWarehouseAsync(long warehouseId,
            CancellationToken cancellationToken = default)
        {
            return await _db.Queryable<MdWarehouseZoneDTO>()
                .Where(z => z.WarehouseId == warehouseId && z.Status == AlgoConstants.Status.ENABLED)
                .OrderBy(z => z.SortNo)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// 获取仓库下可用于上架的存储类库区
        /// </summary>
        public async Task<List<MdWarehouseZoneDTO>> GetStorageZonesAsync(long warehouseId,
            CancellationToken cancellationToken = default)
        {
            var zones = await _db.Queryable<MdWarehouseZoneDTO>()
                .Where(z => z.WarehouseId == warehouseId && z.Status == AlgoConstants.Status.ENABLED)
                .OrderBy(z => z.SortNo)
                .ToListAsync(cancellationToken);

            return zones.Where(z => AlgoConstants.ZoneType.IsStorageZone(z.ZoneType)).ToList();
        }

        /// <summary>
        /// 根据巷道ID获取入库接驳口（优先 INBOUND 类型，其次 MIXED）
        /// </summary>
        public async Task<MdTransferPointDTO?> GetInboundTransferPointAsync(long warehouseId, long aisleId,
            CancellationToken cancellationToken = default)
        {
            var point = await _db.Queryable<MdTransferPointDTO>()
                .Where(p => p.WarehouseId == warehouseId
                            && p.AisleId == aisleId
                            && p.Status == AlgoConstants.Status.ENABLED
                            && p.PointType == AlgoConstants.TransferPointType.INBOUND)
                .FirstAsync(cancellationToken);

            if (point != null) return point;

            return await _db.Queryable<MdTransferPointDTO>()
                .Where(p => p.WarehouseId == warehouseId
                            && p.AisleId == aisleId
                            && p.Status == AlgoConstants.Status.ENABLED
                            && p.PointType == AlgoConstants.TransferPointType.MIXED)
                .FirstAsync(cancellationToken);
        }

        /// <summary>
        /// 根据输送线ID获取入库口（优先 INBOUND 类型，其次 MIXED）
        /// </summary>
        public async Task<MdPortDTO?> GetInboundPortByConveyorAsync(long warehouseId, long conveyorLineId,
            CancellationToken cancellationToken = default)
        {
            var port = await _db.Queryable<MdPortDTO>()
                .Where(p => p.WarehouseId == warehouseId
                            && p.ConveyorLineId == conveyorLineId
                            && p.Status == AlgoConstants.Status.ENABLED
                            && p.PortType == AlgoConstants.PortType.INBOUND)
                .FirstAsync(cancellationToken);

            if (port != null) return port;

            return await _db.Queryable<MdPortDTO>()
                .Where(p => p.WarehouseId == warehouseId
                            && p.ConveyorLineId == conveyorLineId
                            && p.Status == AlgoConstants.Status.ENABLED
                            && p.PortType == AlgoConstants.PortType.MIXED)
                .FirstAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<MdPortDTO?> GetOutboundPortByAisleAsync(long warehouseId, int aisleNo,
            CancellationToken cancellationToken = default)
        {
            // 1. 通过巷道号查找巷道记录
            var aisle = await _db.Queryable<MdAisleDTO>()
                .Where(a => a.WarehouseId == warehouseId
                            && a.AisleNo == aisleNo
                            && a.Status == AlgoConstants.Status.ENABLED)
                .FirstAsync(cancellationToken);

            if (aisle == null) return null;

            // 2. 通过巷道ID查找接驳口 → 获取输送线ID
            var transferPoint = await _db.Queryable<MdTransferPointDTO>()
                .Where(tp => tp.WarehouseId == warehouseId
                             && tp.AisleId == aisle.Id
                             && tp.Status == AlgoConstants.Status.ENABLED)
                .FirstAsync(cancellationToken);

            if (transferPoint == null) return null;

            // 3. 通过输送线ID查找该线上的出库口（优先 OUTBOUND，其次 MIXED）
            var port = await _db.Queryable<MdPortDTO>()
                .Where(p => p.WarehouseId == warehouseId
                            && p.ConveyorLineId == transferPoint.ConveyorLineId
                            && p.Status == AlgoConstants.Status.ENABLED
                            && p.PortType == AlgoConstants.PortType.OUTBOUND)
                .FirstAsync(cancellationToken);

            if (port != null) return port;

            return await _db.Queryable<MdPortDTO>()
                .Where(p => p.WarehouseId == warehouseId
                            && p.ConveyorLineId == transferPoint.ConveyorLineId
                            && p.Status == AlgoConstants.Status.ENABLED
                            && p.PortType == AlgoConstants.PortType.MIXED)
                .FirstAsync(cancellationToken);
        }

        /// <summary>
        /// 获取仓库下指定逻辑分区类型及其物理库区映射
        /// </summary>
        public async Task<List<LogicalZoneDTO>> GetLogicalZonesAsync(long warehouseId, string? zoneType = null,
            CancellationToken cancellationToken = default)
        {
            var logicalZones = await _db.Queryable<MdLogicalZoneDTO>()
                .Where(lz => lz.WarehouseId == warehouseId && lz.Status == AlgoConstants.Status.ENABLED)
                .WhereIF(!string.IsNullOrWhiteSpace(zoneType), lz => lz.ZoneType == zoneType)
                .OrderBy(lz => lz.SortNo)
                .ToListAsync(cancellationToken);

            if (logicalZones.Count == 0) return new List<LogicalZoneDTO>();

            var result = new List<LogicalZoneDTO>();
            foreach (var lz in logicalZones)
            {
                var mappings = await _db.Queryable<MdLogicalZoneMappingDTO>()
                    .LeftJoin<MdWarehouseZoneDTO>((m, wz) => m.PhysicalZoneId == wz.Id)
                    .Where((m, wz) => m.LogicalZoneId == lz.Id && m.Status == AlgoConstants.Status.ENABLED && wz.Status == AlgoConstants.Status.ENABLED)
                    .OrderBy((m, wz) => m.Priority)
                    .Select((m, wz) => new LogicalZonePhysicalMapping
                    {
                        PhysicalZoneId = wz.Id,
                        PhysicalZoneCode = wz.ZoneCode,
                        PhysicalZoneName = wz.ZoneName,
                        PhysicalZoneType = wz.ZoneType,
                        Priority = m.Priority,
                    })
                    .ToListAsync(cancellationToken);

                result.Add(new LogicalZoneDTO
                {
                    Id = lz.Id,
                    ZoneCode = lz.ZoneCode,
                    ZoneName = lz.ZoneName,
                    ZoneType = lz.ZoneType,
                    WarehouseId = lz.WarehouseId,
                    PhysicalZones = mappings,
                });
            }

            return result;
        }
    }
}
