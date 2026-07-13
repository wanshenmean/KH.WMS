using KH.WMS.Core.Models;
using KH.WMS.Core.Services;
using KH.WMS.Entities.Outbound;
using KH.WMS.Modules.OutboundModule.DTOs;

namespace KH.WMS.Modules.OutboundModule.Interfaces
{
    public interface IOutboundOrderService : ICrudService<OutboundOrder>
    {
        /// <summary>创建出库单（含明细行）</summary>
        Task<long> CreateWithLinesAsync(OutboundOrderCreateDto dto);

        /// <summary>更新出库单（含明细行，全量替换）</summary>
        Task<bool> UpdateWithLinesAsync(long id, OutboundOrderCreateDto dto);

        /// <summary>获取出库单详情（含明细行）</summary>
        Task<OutboundOrderCreateDto?> GetDetailAsync(long id);

        /// <summary>确认出库单（DRAFT → CONFIRMED，确认后不可编辑/删除）</summary>
        Task<ServiceResult> ConfirmAsync(long id);
    }
}
