using KH.WMS.Core.Api.Responses;
using KH.WMS.Core.Database.Repositories;
using KH.WMS.Core.Database.UnitOfWorks;
using KH.WMS.Core.DependencyInjection.ServiceLifetimes;
using KH.WMS.Core.Services;
using KH.WMS.Entities.Constants;
using KH.WMS.Entities.Inventory;
using KH.WMS.Modules.InventoryModule.Interfaces;
using SqlSugar;

namespace KH.WMS.Modules.InventoryModule.Services
{
    [RegisteredService(ServiceType = typeof(IInvSnapshotHeaderService))]
    public class InvSnapshotHeaderService(
        IRepository<InvSnapshotHeader, long> repository,
        ISqlSugarClient db,
        IUnitOfWork unitOfWork,
        IDetailSaveService detailSaveService) : CrudService<InvSnapshotHeader>(repository, unitOfWork, detailSaveService), IInvSnapshotHeaderService
    {
        /// <inheritdoc />
        public async Task<ApiResponse> CreateSnapshotAsync(string snapshotName, string snapshotType, string? description, long? stocktakeId = null)
        {
            var now = DateTime.Now;
            var today = DateOnly.FromDateTime(now);

            // 关联库存头，获取仓库/库位/容器信息
            var details = await db.Queryable<InvInventoryDetail>()
                .LeftJoin<InvInventoryHeader>((d, h) => d.HeaderId == h.Id)
                .Where(d => d.Qty > 0)
                .Select((d, h) => new
                {
                    d.MaterialId,
                    d.MaterialCode,
                    d.BatchNo,
                    d.Qty,
                    WarehouseId = h.WarehouseId ?? 0,
                    WarehouseCode = h.WarehouseCode ?? string.Empty,
                    LocationId = h.LocationId ?? 0,
                    LocationCode = h.LocationCode ?? string.Empty,
                    HeaderId = h.Id,
                    ContainerCode = h.ContainerCode,
                })
                .ToListAsync();

            if (details.Count == 0)
                return ApiResponse.Fail(ResponseCode.BAD_REQUEST, "当前没有库存数据，无法创建快照");

            // 创建快照头
            var header = new InvSnapshotHeader
            {
                SnapshotNo = $"KP{now:yyyyMMddHHmmss}",
                SnapshotName = snapshotName,
                SnapshotType = snapshotType,
                SnapshotDate = today,
                MaterialCount = details.Select(d => d.MaterialId).Distinct().Count(),
                TotalStock = details.Sum(d => d.Qty),
                Description = description,
            };

            // 表头 + 明细两步写必须在同一事务中，否则明细写入失败会留下孤儿表头 + 脏汇总，
            // 且 (SnapshotType,SnapshotDate) 已有数据后次日重试也会被压制
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // DAILY 类型幂等：事务内检查当天是否已存在，避免后台服务或多实例重复生成。
                // 彻底防止并发需数据库对 DAILY 类型加 (SnapshotType,SnapshotDate) 过滤唯一约束（见审查报告 #34）。
                if (snapshotType == BizConstants.SnapshotTypes.DAILY)
                {
                    var dailyExists = await db.Queryable<InvSnapshotHeader>()
                        .Where(h => h.SnapshotType == BizConstants.SnapshotTypes.DAILY && h.SnapshotDate == today)
                        .AnyAsync();
                    if (dailyExists)
                    {
                        await _unitOfWork.RollbackAsync();
                        return ApiResponse.Ok(new { skipped = true }, "今日每日库存快照已存在，跳过");
                    }
                }

                await db.Insertable(header).ExecuteReturnEntityAsync();

                // 生成快照明细
                var snapshotRecords = details.Select(d => new InvSnapshot
                {
                    SnapshotHeaderId = header.Id,
                    SnapshotDate = today,
                    SnapshotType = snapshotType,
                    StocktakeId = stocktakeId,
                    WarehouseId = d.WarehouseId,
                    LocationId = d.LocationId,
                    MaterialId = d.MaterialId,
                    BatchNo = d.BatchNo,
                    ContainerId = d.HeaderId,
                    Qty = d.Qty,
                    SnapshotTime = now,
                }).ToList();

                await db.Insertable(snapshotRecords).ExecuteCommandAsync();

                await _unitOfWork.CommitAsync();
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }

            return ApiResponse.Ok(new
            {
                header.Id,
                header.SnapshotNo,
                header.MaterialCount,
                header.TotalStock,
            });
        }

        /// <inheritdoc />
        public async Task<ApiResponse> GetSnapshotDetailsAsync(long headerId)
        {
            var header = await db.Queryable<InvSnapshotHeader>()
                .Where(h => h.Id == headerId)
                .FirstAsync();

            if (header == null)
                return ApiResponse.NotFound("快照不存在");

            var details = await db.Queryable<InvSnapshot>()
                .Where(s => s.SnapshotHeaderId == headerId)
                .OrderBy(s => s.WarehouseId)
                .OrderBy(s => s.LocationId)
                .OrderBy(s => s.MaterialId)
                .ToListAsync();

            return ApiResponse.Ok(new
            {
                header.Id,
                header.SnapshotNo,
                header.SnapshotName,
                header.SnapshotType,
                header.SnapshotDate,
                header.MaterialCount,
                header.TotalStock,
                header.Description,
                Details = details,
            });
        }
    }
}
