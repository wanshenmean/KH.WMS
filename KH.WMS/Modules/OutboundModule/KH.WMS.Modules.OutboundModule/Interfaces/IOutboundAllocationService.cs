using KH.WMS.Core.Models;
using KH.WMS.Core.Services;
using KH.WMS.Entities.Outbound;

namespace KH.WMS.Modules.OutboundModule
{
    public interface IOutboundAllocationService : ICrudService<OutboundAllocationHeader>
    {
        /// <summary>
        /// 按出库单ID查询分配记录（头+明细）
        /// </summary>
        Task<List<OutboundAllocationHeader>> GetByOrderIdAsync(long outboundOrderId);

        /// <summary>
        /// 执行库存分配（按策略为出库单锁定具体库位和批次的库存）
        /// </summary>
        Task<ServiceResult> AllocateAsync(long outboundOrderId);

        /// <summary>
        /// 根据分配头生成出库搬运任务
        /// 将分配明细按容器分组，每个容器生成一个拣货任务
        /// </summary>
        Task<ServiceResult> GenerateTasksAsync(long allocationHeaderId);
    }
}
