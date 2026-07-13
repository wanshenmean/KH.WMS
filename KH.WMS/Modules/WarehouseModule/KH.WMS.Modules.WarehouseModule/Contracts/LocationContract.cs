using KH.WMS.Contracts.Warehouse;
using KH.WMS.Core.Database.UnitOfWorks;
using KH.WMS.Core.DependencyInjection.ServiceLifetimes;
using KH.WMS.Entities.Warehouse;
using Microsoft.Extensions.DependencyInjection;
using SqlSugar;

namespace KH.WMS.Modules.WarehouseModule.Contracts
{
    [RegisteredService(Lifetime = ServiceLifetime.Scoped, ServiceType = typeof(ILocationContract))]
    public class LocationContract(IUnitOfWork unitOfWork) : ILocationContract
    {
        /// <inheritdoc />
        public async Task UpdateLocationStatusAsync(long locationId, string status)
        {
            // 单条 UPDATE 原子更新，避免 read-modify-write 在并发下的丢失更新
            await unitOfWork.DbContext.Db.Updateable<MdLocation>()
                .SetColumns(l => new MdLocation { Status = status })
                .Where(l => l.Id == locationId)
                .ExecuteCommandAsync();
        }

        /// <inheritdoc />
        public async Task UpdateLocationLockStatusAsync(long locationId, byte lockStatus)
        {
            await unitOfWork.DbContext.Db.Updateable<MdLocation>()
                .SetColumns(l => new MdLocation { LockStatus = lockStatus })
                .Where(l => l.Id == locationId)
                .ExecuteCommandAsync();
        }
    }
}
