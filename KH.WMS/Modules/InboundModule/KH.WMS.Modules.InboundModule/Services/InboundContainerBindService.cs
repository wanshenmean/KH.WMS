using KH.WMS.Algorithms.Strategy;
using KH.WMS.Algorithms.Strategy.Constants;
using KH.WMS.Algorithms.Strategy.Enums;
using KH.WMS.Algorithms.Strategy.Interfaces;
using KH.WMS.Algorithms.Strategy.QueryServices;
using KH.WMS.Algorithms.Strategy.Strategies;
using KH.WMS.Config.Abstractions;
using KH.WMS.Contracts.Container;
using KH.WMS.Contracts.Inbound;
using KH.WMS.Contracts.Tasks;
using KH.WMS.Contracts.Warehouse;
using KH.WMS.Core.Database.Repositories;
using KH.WMS.Core.Database.UnitOfWorks;
using KH.WMS.Core.DependencyInjection.ServiceLifetimes;
using KH.WMS.Core.Logging.WMSError;
using KH.WMS.Core.Models;
using KH.WMS.Core.Services;
using KH.WMS.Core.Validation;
using KH.WMS.Entities.BaseData;
using KH.WMS.Entities.Constants;
using static KH.WMS.Core.Logging.WMSError.WMSErrorCodes;
using KH.WMS.Entities.Inbound;
using KH.WMS.Entities.Inventory;
using KH.WMS.Entities.Task;
using KH.WMS.Modules.InboundModule.DTOs;
using SqlSugar;

namespace KH.WMS.Modules.InboundModule
{
    /// <summary>
    /// 上架策略输入参数
    /// </summary>
    internal class PutawayStrategyInput
    {
        public long WarehouseId { get; init; }
        public string BusinessCode { get; init; } = string.Empty;
        public long MaterialId { get; init; }
        public string DocType { get; init; } = string.Empty;
        public string? InboundStationCode { get; init; }
    }

    /// <summary>
    /// 上架策略输出（含接驳口信息）
    /// </summary>
    internal class PutawayStrategyOutput
    {
        public PutawayResult PutawayResult { get; init; } = null!;
        public long? TransferPointId { get; init; }
        public string? TransferPointCode { get; init; }
    }
    [RegisteredService(ServiceType = typeof(IInboundContainerBindService))]
    public class InboundContainerBindService(
        IRepository<InboundContainerBindHeader, long> headerRepository,
        IRepository<InboundContainerBindDetail, long> detailRepository,
        IRepository<InboundOrder, long> orderRepository,
        IRepository<InboundOrderLine, long> orderLineRepository,
        ISqlSugarClient db,
        IContainerContract containerContract,
        ITaskContract taskContract,
        ILocationContract locationContract,
        IInboundOrderContract inboundOrderContract,
        IPolicyEngine policyEngine,
        ILocationQueryService locationQueryService,
        IWarehouseQueryService warehouseQueryService,
        IDocumentStatusValidatorContract statusValidator,
        IUnitOfWork unitOfWork,
        IDetailSaveService detailSaveService)
        : CrudService<InboundContainerBindHeader>(headerRepository, unitOfWork, detailSaveService), IInboundContainerBindService
    {
        private readonly IRepository<InboundContainerBindHeader, long> _headerRepository = headerRepository;
        private readonly IRepository<InboundContainerBindDetail, long> _detailRepository = detailRepository;
        private readonly IRepository<InboundOrder, long> _orderRepository = orderRepository;
        private readonly IRepository<InboundOrderLine, long> _orderLineRepository = orderLineRepository;
        private readonly IContainerContract _containerContract = containerContract;
        private readonly ITaskContract _taskContract = taskContract;
        private readonly ILocationContract _locationContract = locationContract;
        private readonly IInboundOrderContract _inboundOrderContract = inboundOrderContract;
        private readonly IPolicyEngine _policyEngine = policyEngine;
        private readonly ILocationQueryService _locationQueryService = locationQueryService;
        private readonly IWarehouseQueryService _warehouseQueryService = warehouseQueryService;
        private readonly IDocumentStatusValidatorContract _statusValidator = statusValidator;

        /// <inheritdoc />
        [ConfigValidation(ValidatorCodes.BIND_DATA_NOT_EMPTY)]
        [ConfigValidation(ValidatorCodes.BIND_QUANTITY)]
        [ConfigValidation(ValidatorCodes.BATCH_NO_REQUIRED)]
        [ConfigValidation(ValidatorCodes.EXPIRY_DATE_REQUIRED)]
        [ConfigValidation(ValidatorCodes.MIXED_MATERIAL)]
        [ConfigValidation(ValidatorCodes.MIXED_BATCH)]
        public async Task<ServiceResult> ContainerBindAsync(List<ContainerBindDto> binds)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // 准备数据：注册容器 + 验证明细 + 查询订单头
                var containerCodes = binds.Select(b => b.ContainerCode).Distinct().ToList();
                var lineIds = binds.Select(b => b.InboundOrderLineId).Distinct().ToList();

                await _containerContract.RegisterContainersAsync(containerCodes);

                // 对容器记录加更新锁，序列化同一容器的并发组盘（校验-写入非原子，靠事务内行锁串行化）：
                // 第一个请求提交后留下 BOUND 组盘记录，第二个请求拿到锁后校验会被拒绝。
                await db.Queryable<MdContainer>()
                    .Where(c => containerCodes.Contains(c.ContainerNo))
                    .With(SqlWith.UpdLock)
                    .ToListAsync();

                // 校验容器可用性
                var containerError = await ValidateContainerAvailableAsync(containerCodes);
                if (containerError != null)
                    return ServiceResult.Fail(containerError);

                // 校验订单明细行是否存在
                var lines = await _orderLineRepository.GetListAsync(l => lineIds.Contains(l.Id));
                var lineError = ValidateOrderLinesExistAsync(lineIds, lines);
                if (lineError != null)
                    return ServiceResult.Fail(lineError);

                var orderIds = lines.Select(l => l.OrderId).Distinct().ToList();
                var orders = await _orderRepository.GetListAsync(o => orderIds.Contains(o.Id));

                // 按容器分组构建头+明细实体并插入
                var (headers, totalDetails) = BuildBindHeaders(binds, lines, orders);
                foreach (var header in headers)
                    await _headerRepository.AddWithNavAsync(header);

                // 更新容器状态为 IN_USE
                var usedContainerNos = binds.Select(b => b.ContainerCode).Distinct().ToList();
                await _containerContract.UpdateStatusAsync(usedContainerNos, BizConstants.ContainerStatus.IN_USE);

                // 检查入库单是否全部组盘完成并更新状态
                await CheckAndMarkOrdersFullyBoundAsync(orderIds, orders);

                await _unitOfWork.CommitAsync();
                return ServiceResult.Ok(WMSErrorMessages.GetMessage(CONTAINER_BIND_CREATED, headers.Count, totalDetails));
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                return ServiceResult.Fail(WMSErrorMessages.GetMessage(CONTAINER_BIND_FAILED, ex.Message));
            }
        }

        /// <summary>
        /// 校验容器可用性：无活跃组盘 + 无库存 + 无活跃任务
        /// </summary>
        private async Task<string?> ValidateContainerAvailableAsync(List<string> containerCodes)
        {
            foreach (var code in containerCodes)
            {
                var bindCheck = await _headerRepository.GetFirstOrDefaultAsync(
                    h => h.ContainerCode == code
                        && (h.BindStatus == BizConstants.BindStatus.BOUND || h.BindStatus == BizConstants.BindStatus.PUTTING_AWAY));
                if (bindCheck != null)
                    return WMSErrorMessages.GetMessage(CONTAINER_HAS_ACTIVE_BIND, code);

                var hasInventory = await db.Queryable<InvInventoryHeader>()
                    .AnyAsync(h => h.ContainerCode == code);
                if (hasInventory)
                    return WMSErrorMessages.GetMessage(CONTAINER_HAS_INVENTORY, code);

                var hasActiveTask = await db.Queryable<TaskHeader>()
                    .AnyAsync(t => t.ContainerNo == code
                        && (t.TaskStatus == BizConstants.TaskStatus.PENDING || t.TaskStatus == BizConstants.TaskStatus.IN_PROGRESS));
                if (hasActiveTask)
                    return WMSErrorMessages.GetMessage(CONTAINER_HAS_ACTIVE_TASK, code);
            }
            return null;
        }

        /// <summary>
        /// 校验订单明细行是否全部存在
        /// </summary>
        private static string? ValidateOrderLinesExistAsync(List<long> lineIds, List<InboundOrderLine> lines)
        {
            var missingLines = lineIds.Except(lines.Select(l => l.Id)).ToList();
            if (missingLines.Count > 0)
                return WMSErrorMessages.GetMessage(CONTAINER_BIND_DETAIL_NOT_EXIST, string.Join(", ", missingLines));
            return null;
        }

        /// <summary>
        /// 按容器分组构建组盘头+明细实体
        /// </summary>
        private static (List<InboundContainerBindHeader> Headers, int TotalDetails) BuildBindHeaders(
            List<ContainerBindDto> binds, List<InboundOrderLine> lines, List<InboundOrder> orders)
        {
            var headers = new List<InboundContainerBindHeader>();
            var totalDetails = 0;

            foreach (var group in binds.GroupBy(b => b.ContainerCode))
            {
                var firstBind = group.First();
                var firstLine = lines.First(l => l.Id == firstBind.InboundOrderLineId);
                var order = orders.First(o => o.Id == firstLine.OrderId);

                var header = new InboundContainerBindHeader
                {
                    ContainerCode = group.Key,
                    BindStatus = BizConstants.BindStatus.BOUND,
                    WarehouseId = order.WarehouseId,
                    DetailCount = group.Count(),
                    SourceType = BizConstants.SourceTypes.INBOUND,
                    SourceDocNo = order.OrderNo,
                    InboundOrderId = firstLine.OrderId,
                    QualityStatus = firstLine.QualityStatus,
                    BindTime = DateTime.Now,
                };
                header.Details = group.Select(bind =>
                {
                    var line = lines.First(l => l.Id == bind.InboundOrderLineId);
                    return new InboundContainerBindDetail
                    {
                        InboundOrderLineId = bind.InboundOrderLineId,
                        MaterialId = line.MaterialId,
                        MaterialCode = line.MaterialCode,
                        MaterialName = line.MaterialName,
                        Qty = bind.Qty,
                        UnitId = line.UnitId,
                        BatchNo = bind.BatchNo ?? line.BatchNo,
                        ProductionDate = bind.ProductionDate ?? line.ManufactureDate,
                        ExpiryDate = bind.ExpiryDate ?? line.ExpiryDate,
                    };
                }).ToList();

                headers.Add(header);
                totalDetails += header.Details.Count;
            }

            return (headers, totalDetails);
        }

        /// <summary>
        /// 检查入库单是否所有明细行都已组盘完成，若是则更新订单状态
        /// </summary>
        private async Task CheckAndMarkOrdersFullyBoundAsync(List<long> orderIds, List<InboundOrder> orders)
        {
            var allOrderLines = await _orderLineRepository.GetListAsync(l => orderIds.Contains(l.OrderId));
            var allLineIds = allOrderLines.Select(l => l.Id).ToList();

            var allBoundQty = await _detailRepository.AsQueryable()
                .InnerJoin<InboundContainerBindHeader>((d, h) => d.HeaderId == h.Id)
                .Where((d, h) => d.InboundOrderLineId != null && allLineIds.Contains(d.InboundOrderLineId.Value) && h.BindStatus == BizConstants.BindStatus.BOUND)
                .GroupBy(d => d.InboundOrderLineId)
                .Select(d => new { LineId = d.InboundOrderLineId, TotalQty = SqlFunc.AggregateSum(d.Qty) })
                .ToListAsync();

            var boundQtyDict = allBoundQty.ToDictionary(b => b.LineId!.Value, b => b.TotalQty);

            foreach (var group in allOrderLines.GroupBy(l => l.OrderId))
            {
                var order = orders.First(o => o.Id == group.Key);
                if (!order.IsFullyBound(group.ToList(), boundQtyDict))
                    continue;

                var docTypeCode = OrderTypeMappings.ToDocTypeCode(order.OrderType);
                var allowedNext = await _statusValidator.GetAllowedTransitionsAsync(docTypeCode, order.OrderStatus);
                var boundStatus = allowedNext.FirstOrDefault(s => s == BizConstants.InboundOrderStatus.BOUND || s == BizConstants.InboundOrderStatus.COMPLETED)
                    ?? BizConstants.InboundOrderStatus.BOUND;

                await _statusValidator.ValidateTransitionAsync(docTypeCode, order.OrderStatus, boundStatus);
                order.MarkAsBound(boundStatus);
                await _orderRepository.UpdateAsync(order);
            }
        }

        /// <inheritdoc />
        public async Task<List<InboundContainerBindHeader>> GetByOrderIdAsync(long inboundOrderId)
        {
            var list = await _headerRepository.GetListWithNavAsync(h => h.InboundOrderId == inboundOrderId);
            return list.OrderBy(h => h.BindTime).ToList();
        }

        /// <inheritdoc />
        public async Task<List<InboundContainerBindHeader>> GetByContainerCodeAsync(string containerCode)
        {
            var list = await _headerRepository.GetListWithNavAsync(h => h.ContainerCode == containerCode && h.BindStatus == BizConstants.BindStatus.BOUND);
            return list.OrderBy(h => h.BindTime).ToList();
        }

        /// <inheritdoc />
        public async Task<ServiceResult> CancelBindAsync(long headerId)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // 查询组盘头（含明细）
                var header = await _headerRepository.GetByIdWithNavAsync(headerId);
                if (header == null)
                    return ServiceResult.Fail(WMSErrorMessages.GetMessage(CONTAINER_BIND_RECORD_NOT_EXIST, headerId));

                // 仅"已组盘"状态可取消（已进入上架流程的不允许）
                if (header.BindStatus != BizConstants.BindStatus.BOUND)
                    return ServiceResult.Fail($"容器 {header.ContainerCode} 当前状态为 {header.BindStatus}，仅已组盘状态可取消");

                // 1. 组盘状态置为已取消
                header.BindStatus = BizConstants.BindStatus.CANCELLED;
                await _headerRepository.UpdateAsync(header);

                // 2. 容器状态回滚：该容器若无其他活跃组盘，则恢复为空容器
                var hasOtherActiveBind = await _headerRepository.GetFirstOrDefaultAsync(
                    h => h.ContainerCode == header.ContainerCode
                        && h.Id != header.Id
                        && (h.BindStatus == BizConstants.BindStatus.BOUND
                            || h.BindStatus == BizConstants.BindStatus.PUTTING_AWAY));
                if (hasOtherActiveBind == null)
                {
                    await _containerContract.UpdateStatusAsync(
                        new List<string> { header.ContainerCode }, BizConstants.ContainerStatus.EMPTY);
                }

                // 3. 回滚因组盘被推进的入库单状态
                await RollbackOrderStatusAfterCancelAsync(header);

                await _unitOfWork.CommitAsync();
                return ServiceResult.Ok($"取消组盘成功，容器 {header.ContainerCode} 已释放");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                return ServiceResult.Fail($"取消组盘失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 取消组盘后，重新评估受影响入库单的"全部组盘"状态，若不再全部组盘则回退订单状态。
        /// 组盘推进的订单状态（BOUND/COMPLETED）回退到组盘前：需收货类型→已收货，不需收货类型→初始状态。
        /// </summary>
        private async Task RollbackOrderStatusAfterCancelAsync(InboundContainerBindHeader cancelledHeader)
        {
            // 从被取消组盘的明细反查关联的订单行与订单
            var lineIds = (cancelledHeader.Details ?? new List<InboundContainerBindDetail>())
                .Where(d => d.InboundOrderLineId.HasValue)
                .Select(d => d.InboundOrderLineId!.Value)
                .Distinct().ToList();
            if (lineIds.Count == 0) return;

            var lines = await _orderLineRepository.GetListAsync(l => lineIds.Contains(l.Id));
            var orderIds = lines.Select(l => l.OrderId).Distinct().ToList();
            if (orderIds.Count == 0) return;

            var orders = await _orderRepository.GetListAsync(o => orderIds.Contains(o.Id));
            var allOrderLines = await _orderLineRepository.GetListAsync(l => orderIds.Contains(l.OrderId));
            var allLineIds = allOrderLines.Select(l => l.Id).ToList();

            // 重新统计各订单行已组盘数量（仅统计 BOUND 状态的组盘明细，已取消的不计入）
            var allBoundQty = await _detailRepository.AsQueryable()
                .InnerJoin<InboundContainerBindHeader>((d, h) => d.HeaderId == h.Id)
                .Where((d, h) => d.InboundOrderLineId != null
                    && allLineIds.Contains(d.InboundOrderLineId.Value)
                    && h.BindStatus == BizConstants.BindStatus.BOUND)
                .GroupBy(d => d.InboundOrderLineId)
                .Select(d => new { LineId = d.InboundOrderLineId, TotalQty = SqlFunc.AggregateSum(d.Qty) })
                .ToListAsync();
            var boundQtyDict = allBoundQty.ToDictionary(b => b.LineId!.Value, b => b.TotalQty);

            foreach (var group in allOrderLines.GroupBy(l => l.OrderId))
            {
                var order = orders.First(o => o.Id == group.Key);

                // 仅处理因组盘被推进的状态（BOUND/COMPLETED）
                if (order.OrderStatus != BizConstants.InboundOrderStatus.BOUND
                    && order.OrderStatus != BizConstants.InboundOrderStatus.COMPLETED)
                    continue;

                // 仍全部组盘则无需回退
                if (order.IsFullyBound(group.ToList(), boundQtyDict))
                    continue;

                // 回退到组盘前的状态：需收货类型→已收货，不需收货类型→初始状态
                var docTypeCode = OrderTypeMappings.ToDocTypeCode(order.OrderType);
                var needReceive = BizConstants.OrderTypes.IsReceiveRequired(order.OrderType);
                var rollbackStatus = needReceive
                    ? BizConstants.InboundOrderStatus.RECEIVED
                    : (await _statusValidator.GetInitialStatusAsync(docTypeCode) ?? BizConstants.InboundOrderStatus.DRAFT);

                order.SetStatus(rollbackStatus);
                await _orderRepository.UpdateAsync(order);
            }
        }

        /// <inheritdoc />
        public async Task<ServiceResult> RequestPutAwayAsync(List<long> headerIds)
        {
            if (headerIds == null || headerIds.Count == 0)
                return ServiceResult.Fail(WMSErrorMessages.GetMessage(CONTAINER_BIND_SELECT_REQUIRED));

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // 查询组盘头（含明细）
                var headers = new List<InboundContainerBindHeader>();
                foreach (var id in headerIds)
                {
                    var header = await _headerRepository.GetByIdWithNavAsync(id);
                    if (header == null)
                        return ServiceResult.Fail(WMSErrorMessages.GetMessage(CONTAINER_BIND_RECORD_NOT_EXIST, id));

                    if (header.BindStatus != BizConstants.BindStatus.BOUND)
                        return ServiceResult.Fail(WMSErrorMessages.GetMessage(CONTAINER_BIND_STATUS_ERROR, header.ContainerCode, header.BindStatus));

                    headers.Add(header);
                }

                var createdTasks = 0;
                foreach (var header in headers)
                {
                    var taskResult = await CreatePutawayTaskFromBindAsync(header);
                    if (!taskResult.Success)
                        return ServiceResult.Fail(taskResult.Message);
                    createdTasks++;
                }

                // 更新容器状态为 MOVING（上架中移动）
                var containerNos = headers.Select(h => h.ContainerCode).Distinct().ToList();
                await _containerContract.UpdateStatusAsync(containerNos, BizConstants.ContainerStatus.MOVING);

                await _unitOfWork.CommitAsync();
                return ServiceResult.Ok(WMSErrorMessages.GetMessage(PUTAWAY_TASK_CREATED, createdTasks));
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                return ServiceResult.Fail(WMSErrorMessages.GetMessage(CONTAINER_BIND_PUTAWAY_FAILED, ex.Message));
            }
        }

        /// <inheritdoc />
        public async Task<ServiceResult> RequestPutAwayByWcsAsync(string containerCode, string portCode)
        {
            if (string.IsNullOrWhiteSpace(containerCode))
                return ServiceResult.Fail(WMSErrorMessages.GetMessage(CONTAINER_CODE_EMPTY));
            if (string.IsNullOrWhiteSpace(portCode))
                return ServiceResult.Fail(WMSErrorMessages.GetMessage(INBOUND_STATION_CODE_EMPTY));

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // 查询该容器的组盘记录（BOUND状态）
                var headers = await _headerRepository.GetListWithNavAsync(
                    h => h.ContainerCode == containerCode && h.BindStatus == BizConstants.BindStatus.BOUND);

                if (headers == null || headers.Count == 0)
                    return ServiceResult.Fail(WMSErrorMessages.GetMessage(CONTAINER_NO_BOUND_RECORD, containerCode));

                var header = headers.First();

                // 幂等：该容器已有进行中的上架任务，WCS 重复提交不重复创建
                var hasActivePutaway = await db.Queryable<TaskHeader>()
                    .Where(t => t.ContainerNo == containerCode
                        && t.TaskType == BizConstants.TaskTypes.PUTAWAY
                        && (t.TaskStatus == BizConstants.TaskStatus.PENDING || t.TaskStatus == BizConstants.TaskStatus.IN_PROGRESS))
                    .AnyAsync();
                if (hasActivePutaway)
                {
                    await _unitOfWork.RollbackAsync();
                    return ServiceResult.Ok($"容器 {containerCode} 已有进行中的上架任务，无需重复提交");
                }

                var wcsResult = await CreatePutawayTaskFromBindAsync(header, portCode);
                if (!wcsResult.Success)
                    return ServiceResult.Fail(wcsResult.Message);

                // 更新容器状态为 MOVING
                await _containerContract.UpdateStatusAsync(
                    new List<string> { containerCode }, BizConstants.ContainerStatus.MOVING);

                await _unitOfWork.CommitAsync();
                return ServiceResult.Ok(wcsResult.Message);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                return ServiceResult.Fail(WMSErrorMessages.GetMessage(CONTAINER_BIND_WCS_PUTAWAY_FAILED, ex.Message));
            }
        }

        /// <summary>
        /// 从组盘头创建上架任务的公共逻辑
        /// Phase 1: 执行上架策略 → 查找接驳口 → 创建任务 → 更新组盘状态
        /// Phase 2（货位分配）由 WCS 在接驳口调用 /api/task-header/allocate-putaway-location/{id}
        /// </summary>
        private async Task<ServiceResult<string>> CreatePutawayTaskFromBindAsync(
            InboundContainerBindHeader header, string? inboundStationCode = null)
        {
            var warehouseId = header.WarehouseId ?? 0;
            if (warehouseId == 0)
                return ServiceResult<string>.Fail(WMSErrorMessages.GetMessage(CONTAINER_BIND_NO_WAREHOUSE, header.ContainerCode));

            var firstDetail = header.Details?.FirstOrDefault();
            var containerCode = header.ContainerCode;

            // 执行上架策略
            var strategyInput = new PutawayStrategyInput
            {
                WarehouseId = warehouseId,
                BusinessCode = header.SourceDocNo ?? string.Empty,
                MaterialId = firstDetail?.MaterialId ?? 0,
                DocType = BizConstants.SourceTypes.INBOUND,
                InboundStationCode = inboundStationCode,
            };
            var (strategyError, strategyOutput) = await ExecutePutawayStrategyAsync(strategyInput, containerCode);
            if (strategyOutput == null)
                return ServiceResult<string>.Fail(strategyError!);

            // 创建上架任务
            var taskResult = await CreatePutawayTaskAsync(
                header, strategyOutput, inboundStationCode);
            if (!taskResult.Success)
                return ServiceResult<string>.Fail(WMSErrorMessages.GetMessage(CONTAINER_BIND_PUTAWAY_TASK_FAILED, containerCode, taskResult.Message));

            // 通过契约更新组盘头状态为 PUTTING_AWAY
            await _inboundOrderContract.MarkBindAsPuttingAwayAsync(header.Id);

            return ServiceResult<string>.Ok(taskResult.Data, WMSErrorMessages.GetMessage(PUTAWAY_TASK_CREATED_WITH_NO, taskResult.Data));

        }

        // ==================== 策略执行方法 ====================

        /// <summary>
        /// 执行上架策略（确定库区、巷道、入库口），并查找对应接驳口
        /// </summary>
        private async Task<(string? Error, PutawayStrategyOutput? Output)> ExecutePutawayStrategyAsync(
            PutawayStrategyInput input, string containerCode)
        {
            var context = new PolicyContext
            {
                WarehouseId = input.WarehouseId,
                BusinessCode = input.BusinessCode,
                DocType = input.DocType,
                MaterialId = input.MaterialId,
            };

            if (!string.IsNullOrWhiteSpace(input.InboundStationCode))
                context.SetData(StrategyParams.PutawayInput.INBOUND_STATION_CODE, input.InboundStationCode);

            var putawayResult = await _policyEngine.ExecuteAsync(PolicyType.Putaway, context);
            if (!putawayResult.IsSuccess)
                return ($"上架策略执行失败（容器 {containerCode}）: {putawayResult.ErrorMessage}", null);

            var putawayOutput = putawayResult.Output as PutawayResult;
            if (putawayOutput == null)
                return ($"上架策略返回结果为空（容器 {containerCode}）", null);

            // 查找接驳口
            var (transferPointId, transferPointCode) = await FindTransferPointAsync(
                input.WarehouseId, putawayOutput.AisleId);

            return (null, new PutawayStrategyOutput
            {
                PutawayResult = putawayOutput,
                TransferPointId = transferPointId,
                TransferPointCode = transferPointCode,
            });
        }

        // ==================== 辅助方法 ====================

        /// <summary>
        /// 根据巷道查找对应的入库接驳口
        /// </summary>
        private async Task<(long? Id, string? Code)> FindTransferPointAsync(long warehouseId, long? aisleId)
        {
            if (!aisleId.HasValue)
                return (null, null);

            var transferPoint = await _warehouseQueryService.GetInboundTransferPointAsync(warehouseId, aisleId.Value);
            if (transferPoint == null)
                return (null, null);

            return (transferPoint.Id, transferPoint.PointCode);
        }

        /// <summary>
        /// 构建并创建上架任务请求
        /// </summary>
        private async Task<ServiceResult<string>> CreatePutawayTaskAsync(
            InboundContainerBindHeader header, PutawayStrategyOutput strategyOutput, string? inboundStationCode)
        {
            var taskRequest = new KH.WMS.Contracts.Tasks.PutawayTaskRequest
            {
                WarehouseId = header.WarehouseId ?? 0,
                DocId = header.Id,
                DocNo = header.SourceDocNo,
                ContainerNo = header.ContainerCode,
                FromLocationCode = inboundStationCode ?? strategyOutput.PutawayResult.InboundStationCode,
                ToLocationId = strategyOutput.TransferPointId,
                ToLocationCode = strategyOutput.TransferPointCode,
                ToZoneId = strategyOutput.PutawayResult.TargetZoneId,
                Lines = header.Details.Select(d => new KH.WMS.Contracts.Tasks.PutawayTaskLineRequest
                {
                    MaterialId = d.MaterialId,
                    MaterialCode = d.MaterialCode,
                    MaterialName = d.MaterialName,
                    BatchNo = d.BatchNo,
                }).ToList(),
            };

            return await _taskContract.CreatePutawayTaskAsync(taskRequest);
        }

    }
}
