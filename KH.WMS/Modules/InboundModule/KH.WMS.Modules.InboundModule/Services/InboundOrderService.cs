using System.Text.Json;
using KH.WMS.Config.Abstractions;
using KH.WMS.Config.Abstractions;
using KH.WMS.Contracts.BaseData;
using KH.WMS.Contracts.Inbound;
using KH.WMS.Core.Database.Repositories;
using KH.WMS.Core.Database.UnitOfWorks;
using KH.WMS.Core.DependencyInjection.ServiceLifetimes;
using KH.WMS.Core.Logging;
using KH.WMS.Core.Logging.Serilog;
using KH.WMS.Core.Logging.WMSError;
using KH.WMS.Core.Models;
using KH.WMS.Core.Models.Dtos;
using KH.WMS.Core.Services;
using KH.WMS.Entities.Constants;
using KH.WMS.Entities.Inbound;
using KH.WMS.Modules.InboundModule.DTOs;
using Microsoft.Extensions.DependencyInjection;
using static KH.WMS.Core.Logging.WMSError.WMSErrorCodes;

namespace KH.WMS.Modules.InboundModule
{
    [RegisteredService(Lifetime = ServiceLifetime.Scoped, ServiceType = typeof(IInboundOrderService))]
    public class InboundOrderService(
        IRepository<InboundOrder, long> repository,
        IRepository<InboundOrderLine, long> inboundOrderLineRepository,
        ICodeGeneratorService codeGenerator,
        IInboundContainerBindService containerBindService,
        IMaterialContract materialContract,
        IDocumentStatusValidatorContract statusValidator,
        IUnitOfWork unitOfWork,
        IDetailSaveService detailSaveService,
        ILoggerService loggerService,
        ICfgDocumentFieldExtContract extFieldService)
        : CrudService<InboundOrder>(repository, unitOfWork, detailSaveService), IInboundOrderService
    {
        private readonly IRepository<InboundOrderLine, long> _lineRepository = inboundOrderLineRepository;
        private readonly ICodeGeneratorService _codeGenerator = codeGenerator;
        private readonly IInboundContainerBindService _containerBindService = containerBindService;
        private readonly IMaterialContract _materialContract = materialContract;
        private readonly IDocumentStatusValidatorContract _statusValidator = statusValidator;
        private readonly ICfgDocumentFieldExtContract _extFieldService = extFieldService;
        private readonly ILoggerService _loggerService = loggerService;

        public async Task<ServiceResult> ReceiveInboundOrder(InboundOrderDto inboundOrderDto)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // 从配置表获取初始状态
                var initialStatus = await _statusValidator.GetInitialStatusAsync(OrderTypeMappings.ToDocTypeCode(inboundOrderDto.SourceDocType))
                    ?? throw new Exception("未找到入库单的初始状态配置");

                InboundOrder inboundOrder = new InboundOrder()
                {
                    OrderNo = await _codeGenerator.GenerateAsync(BizConstants.CodeRuleTypes.INBOUND_DOC),
                    OrderDate = DateOnly.FromDateTime(DateTime.Now),
                    OrderStatus = initialStatus,
                    OrderType = inboundOrderDto.SourceDocType ?? BizConstants.OrderTypes.PURCHASE_IN,
                    SourceDocNo = inboundOrderDto.SourceDocNo,
                    SourceDocType = inboundOrderDto.SourceDocType,
                    SourceSystem = inboundOrderDto.SourceSystem,
                    SupplierId = inboundOrderDto.SuplierId,
                    WarehouseId = inboundOrderDto.WarehouseId,
                    TotalLines = inboundOrderDto.Details.Count,
                    ExtData = inboundOrderDto.ExtData,
                };

                List<InboundOrderLine> inboundOrderLines = new List<InboundOrderLine>();
                int index = 0;
                foreach (var item in inboundOrderDto.Details)
                {
                    // 通过契约查询物料信息
                    var material = await _materialContract.GetByCodeAsync(item.MaterialCode);

                    InboundOrderLine inboundOrderLine = new InboundOrderLine()
                    {
                        LineNo = ++index,
                        MaterialId = material?.Id ?? 0,
                        MaterialCode = item.MaterialCode,
                        MaterialName = material?.MaterialName,
                        OrderedQty = item.OrderedQty,
                        ReceivedQty = 0,
                        UnitId = item.UnitId ?? material?.BaseUnitId,
                        BatchNo = item.BatchNo,
                        ManufactureDate = item.ManufactureDate,
                        ExpiryDate = item.ExpiryDate,
                        LineStatus = BizConstants.InboundLineStatus.OPEN,
                        QualityStatus = BizConstants.QualityStatus.PENDING,
                        ExtData = item.ExtData,
                    };
                    inboundOrderLines.Add(inboundOrderLine);
                }

                // 通过导航属性一次性插入主表 + 从表
                inboundOrder.Items = inboundOrderLines;
                await _repository.AddWithNavAsync(inboundOrder);

                await _unitOfWork.CommitAsync();
                return ServiceResult.Ok(WMSErrorMessages.GetMessage(RECEIVE_SUCCESS));
            }
            catch (Exception ex)
            {
                _loggerService.LogError(ex, ex.Message);
                await _unitOfWork.RollbackAsync();
                return ServiceResult.Fail(WMSErrorMessages.GetMessage(RECEIVE_INBOUND_FAILED, ex.Message));
            }
        }

        public async Task<long> CreateWithLinesAsync(InboundOrderCreateDto dto)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var order = dto.Order;
                order.OrderNo = await _codeGenerator.GenerateAsync(BizConstants.CodeRuleTypes.INBOUND_DOC);
                order.TotalLines = dto.Lines.Count;

                // 从配置表获取初始状态
                order.OrderStatus = await _statusValidator.GetInitialStatusAsync(OrderTypeMappings.ToDocTypeCode(order.OrderType))
                    ?? throw new Exception("未找到入库单的初始状态配置");

                // 补全物料信息
                await FillMaterialInfoAsync(dto.Lines);

                for (int i = 0; i < dto.Lines.Count; i++)
                {
                    dto.Lines[i].LineNo = i + 1;
                }

                // 通过导航属性一次性插入主表 + 从表
                order.Items = dto.Lines;
                await _repository.AddWithNavAsync(order);

                await _unitOfWork.CommitAsync();
                return order.Id;
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> UpdateWithLinesAsync(long id, InboundOrderCreateDto dto)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var existing = await _repository.GetByIdAsync(id);
                if (existing == null)
                    throw new Exception("入库单不存在");

                await _statusValidator.ValidateAllowEditAsync(OrderTypeMappings.ToDocTypeCode(existing.OrderType), existing.OrderStatus);

                var order = dto.Order;
                order.Id = id;
                order.TotalLines = dto.Lines.Count;

                await _repository.UpdateAsync(order);

                // 删除旧明细，重新插入（全量替换）
                await _lineRepository.DeleteAsync(l => l.OrderId == id);

                // 补全物料信息
                await FillMaterialInfoAsync(dto.Lines);

                for (int i = 0; i < dto.Lines.Count; i++)
                {
                    dto.Lines[i].OrderId = id;
                    dto.Lines[i].LineNo = i + 1;
                }

                if (dto.Lines.Count > 0)
                    await _lineRepository.AddAsync(dto.Lines);

                await _unitOfWork.CommitAsync();
                return true;
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<InboundOrderCreateDto?> GetDetailAsync(long id)
        {
            var order = await _repository.GetByIdAsync(id);
            if (order == null) return null;

            var lines = await _lineRepository.GetListAsync(l => l.OrderId == id)
                ?? new List<InboundOrderLine>();

            var result = new InboundOrderCreateDto { Order = order, Lines = lines };

            // 反序列化单据头扩展字段
            result.ExtDataFlattened = _extFieldService.DeserializeExtData(order.ExtData);

            // 反序列化行级扩展字段
            result.LineExtDataFlattened = new Dictionary<long, Dictionary<string, object?>>();
            foreach (var line in lines)
            {
                if (!string.IsNullOrEmpty(line.ExtData))
                {
                    result.LineExtDataFlattened[line.Id] = _extFieldService.DeserializeExtData(line.ExtData);
                }
            }

            return result;
        }

        /// <inheritdoc />
        public async Task<ServiceResult> ReceiveAsync(long orderId, List<ReceiveLineDto> receiveLines)
        {
            if (receiveLines == null || receiveLines.Count == 0)
                return ServiceResult.Fail(WMSErrorMessages.GetMessage(RECEIVE_DATA_EMPTY));

            // 收货涉及"更新单头状态 + 逐行收货 + 刷新状态"多步写，必须包裹在单一事务中，
            // 否则中途某行失败会留下"半收货"脏数据，且已收货行会被拒收无法重试。
            // 仅在没有外层事务时自行开启：ReceiveAndBindAsync 等调用方已开启事务时加入其中，
            // 避免重复开启造成嵌套事务（嵌套层的回滚信号会在出栈时丢失，见 SqlSugarDbContext）。
            var ownsTransaction = !_unitOfWork.HasActiveTransaction;
            if (ownsTransaction)
                await _unitOfWork.BeginTransactionAsync();

            try
            {
                var order = await _repository.GetByIdAsync(orderId);
                if (order == null)
                    return await FailReceiveAsync(WMSErrorMessages.GetMessage(INBOUND_NOT_EXIST), ownsTransaction);

                // Step 1: 验证入库单状态、目标状态、明细存在性
                var (targetStatus, validateError) = await ValidateReceiveAsync(order, receiveLines);
                if (validateError != null)
                {
                    _loggerService.LogInfo(validateError);
                    return await FailReceiveAsync(validateError, ownsTransaction);
                }
                   

                // Step 2: 更新入库单状态（如有变化）
                var statusChanged = order.OrderStatus != targetStatus;
                if (statusChanged)
                {
                    order.SetStatus(targetStatus);
                    await _repository.UpdateAsync(order);
                }

                // Step 3: 获取明细行并执行收货
                var lineIds = receiveLines.Select(r => r.LineId).Distinct().ToList();
                var lines = await _lineRepository.GetListAsync(l => lineIds.Contains(l.Id));

                var processError = await ProcessReceiveLinesAsync(lines, receiveLines);
                if (processError != null)
                    return await FailReceiveAsync(processError, ownsTransaction);

                // Step 4: 刷新订单状态
                await RefreshOrderStatusAfterReceiveAsync(orderId, order);

                if (ownsTransaction)
                    await _unitOfWork.CommitAsync();

                return ServiceResult.Ok(WMSErrorMessages.GetMessage(INBOUND_RECEIVE_SUCCESS));
            }
            catch (Exception ex)
            {
                _loggerService.LogError(ex, ex.Message);
                if (ownsTransaction)
                    await _unitOfWork.RollbackAsync();
                return ServiceResult.Fail(WMSErrorMessages.GetMessage(RECEIVE_FAILED, ex.Message));
            }
        }

        /// <summary>
        /// 收货过程中业务校验失败时统一回滚（仅当本方法拥有事务时）并返回失败结果。
        /// 调用方已开启事务时由调用方根据返回的 Success 决定回滚，此处不重复处理。
        /// </summary>
        private async Task<ServiceResult> FailReceiveAsync(string message, bool ownsTransaction)
        {
            if (ownsTransaction)
                await _unitOfWork.RollbackAsync();
            return ServiceResult.Fail(message);
        }

        /// <summary>
        /// 验证入库单状态、目标状态、明细存在性
        /// </summary>
        private async Task<(string? TargetStatus, string? Error)> ValidateReceiveAsync(
            InboundOrder order, List<ReceiveLineDto> receiveLines)
        {
            // 状态校验（实体方法）
            var validation = order.ValidateCanReceive();
            if (!validation.IsValid)
                return (null, validation.Errors.First());

            // 确定目标状态：从配置表获取允许流转状态
            var docTypeCode = OrderTypeMappings.ToDocTypeCode(order.OrderType);
            var allowedTransitions = await _statusValidator.GetAllowedTransitionsAsync(
                docTypeCode, order.OrderStatus);

            // 如果当前是初始状态（如 DRAFT），需要流转到收货中状态
            var initialStatus = await _statusValidator.GetInitialStatusAsync(docTypeCode);
            var targetStatus = order.OrderStatus;
            if (order.OrderStatus == initialStatus && allowedTransitions.Count > 0)
            {
                targetStatus = allowedTransitions.FirstOrDefault(s => s != BizConstants.InboundOrderStatus.CANCELLED) ?? order.OrderStatus;
            }

            // 状态流转校验（配置表）
            try
            {
                await _statusValidator.ValidateTransitionAsync(
                    docTypeCode, order.OrderStatus, targetStatus);
            }
            catch (InvalidOperationException ex)
            {
                return (null, ex.Message);
            }

            // 获取所有明细行，校验存在性
            var lineIds = receiveLines.Select(r => r.LineId).Distinct().ToList();
            var lines = await _lineRepository.GetListAsync(l => lineIds.Contains(l.Id));
            if (lines == null || lines.Count == 0)
                return (null, "未找到对应的入库单明细");

            return (targetStatus, null);
        }

        /// <summary>
        /// 遍历收货行并更新
        /// </summary>
        private async Task<string?> ProcessReceiveLinesAsync(
            List<InboundOrderLine> lines, List<ReceiveLineDto> receiveLines)
        {
            foreach (var receiveLine in receiveLines)
            {
                var line = lines.FirstOrDefault(l => l.Id == receiveLine.LineId);
                if (line == null)
                    return $"明细行ID {receiveLine.LineId} 不存在";

                // 校验行状态（实体方法）
                if (!line.CanReceive())
                    return $"明细行 {line.LineNo} 已全部收货，不能重复收货";

                // 校验收货数量（实体方法）
                var validation = line.ValidateReceiveQty(receiveLine.ReceiveQty);
                if (!validation.IsValid)
                    return validation.Errors.First();

                // 执行收货（实体方法）
                line.Receive(receiveLine.ReceiveQty, receiveLine.BatchNo, receiveLine.ManufactureDate, receiveLine.ExpiryDate);
                await _lineRepository.UpdateAsync(line);
            }

            return null;
        }

        /// <summary>
        /// 刷新订单状态（根据所有明细行的收货情况）
        /// </summary>
        private async Task RefreshOrderStatusAfterReceiveAsync(long orderId, InboundOrder order)
        {
            var docTypeCode = OrderTypeMappings.ToDocTypeCode(order.OrderType);
            var allLines = await _lineRepository.GetListAsync(l => l.OrderId == orderId);
            var previousStatus = order.OrderStatus;

            // 从配置表获取已收货状态码
            var nextTransitions = await _statusValidator.GetAllowedTransitionsAsync(
                docTypeCode, order.OrderStatus);
            var receivedStatus = nextTransitions.FirstOrDefault(s => s == BizConstants.InboundOrderStatus.RECEIVED)
                ?? BizConstants.InboundOrderStatus.RECEIVED;

            order.RefreshStatusFromLines(allLines, receivedStatus);
            if (order.OrderStatus != previousStatus)
            {
                await _statusValidator.ValidateTransitionAsync(
                    docTypeCode, previousStatus, order.OrderStatus);
                await _repository.UpdateAsync(order);
            }
        }

        /// <inheritdoc />
        public async Task<ServiceResult> ReceiveAndBindAsync(ReceiveAndBindDto dto)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // 1. 执行收货
                var receiveResult = await ReceiveAsync(dto.InboundOrderId, dto.ReceiveLines);
                if (!receiveResult.Success)
                {
                    await _unitOfWork.RollbackAsync();
                    return receiveResult;
                }

                // 2. 如果有组盘数据，执行组盘
                if (dto.ContainerBinds != null && dto.ContainerBinds.Count > 0)
                {
                    // 补充入库单号到组盘记录
                    var order = await _repository.GetByIdAsync(dto.InboundOrderId);

                    var bindResult = await _containerBindService.ContainerBindAsync(dto.ContainerBinds);
                    if (!bindResult.Success)
                    {
                        await _unitOfWork.RollbackAsync();
                        return bindResult;
                    }
                }

                await _unitOfWork.CommitAsync();
                return ServiceResult.Ok(WMSErrorMessages.GetMessage(RECEIVE_AND_BIND_SUCCESS));
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                return ServiceResult.Fail(WMSErrorMessages.GetMessage(RECEIVE_AND_BIND_FAILED, ex.Message));
            }
        }

        /// <inheritdoc />
        public async Task<List<InboundContainerBindHeader>> GetContainerBindsAsync(long orderId)
        {
            return await _containerBindService.GetByOrderIdAsync(orderId);
        }

        // ==================== 状态配置驱动 ====================

        /// <summary>
        /// 通过契约批量查询物料信息，补全明细行的 MaterialId、MaterialName、UnitId
        /// </summary>
        private async Task FillMaterialInfoAsync(List<InboundOrderLine> lines)
        {
            var materialCodes = lines.Select(l => l.MaterialCode).Where(c => !string.IsNullOrEmpty(c)).Distinct().ToList();
            var materialDict = materialCodes.Count > 0
                ? (await _materialContract.GetByCodesAsync(materialCodes)).ToDictionary(m => m.MaterialCode)
                : new Dictionary<string, KH.WMS.Contracts.BaseData.MaterialInfo>();

            foreach (var line in lines)
            {
                line.Id = 0;
                if (materialDict.TryGetValue(line.MaterialCode ?? string.Empty, out var material))
                {
                    line.MaterialId = material.Id;
                    if (string.IsNullOrEmpty(line.MaterialName))
                        line.MaterialName = material.MaterialName;
                    if (line.UnitId == null)
                        line.UnitId = material.BaseUnitId;
                }
            }
        }

        /// <summary>
        /// 分页查询后处理：为每行计算 AllowedActions
        /// </summary>
        protected override async Task<List<InboundOrder>> AfterQueryAsync(AdvancedQueryRequestDto query, List<InboundOrder> items)
        {
            items = await base.AfterQueryAsync(query, items);
            await ComputeAllowedActionsAsync(items);
            return items;
        }

        /// <summary>
        /// 根据状态配置为每条订单计算允许的操作列表
        /// </summary>
        private async Task ComputeAllowedActionsAsync(List<InboundOrder> orders)
        {
            foreach (var order in orders)
            {
                var docTypeCode = OrderTypeMappings.TryToDocTypeCode(order.OrderType);
                if (docTypeCode == null) continue;
                var config = await _statusValidator.GetStatusConfigAsync(docTypeCode, order.OrderStatus);
                if (config == null) continue;

                var actions = new List<string>();

                // 编辑/删除：直接取配置
                if (config.AllowEdit == BizConstants.BoolFlag.YES) actions.Add(BizConstants.DocActions.EDIT);
                if (config.AllowDelete == BizConstants.BoolFlag.YES) actions.Add(BizConstants.DocActions.DELETE);

                // 从 NextStatuses 解析可流转目标
                var nextList = ParseNextStatuses(config.NextStatuses);

                // 收货：NextStatuses 包含 RECEIVING，或当前已处于 RECEIVING 状态（可继续收货）
                if (nextList.Contains(BizConstants.InboundOrderStatus.RECEIVING)
                    || order.OrderStatus == BizConstants.InboundOrderStatus.RECEIVING)
                    actions.Add(BizConstants.DocActions.RECEIVE);

                // 组盘：NextStatuses 包含 BOUND，或当前已处于 RECEIVED 状态
                if (nextList.Contains(BizConstants.InboundOrderStatus.BOUND)
                    || order.OrderStatus == BizConstants.InboundOrderStatus.RECEIVED)
                    actions.Add(BizConstants.DocActions.BIND);

                order.AllowedActions = actions;
            }
        }

        private static List<string> ParseNextStatuses(string? json)
        {
            if (string.IsNullOrWhiteSpace(json)) return new List<string>();
            try { return JsonSerializer.Deserialize<List<string>>(json) ?? new(); }
            catch { return new List<string>(); }
        }
    }
}
