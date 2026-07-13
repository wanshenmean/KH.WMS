using KH.WMS.Algorithms.Strategy.Constants;
using KH.WMS.Algorithms.Strategy.DTOs;
using KH.WMS.Core.DependencyInjection.ServiceLifetimes;
using SqlSugar;

namespace KH.WMS.Algorithms.Strategy.QueryServices
{
    /// <summary>
    /// 货位查询服务实现
    /// </summary>
    [RegisteredService(Lifetime = Microsoft.Extensions.DependencyInjection.ServiceLifetime.Scoped, ServiceType = typeof(ILocationQueryService))]
    public class LocationQueryService : ILocationQueryService
    {
        private readonly ISqlSugarClient _db;

        public LocationQueryService(ISqlSugarClient db)
        {
            _db = db;
        }

        /// <summary>
        /// 查询空闲可用货位（未锁定、未禁用、状态为EMPTY）
        /// </summary>
        public async Task<List<MdLocationDTO>> GetEmptyLocationsAsync(long warehouseId, long? zoneId = null,
            string? locationType = null, CancellationToken cancellationToken = default)
        {
            return await _db.Queryable<MdLocationDTO>()
                .With(SqlWith.UpdLock)
                .Where(l => l.WarehouseId == warehouseId
                            && l.Status == AlgoConstants.LocationStatus.EMPTY
                            && l.LockStatus == AlgoConstants.LocationLockStatus.NONE
                            && l.IsDisabled == AlgoConstants.BoolFlag.NO)
                .WhereIF(zoneId.HasValue, l => l.ZoneId == zoneId)
                .WhereIF(!string.IsNullOrWhiteSpace(locationType), l => l.LocationType == locationType)
                .OrderBy(l => l.LayerNo)
                .OrderBy(l => l.RowNo)
                .OrderBy(l => l.ColNo)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// 查询指定库区的所有货位
        /// </summary>
        public async Task<List<MdLocationDTO>> GetLocationsByZoneAsync(long zoneId,
            CancellationToken cancellationToken = default)
        {
            return await _db.Queryable<MdLocationDTO>()
                .Where(l => l.ZoneId == zoneId)
                .OrderBy(l => l.AisleNo)
                .OrderBy(l => l.LayerNo)
                .OrderBy(l => l.RowNo)
                .OrderBy(l => l.ColNo)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// 查询某货位附近的空闲货位
        /// 优先级：同巷道同排 > 同巷道 > 同库区
        /// </summary>
        public async Task<List<MdLocationDTO>> GetLocationsNearAsync(string locationCode, long warehouseId,
            int maxCount = 10, CancellationToken cancellationToken = default)
        {
            // 先找到参考货位
            var refLocation = await _db.Queryable<MdLocationDTO>()
                .Where(l => l.LocationCode == locationCode && l.WarehouseId == warehouseId)
                .FirstAsync(cancellationToken);

            if (refLocation == null)
                return new List<MdLocationDTO>();

            // 查询同仓库的空闲货位
            var emptyLocations = await _db.Queryable<MdLocationDTO>()
                .Where(l => l.WarehouseId == warehouseId
                            && l.Status == AlgoConstants.LocationStatus.EMPTY
                            && l.LockStatus == AlgoConstants.LocationLockStatus.NONE
                            && l.IsDisabled == AlgoConstants.BoolFlag.NO)
                .ToListAsync(cancellationToken);

            // 按距离排序：同巷道同排(距离0) > 同巷道(距离1) > 同库区(距离2) > 其他(距离3)
            var scored = emptyLocations.Select(l =>
            {
                int distance;
                if (l.AisleNo == refLocation.AisleNo && l.Side == refLocation.Side)
                    distance = 0; // 同巷道同排
                else if (l.AisleNo == refLocation.AisleNo)
                    distance = 1; // 同巷道不同排
                else if (l.ZoneId == refLocation.ZoneId && refLocation.ZoneId.HasValue)
                    distance = 2; // 同库区
                else
                    distance = 3; // 其他

                // 同距离内按层、排、列排序
                return new { Location = l, Distance = distance };
            })
            .OrderBy(x => x.Distance)
            .ThenBy(x => x.Location.LayerNo)
            .ThenBy(x => x.Location.RowNo)
            .ThenBy(x => x.Location.ColNo)
            .Take(maxCount)
            .Select(x => x.Location)
            .ToList();

            return scored;
        }

        /// <summary>
        /// 查询已有指定物料库存的货位编码列表
        /// </summary>
        public async Task<List<string>> GetLocationCodesWithInventoryAsync(long warehouseId, long materialId,
            string? batchNo = null, CancellationToken cancellationToken = default)
        {
            return await _db.Queryable<InvInventoryDetailDTO>()
                .InnerJoin<InvInventoryHeaderDTO>((d, h) => d.HeaderId == h.Id)
                .Where((d, h) => h.WarehouseId == warehouseId
                                   && d.MaterialId == materialId
                                   && d.Qty - d.LockedQty > 0
                                   && h.InventoryStatus == AlgoConstants.InventoryStatus.AVAILABLE)
                .WhereIF(!string.IsNullOrWhiteSpace(batchNo), (d, h) => d.BatchNo == batchNo)
                .Select((d, h) => h.LocationCode!)
                .Distinct()
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// 查询指定货位的配对货位（相同地址不同深度）
        /// 双深货位：同一个物理位置有 Depth=1（前排）和 Depth=2（后排）
        /// </summary>
        public async Task<MdLocationDTO?> GetPairLocationAsync(long locationId, CancellationToken cancellationToken = default)
        {
            // 先找到参考货位
            var refLocation = await _db.Queryable<MdLocationDTO>()
                .Where(l => l.Id == locationId)
                .FirstAsync(cancellationToken);

            if (refLocation == null)
                return null;

            // 查找配对货位：相同仓库+巷道+排+列+层，但深度不同
            var pairDepth = refLocation.Depth == AlgoConstants.DepthType.FRONT ? AlgoConstants.DepthType.BACK : AlgoConstants.DepthType.FRONT;

            return await _db.Queryable<MdLocationDTO>()
                .Where(l => l.WarehouseId == refLocation.WarehouseId
                            && l.AisleNo == refLocation.AisleNo
                            && l.Side == refLocation.Side
                            && l.RowNo == refLocation.RowNo
                            && l.ColNo == refLocation.ColNo
                            && l.LayerNo == refLocation.LayerNo
                            && l.Depth == pairDepth)
                .FirstAsync(cancellationToken);
        }

        /// <summary>
        /// 检查后排货位对应的前排货位状态
        /// 如果指定的是前排货位（Depth=1），返回null
        /// 如果指定的是后排货位（Depth=2），返回对应前排货位的状态
        /// </summary>
        public async Task<string?> GetFrontLocationStatusAsync(long locationId, CancellationToken cancellationToken = default)
        {
            var location = await _db.Queryable<MdLocationDTO>()
                .Where(l => l.Id == locationId)
                .FirstAsync(cancellationToken);

            if (location == null || location.Depth != AlgoConstants.DepthType.BACK)
                return null;

            // 查找前排货位
            var frontLocation = await _db.Queryable<MdLocationDTO>()
                .Where(l => l.WarehouseId == location.WarehouseId
                            && l.AisleNo == location.AisleNo
                            && l.Side == location.Side
                            && l.RowNo == location.RowNo
                            && l.ColNo == location.ColNo
                            && l.LayerNo == location.LayerNo
                            && l.Depth == AlgoConstants.DepthType.FRONT)
                .FirstAsync(cancellationToken);

            return frontLocation?.Status;
        }
    }
}
