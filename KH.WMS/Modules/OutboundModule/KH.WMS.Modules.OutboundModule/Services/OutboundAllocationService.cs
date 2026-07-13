using SqlSugar;
using KH.WMS.Algorithms.Strategy;
using KH.WMS.Algorithms.Strategy.Constants;
using KH.WMS.Algorithms.Strategy.Enums;
using KH.WMS.Algorithms.Strategy.Interfaces;
using KH.WMS.Algorithms.Strategy.QueryServices;
using KH.WMS.Algorithms.Strategy.Strategies;
using KH.WMS.Config.Abstractions;
using KH.WMS.Contracts.Inventory;
using KH.WMS.Contracts.Outbound;
using KH.WMS.Contracts.Tasks;
using KH.WMS.Core.Database.Repositories;
using KH.WMS.Core.Database.UnitOfWorks;
using KH.WMS.Core.DependencyInjection.ServiceLifetimes;
using KH.WMS.Core.Logging.WMSError;
using KH.WMS.Core.Models;
using KH.WMS.Core.Services;
using static KH.WMS.Core.Logging.WMSError.WMSErrorCodes;
using KH.WMS.Entities.Constants;
using KH.WMS.Entities.Inventory;
using KH.WMS.Entities.Outbound;
using KH.WMS.Entities.Warehouse;
using System.Text.Json;

namespace KH.WMS.Modules.OutboundModule
{
    /// <summary>
    /// 出库分配策略输入参数
    /// </summary>
    internal class OutboundAllocationStrategyInput
    {
        public long WarehouseId { get; init; }
        public string WarehouseCode { get; init; } = string.Empty;
        public string BusinessCode { get; init; } = string.Empty;
        public long MaterialId { get; init; }
        public string MaterialCode { get; init; } = string.Empty;
        public string DocType { get; init; } = string.Empty;
        public decimal RequiredQty { get; init; }
    }

    /// <summary>
    /// 出库分配策略输出
    /// </summary>
    internal class OutboundAllocationStrategyOutput
    {
        public InventoryAllocationResult Result { get; init; } = null!;
        public string StrategyCode { get; init; } = string.Empty;
    }

    /// <summary>
    /// 下架策略输入参数
    /// </summary>
    internal class PickingStrategyInput
    {
        public long WarehouseId { get; init; }
        public string BusinessCode { get; init; } = string.Empty;
        public string DocType { get; init; } = string.Empty;
        public int SourceAisleNo { get; init; }
    }

    /// <summary>
    /// 下架策略输出
    /// </summary>
    internal class PickingStrategyOutput
    {
        public long? DestinationStationId { get; init; }
        public string? DestinationStationCode { get; init; }
        public long? DestinationZoneId { get; init; }
    }

    /// <summary>
    /// 单行出库分配结果
    /// </summary>
    internal class SingleLineAllocationResult
    {
        public string StrategyCode { get; init; } = string.Empty;
        public List<OutboundAllocationDetail> Details { get; init; } = new();
    }

    [RegisteredService(ServiceType = typeof(IOutboundAllocationService))]
    public class OutboundAllocationService(
        IRepository<OutboundAllocationHeader, long> headerRepository,
        IRepository<OutboundAllocationDetail, long> detailRepository,
        IRepository<OutboundOrder, long> orderRepository,
        IRepository<OutboundOrderLine, long> orderLineRepository,
        IRepository<MdWarehouse, long> warehouseRepository,
        IPolicyEngine policyEngine,
        ITaskContract taskContract,
        IDocumentStatusValidatorContract statusValidator,
        IInventoryContract inventoryContract,
        IOutboundContract outboundContract,
        IUnitOfWork unitOfWork,
        IDetailSaveService detailSaveService)
        : CrudService<OutboundAllocationHeader>(headerRepository, unitOfWork, detailSaveService), IOutboundAllocationService
    {
        private readonly IRepository<OutboundAllocationHeader, long> _headerRepository = headerRepository;
        private readonly IRepository<OutboundAllocationDetail, long> _detailRepository = detailRepository;
        private readonly IRepository<OutboundOrder, long> _orderRepository = orderRepository;
        private readonly IRepository<OutboundOrderLine, long> _orderLineRepository = orderLineRepository;
        private readonly IRepository<MdWarehouse, long> _warehouseRepository = warehouseRepository;
        private readonly IPolicyEngine _policyEngine = policyEngine;
        private readonly ITaskContract _taskContract = taskContract;
        private readonly IDocumentStatusValidatorContract _statusValidator = statusValidator;
        private readonly IInventoryContract _inventoryContract = inventoryContract;
        private readonly IOutboundContract _outboundContract = outboundContract;

        /// <inheritdoc />
        public async Task<List<OutboundAllocationHeader>> GetByOrderIdAsync(long outboundOrderId)
        {
            var list = await _headerRepository.GetListWithNavAsync(h => h.OutboundOrderId == outboundOrderId);
            return list.OrderBy(h => h.AllocTime).ToList();
        }

        /// <inheritdoc />
        public async Task<ServiceResult> AllocateAsync(long outboundOrderId)
        {
            // 验证出库单、仓库、明细行
            var (order, validateError) = await ValidateOrderForAllocationAsync(outboundOrderId);
            if (order == null)
                return ServiceResult.Fail(validateError!);

            var lines = await _orderLineRepository.GetListAsync(l => l.OrderId == outboundOrderId);
            if (lines.Count == 0)
                return ServiceResult.Fail(WMSErrorMessages.GetMessage(ALLOCATION_NO_DETAIL));

            var warehouseId = order.WarehouseId ?? 0;
            var warehouse = await _warehouseRepository.GetByIdAsync(warehouseId);
            if (warehouse == null)
                return ServiceResult.Fail(WMSErrorMessages.GetMessage(WAREHOUSE_NOT_EXIST, warehouseId));

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // 并发防护：对出库单行加更新锁并复核状态，序列化同一订单的并发分配。
                // 第一个分配提交后，并发请求拿到锁会看到已释放(RELEASED)状态 → 拒绝，
                // 避免同一订单被并发分配两次（重复生成拣货任务、重复锁定库存）。
                var lockedOrder = await _unitOfWork.DbContext.Db.Queryable<OutboundOrder>()
                    .Where(o => o.Id == outboundOrderId)
                    .With(SqlWith.UpdLock)
                    .FirstAsync();
                if (lockedOrder == null || lockedOrder.OrderStatus == BizConstants.OutboundOrderStatus.RELEASED)
                {
                    await _unitOfWork.RollbackAsync();
                    return ServiceResult.Fail($"订单 {outboundOrderId} 已被分配或当前状态不允许分配");
                }
                order = lockedOrder;

                var allDetails = new List<OutboundAllocationDetail>();
                var executedStrategyCode = string.Empty;

                foreach (var line in lines)
                {
                    var requiredQty = line.OrderedQty ?? 0;
                    if (requiredQty <= 0) continue;

                    var lineResult = await AllocateSingleLineAsync(
                        line, order, warehouseId, warehouse.WarehouseCode,
                        order.OrderNo, order.OrderType);
                    if (!lineResult.Success)
                        return ServiceResult.Fail(lineResult.Message ?? WMSErrorMessages.GetMessage(ALLOCATION_SINGLE_LINE_FAILED));

                    if (string.IsNullOrEmpty(executedStrategyCode))
                        executedStrategyCode = lineResult.Data?.StrategyCode ?? string.Empty;

                    allDetails.AddRange(lineResult.Data?.Details ?? new List<OutboundAllocationDetail>());
                }

                // 创建分配头
                var header = new OutboundAllocationHeader
                {
                    OutboundOrderId = outboundOrderId,
                    OutboundOrderNo = order.OrderNo,
                    AllocStatus = BizConstants.AllocationStatus.ALLOCATED,
                    WarehouseId = order.WarehouseId,
                    StrategyType = executedStrategyCode,
                    TotalLines = allDetails.Count,
                    AllocTime = DateTime.Now,
                    Details = allDetails,
                };

                await _headerRepository.AddWithNavAsync(header);

                // 通过契约更新出库单状态为已释放
                await _outboundContract.UpdateOrderStatusAsync(outboundOrderId, BizConstants.OutboundOrderStatus.RELEASED);

                // 通过契约更新出库单行状态为已分配
                foreach (var line in lines)
                {
                    await _outboundContract.UpdateOrderLineStatusAsync(line.Id, BizConstants.OutboundLineStatus.ALLOCATED);
                }

                await _unitOfWork.CommitAsync();
                return ServiceResult.Ok(WMSErrorMessages.GetMessage(ALLOCATION_SUCCESS, allDetails.Count));
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                return ServiceResult.Fail(WMSErrorMessages.GetMessage(ALLOCATE_FAILED, ex.Message));
            }
        }

        /// <inheritdoc />
        public async Task<ServiceResult> GenerateTasksAsync(long allocationHeaderId)
        {
            // 验证分配头
            var (header, validateError) = await ValidateAllocationHeaderAsync(allocationHeaderId);
            if (header == null)
                return ServiceResult.Fail(validateError!);

            var details = header.Details!;
            var order = await _orderRepository.GetByIdAsync(header.OutboundOrderId);
            if (order == null)
                return ServiceResult.Fail(WMSErrorMessages.GetMessage(OUTBOUND_NOT_EXIST));

            // 查找源库位所在巷道号
            var sourceAisleNo = await GetSourceAisleNoAsync(details.First().LocationId);

            // 执行下架策略
            var pickingInput = new PickingStrategyInput
            {
                WarehouseId = header.WarehouseId ?? 0,
                BusinessCode = header.OutboundOrderNo,
                DocType = order.OrderType,
                SourceAisleNo = sourceAisleNo,
            };
            var (pickingError, pickingOutput) = await ExecutePickingStrategyAsync(pickingInput);
            if (pickingOutput == null)
                return ServiceResult.Fail(pickingError!);

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // 按容器分组生成拣货任务
                var groupedByContainer = details
                    .GroupBy(d => d.ContainerCode ?? string.Empty)
                    .ToList();

                var taskCount = await CreatePickingTasksByContainerAsync(header, groupedByContainer, pickingOutput);

                // 回写 TaskHeaderId 到分配明细
                await WritebackTaskHeaderIdAsync(header.OutboundOrderId, groupedByContainer);

                // 通过契约更新分配头状态为拣货中
                await _outboundContract.UpdateAllocationHeaderStatusAsync(
                    allocationHeaderId, BizConstants.AllocationStatus.PICKING);

                await _unitOfWork.CommitAsync();
                return ServiceResult.Ok(WMSErrorMessages.GetMessage(OUTBOUND_TASK_CREATED, taskCount));
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                return ServiceResult.Fail(WMSErrorMessages.GetMessage(ALLOCATION_TASK_GENERATE_FAILED, ex.Message));
            }
        }

        // ==================== 验证方法 ====================

        /// <summary>
        /// 验证出库单是否满足分配条件（存在、状态允许）
        /// </summary>
        private async Task<(OutboundOrder? Order, string? Error)> ValidateOrderForAllocationAsync(long outboundOrderId)
        {
            var order = await _orderRepository.GetByIdAsync(outboundOrderId);
            if (order == null)
                return (null, "出库单不存在");

            try
            {
                await _statusValidator.ValidateTransitionAsync(
                    BizConstants.DocTypes.OUTBOUND_ORDER,
                    order.OrderStatus,
                    BizConstants.OutboundOrderStatus.RELEASING);
            }
            catch (InvalidOperationException ex)
            {
                return (null, ex.Message);
            }

            if (order.WarehouseId == null || order.WarehouseId <= 0)
                return (null, "出库单未关联仓库");

            return (order, null);
        }

        /// <summary>
        /// 验证分配头是否满足生成任务条件（存在、状态、明细）
        /// </summary>
        private async Task<(OutboundAllocationHeader? Header, string? Error)> ValidateAllocationHeaderAsync(long allocationHeaderId)
        {
            var header = await _headerRepository.GetFirstOrDefaultWithNavAsync(h => h.Id == allocationHeaderId);
            if (header == null)
                return (null, "分配单不存在");

            if (header.AllocStatus != BizConstants.AllocationStatus.ALLOCATED)
                return (null, $"分配单状态为 {header.AllocStatus}，只有已分配状态才能生成出库任务");

            if (header.Details == null || header.Details.Count == 0)
                return (null, "分配单无明细行");

            return (header, null);
        }

        // ==================== 策略执行方法 ====================

        /// <summary>
        /// 单行分配：策略执行 + 库存锁定 + 明细创建
        /// </summary>
        private async Task<ServiceResult<SingleLineAllocationResult>> AllocateSingleLineAsync(
            OutboundOrderLine line, OutboundOrder order,
            long warehouseId, string warehouseCode, string orderNo, string orderType)
        {
            var requiredQty = line.OrderedQty ?? 0;

            // 执行出库分配策略
            var input = new OutboundAllocationStrategyInput
            {
                WarehouseId = warehouseId,
                WarehouseCode = warehouseCode,
                BusinessCode = orderNo,
                MaterialId = line.MaterialId,
                MaterialCode = line.MaterialCode ?? string.Empty,
                DocType = orderType,
                RequiredQty = requiredQty,
            };
            var (allocError, allocOutput) = await ExecuteOutboundAllocationStrategyAsync(input);
            if (allocOutput == null)
                return ServiceResult<SingleLineAllocationResult>.Fail(allocError!);

            // 检查分配数量是否满足需求
            if (allocOutput.Result.TotalAllocatedQty < requiredQty)
                return ServiceResult<SingleLineAllocationResult>.Fail(
                    WMSErrorMessages.GetMessage(ALLOCATION_INVENTORY_SHORTAGE_DETAIL, line.MaterialCode, requiredQty, allocOutput.Result.TotalAllocatedQty));

            // 将策略分配结果映射为出库分配明细，并锁定库存
            var details = new List<OutboundAllocationDetail>();
            foreach (var item in allocOutput.Result.Items)
            {
                var lockResult = await _inventoryContract.LockInventoryAsync(item.InventoryDetailId, item.AllocatedQty);
                if (lockResult == null)
                    return ServiceResult<SingleLineAllocationResult>.Fail(WMSErrorMessages.GetMessage(ALLOCATION_DETAIL_NOT_EXIST, item.InventoryDetailId));

                details.Add(new OutboundAllocationDetail
                {
                    OrderLineId = line.Id,
                    MaterialId = line.MaterialId,
                    MaterialCode = line.MaterialCode,
                    BatchNo = item.BatchNo,
                    InventoryHeaderId = item.InventoryHeaderId,
                    InventoryDetailId = item.InventoryDetailId,
                    ContainerCode = item.ContainerCode,
                    LocationId = long.TryParse(item.LocationId, out var locId) ? locId : 0,
                    LocationCode = item.LocationCode,
                    AllocQty = item.AllocatedQty,
                    PickedQty = 0,
                    LineStatus = BizConstants.AllocationStatus.ALLOCATED,
                });
            }

            return ServiceResult<SingleLineAllocationResult>.Ok(new SingleLineAllocationResult
            {
                StrategyCode = allocOutput.StrategyCode,
                Details = details,
            });
        }

        /// <summary>
        /// 执行出库分配（A2 后为单步 InventoryAllocation 全仓选批次，出库偏好作为排序参数）
        /// </summary>
        private async Task<(string? Error, OutboundAllocationStrategyOutput? Output)> ExecuteOutboundAllocationStrategyAsync(
            OutboundAllocationStrategyInput input)
        {
            var context = new PolicyContext
            {
                WarehouseId = input.WarehouseId,
                WarehouseCode = input.WarehouseCode,
                BusinessCode = input.BusinessCode,
                MaterialId = input.MaterialId,
                DocType = input.DocType,
            };
            context.SetData(StrategyParams.InventoryAllocationInput.WAREHOUSE_CODE, input.WarehouseCode);
            context.SetData(StrategyParams.InventoryAllocationInput.MATERIAL_CODE, input.MaterialCode);
            context.SetData(StrategyParams.InventoryAllocationInput.REQUIRED_QTY, input.RequiredQty);

            var policyResult = await _policyEngine.ExecuteAsync(PolicyType.InventoryAllocation, context);
            if (!policyResult.IsSuccess)
                return ($"物料 {input.MaterialCode} 出库分配策略执行失败: {policyResult.ErrorMessage}", null);

            // A2: 出库链现为单步 InventoryAllocation，库存分配结果写入上下文（InventoryAllocationOutput.RESULT）。
            var allocationResult = context.GetData<InventoryAllocationResult>(StrategyParams.InventoryAllocationOutput.RESULT);
            if (allocationResult == null)
                return ($"物料 {input.MaterialCode} 未配置库存分配(InventoryAllocation)策略链或执行未产出结果，请检查策略链配置", null);
            if (allocationResult.Items.Count == 0)
                return ($"物料 {input.MaterialCode} 无可用库存", null);

            // 记录策略编码
            var execLog = policyResult.ExecutionLogs.FirstOrDefault(l => l.IsSuccess);
            var strategyCode = execLog?.PolicyCode ?? string.Empty;

            return (null, new OutboundAllocationStrategyOutput
            {
                Result = allocationResult,
                StrategyCode = strategyCode,
            });
        }

        /// <summary>
        /// 执行下架策略，确定目标出库口
        /// </summary>
        private async Task<(string? Error, PickingStrategyOutput? Output)> ExecutePickingStrategyAsync(
            PickingStrategyInput input)
        {
            var context = new PolicyContext
            {
                WarehouseId = input.WarehouseId,
                BusinessCode = input.BusinessCode,
                DocType = input.DocType,
            };
            context.SetData(StrategyParams.PickingInput.SOURCE_AISLE_NO, input.SourceAisleNo);

            var policyResult = await _policyEngine.ExecuteAsync(PolicyType.Picking, context);
            if (!policyResult.IsSuccess)
                return ($"下架策略执行失败: {policyResult.ErrorMessage}", null);

            var pickingOutput = policyResult.Output as PickingResult;
            var output = new PickingStrategyOutput
            {
                DestinationStationId = pickingOutput?.DestinationStationId,
                DestinationStationCode = pickingOutput?.DestinationStationCode,
                DestinationZoneId = pickingOutput?.DestinationZoneId,
            };

            if ((output.DestinationStationId == null || output.DestinationStationId <= 0)
                && string.IsNullOrWhiteSpace(output.DestinationStationCode))
                return ("未找到出库口，无法生成出库任务", null);

            return (null, output);
        }

        // ==================== 辅助方法 ====================

        /// <summary>
        /// 根据库位ID查询所在巷道号
        /// </summary>
        private async Task<int> GetSourceAisleNoAsync(long locationId)
        {
            if (locationId <= 0) return 0;

            var locationRepo = _unitOfWork.GetRepository<Entities.Warehouse.MdLocation, long>();
            var location = await locationRepo.GetByIdAsync(locationId);
            return location?.AisleNo ?? 0;
        }

        /// <summary>
        /// 按容器分组创建拣货任务，返回创建的任务数
        /// </summary>
        private async Task<int> CreatePickingTasksByContainerAsync(
            OutboundAllocationHeader header,
            List<IGrouping<string, OutboundAllocationDetail>> containerGroups,
            PickingStrategyOutput pickingOutput)
        {
            var taskCount = 0;
            foreach (var containerGroup in containerGroups)
            {
                var firstDetail = containerGroup.First();
                var request = new PickingTaskRequest
                {
                    WarehouseId = header.WarehouseId ?? 0,
                    DocId = header.OutboundOrderId,
                    DocNo = header.OutboundOrderNo,
                    ContainerId = firstDetail.ContainerId,
                    ContainerNo = firstDetail.ContainerCode,
                    FromLocationId = firstDetail.LocationId,
                    FromLocationCode = firstDetail.LocationCode,
                    ToLocationId = pickingOutput.DestinationStationId,
                    ToLocationCode = pickingOutput.DestinationStationCode,
                    ToZoneId = pickingOutput.DestinationZoneId,
                    Lines = containerGroup.Select(d => new PickingTaskLineRequest
                    {
                        MaterialId = d.MaterialId,
                        MaterialCode = d.MaterialCode,
                        BatchNo = d.BatchNo,
                        InventoryHeaderId = d.InventoryHeaderId,
                    }).ToList(),
                };

                var taskResult = await _taskContract.CreatePickingTaskAsync(request);
                if (!taskResult.Success)
                    throw new Exception($"创建拣货任务失败: {taskResult.Message}");

                taskCount++;
            }
            return taskCount;
        }

        /// <summary>
        /// 查询刚创建的任务并回写 TaskHeaderId 到分配明细
        /// </summary>
        private async Task WritebackTaskHeaderIdAsync(
            long outboundOrderId,
            List<IGrouping<string, OutboundAllocationDetail>> containerGroups)
        {
            var taskRepo = _unitOfWork.GetRepository<Entities.Task.TaskHeader, long>();

            foreach (var containerGroup in containerGroups)
            {
                var containerCode = containerGroup.Key;
                var locationId = containerGroup.First().LocationId;

                var task = await taskRepo.GetFirstOrDefaultAsync(t =>
                    t.TaskType == BizConstants.TaskTypes.PICKING &&
                    t.DocId == outboundOrderId &&
                    t.ContainerNo == containerCode &&
                    t.FromLocationId == locationId);

                if (task != null)
                {
                    foreach (var detail in containerGroup)
                    {
                        await _outboundContract.UpdateAllocationDetailStatusAsync(
                            detail.Id, BizConstants.AllocationStatus.PICKING, task.Id);
                    }
                }
            }
        }
    }
}
