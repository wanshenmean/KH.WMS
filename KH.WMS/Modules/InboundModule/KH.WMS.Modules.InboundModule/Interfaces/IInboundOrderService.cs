using KH.WMS.Core.Models;
using KH.WMS.Core.Services;
using KH.WMS.Entities.Inbound;
using KH.WMS.Modules.InboundModule.DTOs;

namespace KH.WMS.Modules.InboundModule
{
    public interface IInboundOrderService : ICrudService<InboundOrder>
    {
        Task<ServiceResult> ReceiveInboundOrder(InboundOrderDto inboundOrderDto);

        /// <summary>创建入库单（含明细行）</summary>
        Task<long> CreateWithLinesAsync(InboundOrderCreateDto dto);

        /// <summary>更新入库单（含明细行，全量替换）</summary>
        Task<bool> UpdateWithLinesAsync(long id, InboundOrderCreateDto dto);

        /// <summary>获取入库单详情（含明细行）</summary>
        Task<InboundOrderCreateDto?> GetDetailAsync(long id);

        /// <summary>收货（更新明细行数量和状态）</summary>
        Task<ServiceResult> ReceiveAsync(long orderId, List<ReceiveLineDto> receiveLines);

        /// <summary>收货并组盘（事务内完成收货+组盘）</summary>
        Task<ServiceResult> ReceiveAndBindAsync(ReceiveAndBindDto dto);

        /// <summary>获取入库单的组盘记录（头+明细）</summary>
        Task<List<InboundContainerBindHeader>> GetContainerBindsAsync(long orderId);
    }
}
