using KH.WMS.Contracts.Inbound;
using KH.WMS.Core.Database.UnitOfWorks;
using KH.WMS.Core.DependencyInjection.ServiceLifetimes;
using KH.WMS.Entities.Constants;
using KH.WMS.Entities.Inbound;
using Microsoft.Extensions.DependencyInjection;

namespace KH.WMS.Modules.InboundModule.Contracts
{
    [RegisteredService(Lifetime = ServiceLifetime.Scoped, ServiceType = typeof(IInboundOrderContract))]
    public class InboundOrderContract(IUnitOfWork unitOfWork) : IInboundOrderContract
    {
        /// <inheritdoc />
        public async Task<string?> GetOrderStatusAsync(long orderId)
        {
            var orderRepo = unitOfWork.GetRepository<InboundOrder, long>();
            var order = await orderRepo.GetByIdAsync(orderId);
            return order?.OrderStatus;
        }

        /// <inheritdoc />
        public async Task<long?> GetOrderIdByOrderNoAsync(string orderNo)
        {
            var orderRepo = unitOfWork.GetRepository<InboundOrder, long>();
            var order = await orderRepo.GetFirstOrDefaultAsync(o => o.OrderNo == orderNo);
            return order?.Id;
        }

        /// <inheritdoc />
        public async Task<bool> IsReceivedAsync(long orderId)
        {
            var orderRepo = unitOfWork.GetRepository<InboundOrder, long>();
            var order = await orderRepo.GetByIdAsync(orderId);
            return order?.OrderStatus == BizConstants.InboundOrderStatus.RECEIVED
                || order?.OrderStatus == BizConstants.InboundOrderStatus.BOUND
                || order?.OrderStatus == BizConstants.InboundOrderStatus.COMPLETED;
        }

        /// <inheritdoc />
        public async Task<long?> GetWarehouseIdAsync(long orderId)
        {
            var orderRepo = unitOfWork.GetRepository<InboundOrder, long>();
            var order = await orderRepo.GetByIdAsync(orderId);
            return order?.WarehouseId;
        }

        /// <inheritdoc />
        public async Task<bool> MarkBindAsPuttingAwayAsync(long bindHeaderId)
        {
            var bindHeaderRepo = unitOfWork.GetRepository<InboundContainerBindHeader, long>();
            var bindHeader = await bindHeaderRepo.GetByIdAsync(bindHeaderId);
            if (bindHeader == null) return false;

            bindHeader.BindStatus = BizConstants.BindStatus.PUTTING_AWAY;
            await bindHeaderRepo.UpdateAsync(bindHeader);
            return true;
        }

        /// <inheritdoc />
        public async Task<bool> MarkBindAsPutAwayAsync(long bindHeaderId)
        {
            var bindHeaderRepo = unitOfWork.GetRepository<InboundContainerBindHeader, long>();
            var bindHeader = await bindHeaderRepo.GetByIdAsync(bindHeaderId);
            if (bindHeader == null) return false;

            bindHeader.BindStatus = BizConstants.BindStatus.PUT_AWAY;
            await bindHeaderRepo.UpdateAsync(bindHeader);
            return true;
        }

        /// <inheritdoc />
        public async Task<string?> GetBindSourceDocNoAsync(long bindHeaderId)
        {
            var bindHeaderRepo = unitOfWork.GetRepository<InboundContainerBindHeader, long>();
            var bindHeader = await bindHeaderRepo.GetByIdAsync(bindHeaderId);
            return bindHeader?.SourceDocNo;
        }

        /// <inheritdoc />
        public async Task<List<BindDetailData>?> GetBindDetailsAsync(long bindHeaderId)
        {
            var bindDetailRepo = unitOfWork.GetRepository<InboundContainerBindDetail, long>();
            var details = await bindDetailRepo.GetListAsync(d => d.HeaderId == bindHeaderId);

            return details.Select(d => new BindDetailData
            {
                MaterialId = d.MaterialId,
                MaterialCode = d.MaterialCode,
                Qty = d.Qty,
                BatchNo = d.BatchNo,
                ProductionDate = d.ProductionDate,
                ExpiryDate = d.ExpiryDate,
            }).ToList();
        }

        /// <inheritdoc />
        public async Task<bool> RevertBindToBoundAsync(long bindHeaderId)
        {
            var bindHeaderRepo = unitOfWork.GetRepository<InboundContainerBindHeader, long>();
            var bindHeader = await bindHeaderRepo.GetByIdAsync(bindHeaderId);
            if (bindHeader == null) return false;
            if (bindHeader.BindStatus != BizConstants.BindStatus.PUTTING_AWAY) return false;

            bindHeader.BindStatus = BizConstants.BindStatus.BOUND;
            await bindHeaderRepo.UpdateAsync(bindHeader);
            return true;
        }
    }
}
