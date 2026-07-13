using KH.WMS.Core.Models;
using KH.WMS.Core.Services;
using KH.WMS.Entities.Inbound;
using KH.WMS.Modules.InboundModule.DTOs;

namespace KH.WMS.Modules.InboundModule
{
    public interface IInboundContainerBindService : ICrudService<InboundContainerBindHeader>
    {
        /// <summary>
        /// 组盘（容器与物料绑定，生成头+明细）
        /// </summary>
        Task<ServiceResult> ContainerBindAsync(List<ContainerBindDto> binds);

        /// <summary>
        /// 取消组盘（解除容器绑定，回滚容器状态与入库单组盘状态）
        /// </summary>
        Task<ServiceResult> CancelBindAsync(long headerId);

        /// <summary>
        /// 按入库单ID查询组盘记录（头+明细）
        /// </summary>
        Task<List<InboundContainerBindHeader>> GetByOrderIdAsync(long inboundOrderId);

        /// <summary>
        /// 按容器编号查询组盘记录（头+明细）
        /// </summary>
        Task<List<InboundContainerBindHeader>> GetByContainerCodeAsync(string containerCode);

        /// <summary>
        /// 请求上架（分配库位 + 创建上架任务）
        /// </summary>
        Task<ServiceResult> RequestPutAwayAsync(List<long> headerIds);

        /// <summary>
        /// WCS申请上架（通过容器编号 + 入库口编号）
        /// </summary>
        Task<ServiceResult> RequestPutAwayByWcsAsync(string containerCode, string portCode);
    }
}
