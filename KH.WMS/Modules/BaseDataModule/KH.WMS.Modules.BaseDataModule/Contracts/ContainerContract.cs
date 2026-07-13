using KH.WMS.Contracts.Container;
using KH.WMS.Core.DependencyInjection.ServiceLifetimes;
using KH.WMS.Entities.BaseData;
using KH.WMS.Entities.Constants;
using SqlSugar;

namespace KH.WMS.Modules.BaseDataModule.Contracts
{
    [RegisteredService(Lifetime = Microsoft.Extensions.DependencyInjection.ServiceLifetime.Scoped, ServiceType = typeof(IContainerContract))]
    public class ContainerContract(ISqlSugarClient db) : IContainerContract
    {
        /// <inheritdoc />
        public async Task<int> RegisterContainersAsync(List<string> containerNos)
        {
            if (containerNos == null || containerNos.Count == 0)
                return 0;

            // 查询已存在的容器
            var existingNos = await db.Queryable<MdContainer>()
                .Where(c => containerNos.Contains(c.ContainerNo))
                .Select(c => c.ContainerNo)
                .ToListAsync();

            var missingNos = containerNos.Except(existingNos).ToList();
            if (missingNos.Count == 0)
                return 0;

            // 获取默认容器类型
            var defaultContainerType = await db.Queryable<MdContainerType>()
                .FirstAsync();
            if (defaultContainerType == null)
                throw new InvalidOperationException("系统中不存在容器类型，请先创建容器类型");

            var newContainers = missingNos.Select(containerNo => new MdContainer
            {
                ContainerNo = containerNo,
                ContainerTypeId = defaultContainerType.Id,
                Status = BizConstants.ContainerStatus.EMPTY,
            }).ToList();

            await db.Insertable(newContainers).ExecuteCommandAsync();
            return newContainers.Count;
        }

        /// <inheritdoc />
        public async Task UpdateStatusAsync(List<string> containerNos, string status)
        {
            if (containerNos == null || containerNos.Count == 0) return;

            await db.Updateable<MdContainer>()
                .SetColumns(c => c.Status == status)
                .Where(c => containerNos.Contains(c.ContainerNo))
                .ExecuteCommandAsync();
        }
    }
}
