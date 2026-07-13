using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KH.WMS.Algorithms.Strategy.Constants;
using KH.WMS.Algorithms.Strategy.DTOs;
using KH.WMS.Core.DependencyInjection.ServiceLifetimes;
using SqlSugar;

namespace KH.WMS.Algorithms.Strategy.QueryServices
{
    [RegisteredService(ServiceType = typeof(IInventoryQueryService))]
    public class InventoryQueryService : IInventoryQueryService
    {
        // #34 说明：所有库存查询带 With(UpdLock) 是有意为之——出库分配调用方(OutboundAllocationService.AllocateAsync)
        // 在事务内执行并对订单行加 UpdLock 串行化，库存查询的更新锁防止分配期间库存被并发改动导致超卖。
        // 不配 READPAST：保证 FIFO/FEFO 不跳过被锁的最旧行（吞吐优先级低于分配正确性）。
        // 如需高并发吞吐，可在充分并发测试后改为 UpdLock+ReadPastCommitted（会跳过正在分配的行）。
        private string availableStatus = AlgoConstants.InventoryStatus.AVAILABLE;
        private string occupiedStatus = AlgoConstants.LocationStatus.OCCUPIED;

        private readonly ISqlSugarClient _db;

        public InventoryQueryService(ISqlSugarClient db)
        {
            _db = db;
        }

        /// <summary>
        /// 获取按FIFO排序的库存列表
        /// </summary>
        public async Task<List<InventoryInfoDTO>> GetByFIFOAsync(string warehouseCode, string materialCode, CancellationToken cancellationToken = default)
        {
            // 查询库存明细和库存表，获取符合条件的库存信息
            // 注意：不在 SQL 层 OrderBy InboundTime——SQL Server 升序时 NULL 排最前会破坏 FIFO。
            // 改为取出后在内存按"入库时间升序、NULL 兜底为最大值"统一排序（与 FEFO 一致）。
            var result = await _db.Queryable<InvInventoryDetailDTO>().With(SqlWith.UpdLock)
                 .InnerJoin<InvInventoryHeaderDTO>((d, h) => d.HeaderId == h.Id)
                 .InnerJoin<MdLocationDTO>((d, h, l) => h.LocationCode == l.LocationCode)
                 .Where((d, h, l) => h.WarehouseCode == warehouseCode
                                    && d.MaterialCode == materialCode
                                    && d.Qty - d.LockedQty > 0
                                    && h.InventoryStatus == availableStatus
                                    && l.LockStatus == AlgoConstants.LocationLockStatus.NONE
                                    && l.IsDisabled == AlgoConstants.BoolFlag.NO
                                    && l.Status == occupiedStatus)
               .Select((d, h, l) => new InventoryInfoDTO
               {
                   Id = d.Id,
                   WarehouseCode = h.WarehouseCode,
                   MaterialId = d.MaterialId,
                   LocationCode = h.LocationCode,
                   ZoneCode = l.ZoneCode,
                   BatchNo = d.BatchNo,
                   Qty = d.Qty,
                   LockedQty = d.LockedQty,
                   ProductionDate = d.ProductionDate,
                   ExpiryDate = d.ExpiryDate,
                   InboundTime = d.InboundTime,
                   SupplierId = d.SupplierId,
                   ContainerCode = h.ContainerCode,
                   InventoryStatus = h.InventoryStatus,
                   MaterialCode = d.MaterialCode,
                   Unit = d.Unit,
                   HeaderId = d.HeaderId,
                   InboundDocNo = d.InboundDocNo,
                   LocationId = h.LocationId,
                   SerialNo = d.SerialNo,
                   WarehouseId = h.WarehouseId
               })
               .ToListAsync(cancellationToken);

            return result.OrderBy(x => x.InboundTime ?? DateTime.MaxValue).ToList();
        }

        /// <summary>
        /// 获取按FEFO排序的库存列表（按过期时间排序）
        /// </summary>
        public async Task<List<InventoryInfoDTO>> GetByFEFOAsync(string warehouseCode, string materialCode, CancellationToken cancellationToken = default)
        {
            var result = await _db.Queryable<InvInventoryDetailDTO>().With(SqlWith.UpdLock)
                 .InnerJoin<InvInventoryHeaderDTO>((d, h) => d.HeaderId == h.Id)
                 .InnerJoin<MdLocationDTO>((d, h, l) => h.LocationCode == l.LocationCode)
                 .Where((d, h, l) => h.WarehouseCode == warehouseCode
                                    && d.MaterialCode == materialCode
                                    && d.Qty - d.LockedQty > 0
                                    && h.InventoryStatus == availableStatus
                                    && l.LockStatus == AlgoConstants.LocationLockStatus.NONE
                                    && l.IsDisabled == AlgoConstants.BoolFlag.NO
                                    && l.Status == occupiedStatus)
                  .Select((d, h) => new InventoryInfoDTO
                  {
                      Id = d.Id,
                      WarehouseCode = h.WarehouseCode,
                      MaterialId = d.MaterialId,
                      LocationCode = h.LocationCode,
                      BatchNo = d.BatchNo,
                      Qty = d.Qty,
                      LockedQty = d.LockedQty,
                      ProductionDate = d.ProductionDate,
                      ExpiryDate = d.ExpiryDate,
                      InboundTime = d.InboundTime,
                      SupplierId = d.SupplierId,
                      ContainerCode = h.ContainerCode,
                      InventoryStatus = h.InventoryStatus,
                      MaterialCode = d.MaterialCode,
                      Unit = d.Unit,
                      HeaderId = d.HeaderId,
                      InboundDocNo = d.InboundDocNo,
                      LocationId = h.LocationId,
                      SerialNo = d.SerialNo,
                      WarehouseId = h.WarehouseId
                  })
                  .ToListAsync(cancellationToken);

            // FEFO：先过期先出，无过期日期(ExpiryDate 为 null)的库存应排到最后出库。
            // 各数据库对 ORDER BY NULL 的默认位置不一致（SQL Server 升序时 NULL 排最前），
            // 故改为取出后在内存中按"过期日期升序、NULL 兜底为最大值"统一排序，保证语义一致。
            return result.OrderBy(x => x.ExpiryDate ?? DateOnly.MaxValue).ToList();
        }

        /// <summary>
        /// 获取指定批次的库存列表
        /// </summary>
        public async Task<List<InventoryInfoDTO>> GetByBatchAsync(string warehouseCode, string materialCode, string batchNo, CancellationToken cancellationToken = default)
        {
            // #47：batchNo 为空直接返回空（避免生成 BatchNo IS NULL 查询误返无批次库存）
            if (string.IsNullOrWhiteSpace(batchNo))
                return new List<InventoryInfoDTO>();

            return await _db.Queryable<InvInventoryDetailDTO>().With(SqlWith.UpdLock)
                 .InnerJoin<InvInventoryHeaderDTO>((d, h) => d.HeaderId == h.Id)
                 .InnerJoin<MdLocationDTO>((d, h, l) => h.LocationCode == l.LocationCode)
                 .Where((d, h, l) => h.WarehouseCode == warehouseCode
                                    && d.MaterialCode == materialCode
                                    && d.BatchNo == batchNo
                                    && d.Qty - d.LockedQty > 0
                                    && h.InventoryStatus == availableStatus
                                    && l.LockStatus == AlgoConstants.LocationLockStatus.NONE
                                    && l.IsDisabled == AlgoConstants.BoolFlag.NO
                                    && l.Status == occupiedStatus)
                .Select((d, h) => new InventoryInfoDTO
                {
                    Id = d.Id,
                    WarehouseCode = h.WarehouseCode,
                    MaterialId = d.MaterialId,
                    LocationCode = h.LocationCode,
                    BatchNo = d.BatchNo,
                    Qty = d.Qty,
                    LockedQty = d.LockedQty,
                    ProductionDate = d.ProductionDate,
                    ExpiryDate = d.ExpiryDate,
                    InboundTime = d.InboundTime,
                    SupplierId = d.SupplierId,
                    ContainerCode = h.ContainerCode,
                    InventoryStatus = h.InventoryStatus,
                    MaterialCode = d.MaterialCode,
                    Unit = d.Unit,
                    HeaderId = d.HeaderId,
                    InboundDocNo = d.InboundDocNo,
                    LocationId = h.LocationId,
                    SerialNo = d.SerialNo,
                    WarehouseId = h.WarehouseId
                })
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// 获取指定物料的库存列表
        /// </summary>
        public async Task<List<InventoryInfoDTO>> GetByMaterialAsync(string warehouseCode, string materialCode, CancellationToken cancellationToken = default)
        {
            return await _db.Queryable<InvInventoryDetailDTO>().With(SqlWith.UpdLock)
                 .InnerJoin<InvInventoryHeaderDTO>((d, h) => d.HeaderId == h.Id)
                 .InnerJoin<MdLocationDTO>((d, h, l) => h.LocationCode == l.LocationCode)
                 .Where((d, h, l) => h.WarehouseCode == warehouseCode
                                    && d.MaterialCode == materialCode
                                    && d.Qty - d.LockedQty > 0
                                    && h.InventoryStatus == availableStatus
                                    && l.LockStatus == AlgoConstants.LocationLockStatus.NONE
                                    && l.IsDisabled == AlgoConstants.BoolFlag.NO
                                        && l.Status == occupiedStatus)
                 .Select((d, h) => new InventoryInfoDTO
                 {
                     Id = d.Id,
                     WarehouseCode = h.WarehouseCode,
                     MaterialId = d.MaterialId,
                     LocationCode = h.LocationCode,
                     BatchNo = d.BatchNo,
                     Qty = d.Qty,
                     LockedQty = d.LockedQty,
                     ProductionDate = d.ProductionDate,
                     ExpiryDate = d.ExpiryDate,
                     InboundTime = d.InboundTime,
                     SupplierId = d.SupplierId,
                     ContainerCode = h.ContainerCode,
                     InventoryStatus = h.InventoryStatus,
                     MaterialCode = d.MaterialCode,
                     Unit = d.Unit,
                     HeaderId = d.HeaderId,
                     InboundDocNo = d.InboundDocNo,
                     LocationId = h.LocationId,
                     SerialNo = d.SerialNo,
                     WarehouseId = h.WarehouseId
                 })
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// 获取指定货位的库存列表（#36：补齐与其它查询一致的过滤条件）
        /// </summary>
        public async Task<List<InventoryInfoDTO>> GetByLocationAsync(string locationCode, CancellationToken cancellationToken = default)
        {
            return await _db.Queryable<InvInventoryDetailDTO>().With(SqlWith.UpdLock)
               .InnerJoin<InvInventoryHeaderDTO>((d, h) => d.HeaderId == h.Id)
               .InnerJoin<MdLocationDTO>((d, h, l) => h.LocationCode == l.LocationCode)
               .Where((d, h, l) => h.LocationCode == locationCode
                                   && d.Qty - d.LockedQty > 0
                                   && h.InventoryStatus == availableStatus
                                   && l.LockStatus == AlgoConstants.LocationLockStatus.NONE
                                   && l.IsDisabled == AlgoConstants.BoolFlag.NO
                                   && l.Status == occupiedStatus)
               .Select((d, h, l) => new InventoryInfoDTO
               {
                   Id = d.Id,
                   WarehouseCode = h.WarehouseCode,
                   MaterialId = d.MaterialId,
                   LocationCode = h.LocationCode,
                   ZoneCode = l.ZoneCode,
                   BatchNo = d.BatchNo,
                   Qty = d.Qty,
                   LockedQty = d.LockedQty,
                   ProductionDate = d.ProductionDate,
                   ExpiryDate = d.ExpiryDate,
                   InboundTime = d.InboundTime,
                   SupplierId = d.SupplierId,
                   ContainerCode = h.ContainerCode,
                   InventoryStatus = h.InventoryStatus,
                   MaterialCode = d.MaterialCode,
                   Unit = d.Unit,
                   HeaderId = d.HeaderId,
                   InboundDocNo = d.InboundDocNo,
                   LocationId = h.LocationId,
                   SerialNo = d.SerialNo,
                   WarehouseId = h.WarehouseId
               })
               .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// 获取指定区域的库存列表
        /// </summary>
        public async Task<List<InventoryInfoDTO>> GetByAreaAsync(string warehouseCode, string areaCode, string materialCode, CancellationToken cancellationToken = default)
        {
            return await _db.Queryable<InvInventoryDetailDTO>().With(SqlWith.UpdLock)
                .InnerJoin<InvInventoryHeaderDTO>((d, h) => d.HeaderId == h.Id)
                .InnerJoin<MdLocationDTO>((d, h, l) => h.LocationCode == l.LocationCode)
                .Where((d, h, l) => h.WarehouseCode == warehouseCode
                                   && l.ZoneCode == areaCode
                                   && d.MaterialCode == materialCode
                                   && d.Qty - d.LockedQty > 0
                                   && h.InventoryStatus == availableStatus
                                   && l.LockStatus == AlgoConstants.LocationLockStatus.NONE
                                   && l.IsDisabled == AlgoConstants.BoolFlag.NO
                                   && l.Status == occupiedStatus)
                .Select((d, h, l) => new InventoryInfoDTO
                {
                    Id = d.Id,
                    WarehouseCode = h.WarehouseCode,
                    MaterialId = d.MaterialId,
                    LocationCode = h.LocationCode,
                    BatchNo = d.BatchNo,
                    Qty = d.Qty,
                    LockedQty = d.LockedQty,
                    ProductionDate = d.ProductionDate,
                    ExpiryDate = d.ExpiryDate,
                    InboundTime = d.InboundTime,
                    SupplierId = d.SupplierId,
                    ContainerCode = h.ContainerCode,
                    InventoryStatus = h.InventoryStatus,
                    MaterialCode = d.MaterialCode,
                    Unit = d.Unit,
                    HeaderId = d.HeaderId,
                    InboundDocNo = d.InboundDocNo,
                    LocationId = h.LocationId,
                    SerialNo = d.SerialNo,
                    WarehouseId = h.WarehouseId
                })
                .ToListAsync(cancellationToken);
        }
    }
}
