using KH.WMS.Contracts.Container;
using KH.WMS.Contracts.Inventory;
using KH.WMS.Contracts.Tasks;
using KH.WMS.Contracts.Warehouse;
using KH.WMS.Algorithms.Strategy.Constants;
using KH.WMS.Core.Database.Repositories;
using KH.WMS.Core.Database.UnitOfWorks;
using KH.WMS.Core.DependencyInjection.ServiceLifetimes;
using KH.WMS.Core.Logging.WMSError;
using KH.WMS.Core.Models;
using KH.WMS.Entities.Constants;
using KH.WMS.Entities.Inbound;
using KH.WMS.Entities.Inventory;
using KH.WMS.Entities.Task;
using KH.WMS.Entities.Warehouse;
using KH.WMS.Modules.TaskModule.DTOs;
using KH.WMS.Modules.TaskModule.Interfaces;
using SqlSugar;
using static KH.WMS.Core.Logging.WMSError.WMSErrorCodes;

namespace KH.WMS.Modules.TaskModule.Services;

[RegisteredService(ServiceType = typeof(IAdhocService))]
public class AdhocService(
    ITaskContract taskContract,
    IInventoryContract inventoryContract,
    IContainerContract containerContract,
    ILocationContract locationContract,
    IUnitOfWork unitOfWork,
    ISqlSugarClient db) : IAdhocService
{
    // ==================== 入库 ====================

    /// <inheritdoc />
    public async Task<ServiceResult<string>> AdhocInboundAsync(AdhocInboundRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ContainerCode))
            return ServiceResult<string>.Fail(WMSErrorMessages.GetMessage(CONTAINER_CODE_EMPTY));

        if (request.Lines == null || request.Lines.Count == 0)
            return ServiceResult<string>.Fail(WMSErrorMessages.GetMessage(ADHOC_INBOUND_LINES_EMPTY));

        try
        {
            await unitOfWork.BeginTransactionAsync();

            await containerContract.RegisterContainersAsync([request.ContainerCode]);

            // 校验容器：无活跃组盘 + 无活跃任务 + 无库存
            var containerCheck = await ValidateContainerForBindingAsync(request.ContainerCode);
            if (containerCheck != null)
                return ServiceResult<string>.Fail(containerCheck);

            var bindHeader = await CreateAdhocBindHeaderAsync(request);
            var taskResult = await CreatePutawayTaskForBindAsync(bindHeader, request);
            if (!taskResult.Success)
                return ServiceResult<string>.Fail(taskResult.Message ?? WMSErrorMessages.GetMessage(CONTAINER_BIND_PUTAWAY_TASK_FAILED));

            await UpdateBindStatusAsync(bindHeader, BizConstants.BindStatus.PUTTING_AWAY);
            await SetContainerMovingAsync(request.ContainerCode);

            await unitOfWork.CommitAsync();
            return ServiceResult<string>.Ok(taskResult.Data,
                WMSErrorMessages.GetMessage(PUTAWAY_TASK_CREATED_WITH_NO, taskResult.Data));
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackAsync();
            return ServiceResult<string>.Fail(WMSErrorMessages.GetMessage(ADHOC_OPERATION_FAILED, ex.Message));
        }
    }

    // ==================== 出库 ====================

    /// <inheritdoc />
    public async Task<ServiceResult<string>> AdhocOutboundAsync(AdhocOutboundRequest request)
    {
        if (request.Lines == null || request.Lines.Count == 0)
            return ServiceResult<string>.Fail(WMSErrorMessages.GetMessage(ADHOC_OUTBOUND_LINES_EMPTY));

        try
        {
            await unitOfWork.BeginTransactionAsync();

            // 校验容器是否有活跃任务
            if (!string.IsNullOrWhiteSpace(request.ContainerCode))
            {
                var taskCheck = await ValidateContainerForTaskAsync(request.ContainerCode);
                if (taskCheck != null)
                    return ServiceResult<string>.Fail(taskCheck);
            }

            var (lockResult, taskLines, fromLocationId, fromLocationCode) =
                await LockInventoryAndBuildTaskLinesAsync(request);
            if (!lockResult.Success)
                return lockResult;

            var taskResult = await CreatePickingTaskAsync(request, taskLines, fromLocationId, fromLocationCode);
            if (!taskResult.Success)
                return ServiceResult<string>.Fail(taskResult.Message ?? WMSErrorMessages.GetMessage(ALLOCATE_FAILED));

            await SetContainerMovingAsync(request.ContainerCode);

            await unitOfWork.CommitAsync();
            return ServiceResult<string>.Ok(taskResult.Data,
                WMSErrorMessages.GetMessage(OUTBOUND_TASK_CREATED, 1));
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackAsync();
            return ServiceResult<string>.Fail(WMSErrorMessages.GetMessage(ADHOC_OPERATION_FAILED, ex.Message));
        }
    }

    /// <inheritdoc />
    public async Task<ServiceResult<string>> AdhocOutboundByContainerAsync(AdhocOutboundByContainerRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ContainerCode))
            return ServiceResult<string>.Fail(WMSErrorMessages.GetMessage(CONTAINER_CODE_EMPTY));

        try
        {
            await unitOfWork.BeginTransactionAsync();

            var (linesResult, locationId, locationCode) =
                await BuildOutboundLinesFromContainerAsync(request);
            if (!linesResult.Success)
                return ServiceResult<string>.Fail(linesResult.Message ?? WMSErrorMessages.GetMessage(ADHOC_OPERATION_FAILED));

            var outboundRequest = new AdhocOutboundRequest
            {
                WarehouseId = request.WarehouseId,
                ContainerCode = request.ContainerCode,
                FromLocationId = locationId,
                FromLocationCode = locationCode,
                Lines = linesResult.Data,
            };

            var result = await AdhocOutboundAsync(outboundRequest);
            if (!result.Success)
            {
                await unitOfWork.RollbackAsync();
                return result;
            }

            await unitOfWork.CommitAsync();
            return result;
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackAsync();
            return ServiceResult<string>.Fail(WMSErrorMessages.GetMessage(ADHOC_OPERATION_FAILED, ex.Message));
        }
    }

    /// <inheritdoc />
    public async Task<ServiceResult<string>> AdhocOutboundByLocationAsync(AdhocOutboundByLocationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.LocationCode))
            return ServiceResult<string>.Fail(WMSErrorMessages.GetMessage(ADHOC_LOCATION_CODE_EMPTY));

        try
        {
            await unitOfWork.BeginTransactionAsync();

            var (linesResult, locationId, locationCode) =
                await BuildOutboundLinesFromLocationAsync(request);
            if (!linesResult.Success)
                return ServiceResult<string>.Fail(linesResult.Message ?? WMSErrorMessages.GetMessage(ADHOC_OPERATION_FAILED));

            var outboundRequest = new AdhocOutboundRequest
            {
                WarehouseId = request.WarehouseId,
                FromLocationId = locationId,
                FromLocationCode = locationCode,
                Lines = linesResult.Data,
            };

            var result = await AdhocOutboundAsync(outboundRequest);
            if (!result.Success)
            {
                await unitOfWork.RollbackAsync();
                return result;
            }

            await unitOfWork.CommitAsync();
            return result;
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackAsync();
            return ServiceResult<string>.Fail(WMSErrorMessages.GetMessage(ADHOC_OPERATION_FAILED, ex.Message));
        }
    }

    // ==================== 上架 ====================

    /// <inheritdoc />
    public async Task<ServiceResult<string>> AdhocPutawayFromToAsync(AdhocPutawayFromToRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FromLocationCode))
            return ServiceResult<string>.Fail(WMSErrorMessages.GetMessage(ADHOC_FROM_LOCATION_CODE_EMPTY));
        if (string.IsNullOrWhiteSpace(request.ToLocationCode))
            return ServiceResult<string>.Fail(WMSErrorMessages.GetMessage(ADHOC_TO_LOCATION_CODE_EMPTY));

        try
        {
            await unitOfWork.BeginTransactionAsync();

            var locationRepo = unitOfWork.GetRepository<MdLocation, long>();
            var fromLocation = await ValidateLocationAsync(locationRepo, request.FromLocationCode, validateStatus: false);
            if (fromLocation == null)
                return ServiceResult<string>.Fail(WMSErrorMessages.GetMessage(ADHOC_FROM_LOCATION_NOT_EXIST, request.FromLocationCode));

            var toLocation = await ValidateTargetLocationAsync(locationRepo, request.ToLocationCode);
            if (toLocation == null)
                return ServiceResult<string>.Fail(WMSErrorMessages.GetMessage(TARGET_LOCATION_NOT_EXIST));

            // 校验容器是否有活跃任务
            if (!string.IsNullOrWhiteSpace(request.ContainerCode))
            {
                var taskCheck = await ValidateContainerForTaskAsync(request.ContainerCode);
                if (taskCheck != null)
                    return ServiceResult<string>.Fail(taskCheck);
            }

            var taskResult = await CreatePutawayTaskWithTargetAsync(request, toLocation, BizConstants.DocTypes.ADHOC_TRANSFER);
            if (!taskResult.Success)
                return ServiceResult<string>.Fail(taskResult.Message ?? WMSErrorMessages.GetMessage(CONTAINER_BIND_PUTAWAY_TASK_FAILED));

            await MarkTaskLocationAllocatedAsync(taskResult.Data);
            await SetLocationStatusAsync(fromLocation.Id, BizConstants.LocationStatus.EMPTY);
            await SetLocationLockStatusAsync(toLocation.Id, BizConstants.LocationLockStatus.INBOUND_RESERVED);
            await SetContainerMovingAsync(request.ContainerCode);

            await unitOfWork.CommitAsync();
            return ServiceResult<string>.Ok(taskResult.Data,
                WMSErrorMessages.GetMessage(PUTAWAY_TASK_CREATED_WITH_NO, taskResult.Data));
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackAsync();
            return ServiceResult<string>.Fail(WMSErrorMessages.GetMessage(ADHOC_OPERATION_FAILED, ex.Message));
        }
    }

    /// <inheritdoc />
    public async Task<ServiceResult<string>> AdhocPutawayToAsync(AdhocPutawayToRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ToLocationCode))
            return ServiceResult<string>.Fail(WMSErrorMessages.GetMessage(ADHOC_TO_LOCATION_CODE_EMPTY));

        try
        {
            await unitOfWork.BeginTransactionAsync();

            var locationRepo = unitOfWork.GetRepository<MdLocation, long>();
            var toLocation = await ValidateTargetLocationAsync(locationRepo, request.ToLocationCode);
            if (toLocation == null)
                return ServiceResult<string>.Fail(WMSErrorMessages.GetMessage(TARGET_LOCATION_NOT_EXIST));

            // 校验容器是否有活跃任务
            if (!string.IsNullOrWhiteSpace(request.ContainerCode))
            {
                var taskCheck = await ValidateContainerForTaskAsync(request.ContainerCode);
                if (taskCheck != null)
                    return ServiceResult<string>.Fail(taskCheck);
            }

            var taskResult = await CreatePutawayTaskWithTargetAsync(request, toLocation, BizConstants.DocTypes.ADHOC_INBOUND);
            if (!taskResult.Success)
                return ServiceResult<string>.Fail(taskResult.Message ?? WMSErrorMessages.GetMessage(CONTAINER_BIND_PUTAWAY_TASK_FAILED));

            await MarkTaskLocationAllocatedAsync(taskResult.Data);
            await SetLocationLockStatusAsync(toLocation.Id, BizConstants.LocationLockStatus.INBOUND_RESERVED);
            await SetContainerMovingAsync(request.ContainerCode);

            await unitOfWork.CommitAsync();
            return ServiceResult<string>.Ok(taskResult.Data,
                WMSErrorMessages.GetMessage(PUTAWAY_TASK_CREATED_WITH_NO, taskResult.Data));
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackAsync();
            return ServiceResult<string>.Fail(WMSErrorMessages.GetMessage(ADHOC_OPERATION_FAILED, ex.Message));
        }
    }

    // ==================== 容器校验方法 ====================

    /// <summary>
    /// 校验容器是否允许组盘：无活跃组盘 + 无活跃任务 + 无库存
    /// </summary>
    private async Task<string?> ValidateContainerForBindingAsync(string containerCode)
    {
        var bindHeaderRepo = unitOfWork.GetRepository<InboundContainerBindHeader, long>();
        var hasActiveBind = await bindHeaderRepo.GetFirstOrDefaultAsync(
            h => h.ContainerCode == containerCode
                && (h.BindStatus == BizConstants.BindStatus.BOUND || h.BindStatus == BizConstants.BindStatus.PUTTING_AWAY));
        if (hasActiveBind != null)
            return WMSErrorMessages.GetMessage(CONTAINER_HAS_ACTIVE_BIND, containerCode);

        if (await inventoryContract.ContainerHasInventoryAsync(containerCode))
            return WMSErrorMessages.GetMessage(CONTAINER_HAS_INVENTORY, containerCode);

        var taskCheck = await ValidateContainerForTaskAsync(containerCode);
        return taskCheck;
    }

    /// <summary>
    /// 校验容器是否已有活跃任务（PENDING/IN_PROGRESS）
    /// </summary>
    private async Task<string?> ValidateContainerForTaskAsync(string containerCode)
    {
        var taskRepo = unitOfWork.GetRepository<TaskHeader, long>();
        var hasActiveTask = await taskRepo.GetFirstOrDefaultAsync(
            t => t.ContainerNo == containerCode
                && (t.TaskStatus == BizConstants.TaskStatus.PENDING || t.TaskStatus == BizConstants.TaskStatus.IN_PROGRESS));
        if (hasActiveTask != null)
            return WMSErrorMessages.GetMessage(CONTAINER_HAS_ACTIVE_TASK, containerCode, hasActiveTask.TaskNo);

        return null;
    }

    /// <summary>
    /// 校验库存头是否冻结
    /// </summary>
    private async Task<string?> ValidateInventoryNotFrozenAsync(long inventoryDetailId)
    {
        var detailRepo = unitOfWork.GetRepository<InvInventoryDetail, long>();
        var headerRepo = unitOfWork.GetRepository<InvInventoryHeader, long>();
        var invDetail = await detailRepo.GetByIdAsync(inventoryDetailId);
        if (invDetail == null)
            return WMSErrorMessages.GetMessage(ALLOCATION_DETAIL_NOT_EXIST, inventoryDetailId);

        var invHeader = await headerRepo.GetByIdAsync(invDetail.HeaderId);
        if (invHeader != null && invHeader.InventoryStatus == BizConstants.InventoryStatus.FROZEN)
            return WMSErrorMessages.GetMessage(INVENTORY_FROZEN, inventoryDetailId);

        return null;
    }

    // ==================== 库位校验方法 ====================

    /// <summary>
    /// 校验库位是否存在，可选择性校验状态（禁用/锁定/占用）
    /// </summary>
    private async Task<MdLocation?> ValidateLocationAsync(
        IRepository<MdLocation, long> locationRepo, string locationCode, bool validateStatus)
    {
        var location = await locationRepo.GetFirstOrDefaultAsync(l => l.LocationCode == locationCode);
        if (location == null)
            return null;

        if (validateStatus)
            ValidateLocationUsable(location, locationCode);

        return location;
    }

    /// <summary>
    /// 校验目标库位：必须存在 + 未禁用 + 未锁定 + 空闲
    /// </summary>
    private async Task<MdLocation?> ValidateTargetLocationAsync(
        IRepository<MdLocation, long> locationRepo, string locationCode)
    {
        var location = await ValidateLocationAsync(locationRepo, locationCode, validateStatus: true);
        if (location == null)
            return null;

        if (location.Status == BizConstants.LocationStatus.OCCUPIED)
            throw new InvalidOperationException(
                WMSErrorMessages.GetMessage(ADHOC_TO_LOCATION_STATUS_ERROR, locationCode, "已占用"));

        if (location.LockStatus != BizConstants.LocationLockStatus.NONE)
            throw new InvalidOperationException(
                WMSErrorMessages.GetMessage(ADHOC_TO_LOCATION_STATUS_ERROR, locationCode, "已锁定"));

        return location;
    }

    /// <summary>
    /// 校验库位可用性（禁用/锁定）
    /// </summary>
    private void ValidateLocationUsable(MdLocation location, string locationCode)
    {
        if (location.IsDisabled == BizConstants.BoolFlag.YES)
            throw new InvalidOperationException(
                WMSErrorMessages.GetMessage(LOCATION_STATUS_ERROR, locationCode));

        if (location.LockStatus != BizConstants.LocationLockStatus.NONE)
            throw new InvalidOperationException(
                WMSErrorMessages.GetMessage(LOCATION_STATUS_ERROR, locationCode));
    }

    // ==================== 库位/容器状态更新方法 ====================

    /// <summary>
    /// 更新库位状态
    /// </summary>
    private async Task SetLocationStatusAsync(long locationId, string status)
    {
        await locationContract.UpdateLocationStatusAsync(locationId, status);
    }

    /// <summary>
    /// 更新库位锁定状态
    /// </summary>
    private async Task SetLocationLockStatusAsync(long locationId, byte lockStatus)
    {
        await locationContract.UpdateLocationLockStatusAsync(locationId, lockStatus);
    }

    /// <summary>
    /// 设置容器状态为搬运中
    /// </summary>
    private async Task SetContainerMovingAsync(string? containerCode)
    {
        if (!string.IsNullOrWhiteSpace(containerCode))
        {
            await containerContract.UpdateStatusAsync(
                [containerCode], BizConstants.ContainerStatus.MOVING);
        }
    }

    // ==================== 组盘相关方法 ====================

    /// <summary>
    /// 创建无单据组盘记录
    /// </summary>
    private async Task<InboundContainerBindHeader> CreateAdhocBindHeaderAsync(AdhocInboundRequest request)
    {
        var bindHeaderRepo = unitOfWork.GetRepository<InboundContainerBindHeader, long>();
        var bindHeader = new InboundContainerBindHeader
        {
            ContainerCode = request.ContainerCode,
            BindStatus = BizConstants.BindStatus.BOUND,
            WarehouseId = request.WarehouseId,
            DetailCount = request.Lines.Count,
            SourceType = BizConstants.SourceTypes.ADHOC,
            InboundOrderId = null,
            QualityStatus = BizConstants.QualityStatus.QUALIFIED,
            BindTime = DateTime.Now,
            Details = request.Lines.Select(l => new InboundContainerBindDetail
            {
                MaterialId = l.MaterialId,
                MaterialCode = l.MaterialCode,
                MaterialName = l.MaterialName,
                Qty = l.Qty,
                BatchNo = l.BatchNo,
                ProductionDate = l.ProductionDate,
                ExpiryDate = l.ExpiryDate,
            }).ToList(),
        };
        await bindHeaderRepo.AddWithNavAsync(bindHeader);
        return bindHeader;
    }

    /// <summary>
    /// 更新组盘状态
    /// </summary>
    private async Task UpdateBindStatusAsync(InboundContainerBindHeader bindHeader, string status)
    {
        var bindHeaderRepo = unitOfWork.GetRepository<InboundContainerBindHeader, long>();
        bindHeader.BindStatus = status;
        await bindHeaderRepo.UpdateAsync(bindHeader);
    }

    // ==================== 任务创建方法 ====================

    /// <summary>
    /// 为组盘创建上架任务
    /// </summary>
    private async Task<ServiceResult<string>> CreatePutawayTaskForBindAsync(
        InboundContainerBindHeader bindHeader, AdhocInboundRequest request)
    {
        var taskRequest = new PutawayTaskRequest
        {
            WarehouseId = request.WarehouseId,
            DocId = bindHeader.Id,
            DocNo = null,
            DocType = BizConstants.DocTypes.ADHOC_INBOUND,
            ContainerNo = request.ContainerCode,
            Lines = request.Lines.Select(l => new PutawayTaskLineRequest
            {
                MaterialId = l.MaterialId,
                MaterialCode = l.MaterialCode,
                MaterialName = l.MaterialName,
                BatchNo = l.BatchNo,
            }).ToList(),
        };

        return await taskContract.CreatePutawayTaskAsync(taskRequest);
    }

    /// <summary>
    /// 创建指定目标库位的上架任务（起始→目的 / 直接上架）
    /// </summary>
    private async Task<ServiceResult<string>> CreatePutawayTaskWithTargetAsync(
        IAdhocPutawayRequest request, MdLocation toLocation, string docType)
    {
        var taskRequest = new PutawayTaskRequest
        {
            WarehouseId = request.WarehouseId,
            DocNo = null,
            DocType = docType,
            ContainerNo = request.ContainerCode,
            FromLocationCode = request.FromLocationCode,
            ToLocationId = toLocation.Id,
            ToLocationCode = toLocation.LocationCode,
            Lines = (request.Lines ?? []).Select(l => new PutawayTaskLineRequest
            {
                MaterialId = l.MaterialId,
                MaterialCode = l.MaterialCode,
                MaterialName = l.MaterialName,
                BatchNo = l.BatchNo,
            }).ToList(),
        };

        return await taskContract.CreatePutawayTaskAsync(taskRequest);
    }

    /// <summary>
    /// 标记任务已分配实际货位（跳过 Phase 2 策略分配）
    /// </summary>
    private async Task MarkTaskLocationAllocatedAsync(string taskNo)
    {
        var taskRepo = unitOfWork.GetRepository<TaskHeader, long>();
        var task = await taskRepo.GetFirstOrDefaultAsync(t => t.TaskNo == taskNo);
        if (task != null)
        {
            task.LocationAllocated = BizConstants.BoolFlag.YES;
            await taskRepo.UpdateAsync(task);
        }
    }

    /// <summary>
    /// 创建拣货任务
    /// </summary>
    private async Task<ServiceResult<string>> CreatePickingTaskAsync(
        AdhocOutboundRequest request, List<PickingTaskLineRequest> taskLines,
        long? fromLocationId, string? fromLocationCode)
    {
        var taskRequest = new PickingTaskRequest
        {
            WarehouseId = request.WarehouseId,
            DocNo = null,
            DocType = BizConstants.DocTypes.ADHOC_OUTBOUND,
            ContainerNo = request.ContainerCode,
            FromLocationId = fromLocationId,
            FromLocationCode = fromLocationCode,
            Lines = taskLines,
        };

        return await taskContract.CreatePickingTaskAsync(taskRequest);
    }

    // ==================== 库存操作方法 ====================

    /// <summary>
    /// 锁定库存并构建拣货任务行
    /// </summary>
    private async Task<(ServiceResult<string> Result, List<PickingTaskLineRequest> TaskLines,
        long? FromLocationId, string? FromLocationCode)>
        LockInventoryAndBuildTaskLinesAsync(AdhocOutboundRequest request)
    {
        var detailRepo = unitOfWork.GetRepository<InvInventoryDetail, long>();
        var headerRepo = unitOfWork.GetRepository<InvInventoryHeader, long>();
        var taskLines = new List<PickingTaskLineRequest>();
        long? fromLocationId = request.FromLocationId;
        string? fromLocationCode = request.FromLocationCode;
        long? inventoryHeaderId = null;

        foreach (var line in request.Lines)
        {
            var invDetail = await detailRepo.GetByIdAsync(line.InventoryDetailId);
            if (invDetail == null)
                return (ServiceResult<string>.Fail(
                    WMSErrorMessages.GetMessage(ALLOCATION_DETAIL_NOT_EXIST, line.InventoryDetailId)),
                    [], null, null);

            // 校验库存是否冻结
            var frozenCheck = await ValidateInventoryNotFrozenAsync(line.InventoryDetailId);
            if (frozenCheck != null)
                return (ServiceResult<string>.Fail(frozenCheck), [], null, null);

            var availableQty = invDetail.Qty - invDetail.LockedQty;
            if (availableQty < line.Qty)
                return (ServiceResult<string>.Fail(
                    WMSErrorMessages.GetMessage(INVENTORY_QTY_INSUFFICIENT, line.InventoryDetailId)),
                    [], null, null);

            var lockResult = await inventoryContract.LockInventoryAsync(line.InventoryDetailId, line.Qty);
            if (lockResult == null)
                return (ServiceResult<string>.Fail(WMSErrorMessages.GetMessage(ADHOC_INVENTORY_LOCK_FAILED)),
                    [], null, null);

            if (inventoryHeaderId == null)
            {
                var invHeader = await headerRepo.GetByIdAsync(invDetail.HeaderId);
                if (invHeader != null)
                {
                    fromLocationId ??= invHeader.LocationId;
                    fromLocationCode ??= invHeader.LocationCode;
                    inventoryHeaderId = invHeader.Id;
                }
            }

            taskLines.Add(new PickingTaskLineRequest
            {
                MaterialId = invDetail.MaterialId,
                MaterialCode = invDetail.MaterialCode,
                BatchNo = invDetail.BatchNo,
                InventoryHeaderId = inventoryHeaderId,
            });
        }

        return (ServiceResult<string>.Ok(string.Empty), taskLines, fromLocationId, fromLocationCode);
    }

    /// <summary>
    /// 根据容器号构建出库行
    /// </summary>
    private async Task<(ServiceResult<List<AdhocOutboundLineRequest>> Result,
        long? LocationId, string? LocationCode)>
        BuildOutboundLinesFromContainerAsync(AdhocOutboundByContainerRequest request)
    {
        var headerRepo = unitOfWork.GetRepository<InvInventoryHeader, long>();
        var detailRepo = unitOfWork.GetRepository<InvInventoryDetail, long>();

        var invHeader = await headerRepo.GetFirstOrDefaultAsync(
            h => h.ContainerCode == request.ContainerCode && h.WarehouseId == request.WarehouseId);
        if (invHeader == null)
            return (ServiceResult<List<AdhocOutboundLineRequest>>.Fail(
                WMSErrorMessages.GetMessage(ADHOC_CONTAINER_NO_INVENTORY, request.ContainerCode)),
                null, null);

        var details = await detailRepo.GetListAsync(d => d.HeaderId == invHeader.Id);
        var lines = BuildOutboundLines(details, request.AllOutbound, request.SelectedLines);
        if (lines.Count == 0)
            return (ServiceResult<List<AdhocOutboundLineRequest>>.Fail(
                WMSErrorMessages.GetMessage(ADHOC_NO_AVAILABLE_INVENTORY)),
                null, null);

        return (ServiceResult<List<AdhocOutboundLineRequest>>.Ok(lines),
            invHeader.LocationId, invHeader.LocationCode);
    }

    /// <summary>
    /// 根据货位编码构建出库行
    /// </summary>
    private async Task<(ServiceResult<List<AdhocOutboundLineRequest>> Result,
        long? LocationId, string? LocationCode)>
        BuildOutboundLinesFromLocationAsync(AdhocOutboundByLocationRequest request)
    {
        var headerRepo = unitOfWork.GetRepository<InvInventoryHeader, long>();
        var detailRepo = unitOfWork.GetRepository<InvInventoryDetail, long>();

        var invHeaders = await headerRepo.GetListAsync(
            h => h.LocationCode == request.LocationCode && h.WarehouseId == request.WarehouseId);
        if (invHeaders == null || invHeaders.Count == 0)
            return (ServiceResult<List<AdhocOutboundLineRequest>>.Fail(
                WMSErrorMessages.GetMessage(ADHOC_LOCATION_NO_INVENTORY, request.LocationCode)),
                null, null);

        var allDetails = new List<InvInventoryDetail>();
        foreach (var h in invHeaders)
        {
            var details = await detailRepo.GetListAsync(d => d.HeaderId == h.Id);
            if (details != null && details.Count > 0)
                allDetails.AddRange(details);
        }

        var lines = BuildOutboundLines(allDetails, request.AllOutbound, request.SelectedLines);
        if (lines.Count == 0)
            return (ServiceResult<List<AdhocOutboundLineRequest>>.Fail(
                WMSErrorMessages.GetMessage(ADHOC_NO_AVAILABLE_INVENTORY)),
                null, null);

        return (ServiceResult<List<AdhocOutboundLineRequest>>.Ok(lines),
            invHeaders.First().LocationId, request.LocationCode);
    }

    /// <summary>
    /// 从库存明细列表构建出库行（全部出库 or 按选择）
    /// </summary>
    private List<AdhocOutboundLineRequest> BuildOutboundLines(
        List<InvInventoryDetail> details, bool allOutbound, List<AdhocOutboundLineRequest>? selectedLines)
    {
        if (allOutbound)
        {
            return details
                .Select(d => new AdhocOutboundLineRequest
                {
                    InventoryDetailId = d.Id,
                    Qty = d.Qty - d.LockedQty,
                })
                .Where(l => l.Qty > 0)
                .ToList();
        }

        if (selectedLines == null || selectedLines.Count == 0)
            return [];

        return selectedLines;
    }

    // ==================== 库存查询 ====================

    /// <inheritdoc />
    public async Task<ServiceResult<List<object>>> QueryInventoryAsync(AdhocInventoryQueryRequest request)
    {
        try
        {
            // 如果按出库口筛选，先查出库口所属库区的 ZoneId，再查该库区下所有货位
            List<string>? locationCodes = null;
            if (!string.IsNullOrWhiteSpace(request.PortCode))
            {
                var port = await db.Queryable<MdPort>()
                    .Where(p => p.WarehouseId == request.WarehouseId && p.PortCode == request.PortCode)
                    .FirstAsync();
                if (port == null)
                    return ServiceResult<List<object>>.Fail("出库口不存在");

                if (port.ZoneId == null)
                    return ServiceResult<List<object>>.Fail("出库口未关联库区");

                locationCodes = await db.Queryable<MdLocation>()
                    .Where(l => l.WarehouseId == request.WarehouseId && l.ZoneId == port.ZoneId)
                    .Select(l => l.LocationCode)
                    .ToListAsync();
            }

            // 如果按巷道编码筛选，先通过 MdAisle 找到巷道的 ZoneId + AisleNo，再查货位
            int? aisleNo = null;
            if (!string.IsNullOrWhiteSpace(request.AisleCode))
            {
                var aisle = await db.Queryable<MdAisle>()
                    .Where(a => a.WarehouseId == request.WarehouseId && a.AisleCode == request.AisleCode)
                    .FirstAsync();
                if (aisle != null)
                    aisleNo = aisle.AisleNo;
            }

            var query = db.Queryable<InvInventoryHeader>()
                .LeftJoin<InvInventoryDetail>((h, d) => h.Id == d.HeaderId)
                .Where((h, d) => h.WarehouseId == request.WarehouseId);

            // 容器编号
            if (!string.IsNullOrWhiteSpace(request.ContainerCode))
                query = query.Where((h, d) => h.ContainerCode == request.ContainerCode);

            // 货位编码
            if (!string.IsNullOrWhiteSpace(request.LocationCode))
                query = query.Where((h, d) => h.LocationCode == request.LocationCode);

            // 出库口筛选 → 限定货位编码范围
            if (locationCodes != null && locationCodes.Count > 0)
                query = query.Where((h, d) => locationCodes.Contains(h.LocationCode!));

            // 物料编码（模糊搜索）
            if (!string.IsNullOrWhiteSpace(request.MaterialCode))
                query = query.Where((h, d) => d.MaterialCode.Contains(request.MaterialCode));

            // 物料ID
            if (request.MaterialId.HasValue)
                query = query.Where((h, d) => d.MaterialId == request.MaterialId.Value);

            // 按库区编码筛选（通过 InvInventoryHeader.LocationCode 关联 MdLocation）
            if (!string.IsNullOrWhiteSpace(request.ZoneCode))
            {
                query = query.Where((h, d) =>
                    db.Queryable<MdLocation>()
                        .Where(l => l.LocationCode == h.LocationCode && l.ZoneCode == request.ZoneCode)
                        .Any());
            }

            // 按巷道编码筛选（通过 MdLocation.AisleNo）
            if (aisleNo.HasValue)
            {
                query = query.Where((h, d) =>
                    db.Queryable<MdLocation>()
                        .Where(l => l.LocationCode == h.LocationCode && l.AisleNo == aisleNo.Value)
                        .Any());
            }

            var result = await query
                .Select((h, d) => new
                {
                    headerId = h.Id,
                    detailId = d.Id,
                    containerCode = h.ContainerCode,
                    locationCode = h.LocationCode,
                    zoneCode = h.LocationCode != null
                        ? db.Queryable<MdLocation>()
                            .Where(l => l.LocationCode == h.LocationCode)
                            .Select(l => l.ZoneCode)
                            .First()
                        : null,
                    materialCode = d.MaterialCode,
                    materialName = (string?)null,
                    batchNo = d.BatchNo,
                    qty = d.Qty,
                    lockedQty = d.LockedQty,
                    availableQty = d.Qty - d.LockedQty,
                })
                .ToListAsync();

            return ServiceResult<List<object>>.Ok(result.Cast<object>().ToList());
        }
        catch (Exception ex)
        {
            return ServiceResult<List<object>>.Fail(
                WMSErrorMessages.GetMessage(ADHOC_OPERATION_FAILED, ex.Message));
        }
    }

    // ==================== 按区域/巷道/出库口出库 ====================

    /// <inheritdoc />
    public async Task<ServiceResult<string>> AdhocOutboundByZoneAsync(AdhocOutboundByZoneRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ZoneCode))
            return ServiceResult<string>.Fail("库区编码不能为空");

        try
        {
            await unitOfWork.BeginTransactionAsync();

            // 查询该库区下所有货位的库存
            var (linesResult, locationId, locationCode) =
                await BuildOutboundLinesFromZoneAsync(request);
            if (!linesResult.Success)
                return ServiceResult<string>.Fail(linesResult.Message ?? WMSErrorMessages.GetMessage(ADHOC_OPERATION_FAILED));

            var outboundRequest = new AdhocOutboundRequest
            {
                WarehouseId = request.WarehouseId,
                FromLocationId = locationId,
                FromLocationCode = locationCode,
                Lines = linesResult.Data,
            };

            var result = await AdhocOutboundAsync(outboundRequest);
            if (!result.Success)
            {
                await unitOfWork.RollbackAsync();
                return result;
            }

            await unitOfWork.CommitAsync();
            return result;
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackAsync();
            return ServiceResult<string>.Fail(WMSErrorMessages.GetMessage(ADHOC_OPERATION_FAILED, ex.Message));
        }
    }

    /// <inheritdoc />
    public async Task<ServiceResult<string>> AdhocOutboundByAisleAsync(AdhocOutboundByAisleRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.AisleCode))
            return ServiceResult<string>.Fail("巷道编码不能为空");

        try
        {
            await unitOfWork.BeginTransactionAsync();

            var (linesResult, locationId, locationCode) =
                await BuildOutboundLinesFromAisleAsync(request);
            if (!linesResult.Success)
                return ServiceResult<string>.Fail(linesResult.Message ?? WMSErrorMessages.GetMessage(ADHOC_OPERATION_FAILED));

            var outboundRequest = new AdhocOutboundRequest
            {
                WarehouseId = request.WarehouseId,
                FromLocationId = locationId,
                FromLocationCode = locationCode,
                Lines = linesResult.Data,
            };

            var result = await AdhocOutboundAsync(outboundRequest);
            if (!result.Success)
            {
                await unitOfWork.RollbackAsync();
                return result;
            }

            await unitOfWork.CommitAsync();
            return result;
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackAsync();
            return ServiceResult<string>.Fail(WMSErrorMessages.GetMessage(ADHOC_OPERATION_FAILED, ex.Message));
        }
    }

    /// <inheritdoc />
    public async Task<ServiceResult<string>> AdhocOutboundByPortAsync(AdhocOutboundByPortRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.PortCode))
            return ServiceResult<string>.Fail("出库口编码不能为空");

        try
        {
            await unitOfWork.BeginTransactionAsync();

            var (linesResult, locationId, locationCode) =
                await BuildOutboundLinesFromPortAsync(request);
            if (!linesResult.Success)
                return ServiceResult<string>.Fail(linesResult.Message ?? WMSErrorMessages.GetMessage(ADHOC_OPERATION_FAILED));

            var outboundRequest = new AdhocOutboundRequest
            {
                WarehouseId = request.WarehouseId,
                FromLocationId = locationId,
                FromLocationCode = locationCode,
                Lines = linesResult.Data,
            };

            var result = await AdhocOutboundAsync(outboundRequest);
            if (!result.Success)
            {
                await unitOfWork.RollbackAsync();
                return result;
            }

            await unitOfWork.CommitAsync();
            return result;
        }
        catch (Exception ex)
        {
            await unitOfWork.RollbackAsync();
            return ServiceResult<string>.Fail(WMSErrorMessages.GetMessage(ADHOC_OPERATION_FAILED, ex.Message));
        }
    }

    // ==================== 按区域/巷道/出库口构建出库行 ====================

    /// <summary>
    /// 根据库区编码构建出库行（通过货位关联库区）
    /// </summary>
    private async Task<(ServiceResult<List<AdhocOutboundLineRequest>> Result,
        long? LocationId, string? LocationCode)>
        BuildOutboundLinesFromZoneAsync(AdhocOutboundByZoneRequest request)
    {
        var headerRepo = unitOfWork.GetRepository<InvInventoryHeader, long>();
        var detailRepo = unitOfWork.GetRepository<InvInventoryDetail, long>();

        // 查询该库区下所有货位编码
        var locationCodesInZone = await db.Queryable<MdLocation>()
            .Where(l => l.WarehouseId == request.WarehouseId && l.ZoneCode == request.ZoneCode)
            .Select(l => l.LocationCode)
            .ToListAsync();

        if (locationCodesInZone.Count == 0)
            return (ServiceResult<List<AdhocOutboundLineRequest>>.Fail(
                "该库区下没有货位"), null, null);

        // 查询这些货位上的库存头
        var invHeaders = await db.Queryable<InvInventoryHeader>()
            .Where(h => h.WarehouseId == request.WarehouseId
                && locationCodesInZone.Contains(h.LocationCode!))
            .ToListAsync();

        if (invHeaders.Count == 0)
            return (ServiceResult<List<AdhocOutboundLineRequest>>.Fail(
                "该库区下没有库存"), null, null);

        var allDetails = new List<InvInventoryDetail>();
        foreach (var h in invHeaders)
        {
            var details = await detailRepo.GetListAsync(d => d.HeaderId == h.Id);
            if (details != null && details.Count > 0)
                allDetails.AddRange(details);
        }

        var lines = BuildOutboundLines(allDetails, request.AllOutbound, request.SelectedLines);
        if (lines.Count == 0)
            return (ServiceResult<List<AdhocOutboundLineRequest>>.Fail(
                WMSErrorMessages.GetMessage(ADHOC_NO_AVAILABLE_INVENTORY)),
                null, null);

        return (ServiceResult<List<AdhocOutboundLineRequest>>.Ok(lines),
            invHeaders.First().LocationId, invHeaders.First().LocationCode);
    }

    /// <summary>
    /// 根据巷道编码构建出库行（通过 MdAisle 查巷道号，再查该巷道号下的货位）
    /// </summary>
    private async Task<(ServiceResult<List<AdhocOutboundLineRequest>> Result,
        long? LocationId, string? LocationCode)>
        BuildOutboundLinesFromAisleAsync(AdhocOutboundByAisleRequest request)
    {
        var detailRepo = unitOfWork.GetRepository<InvInventoryDetail, long>();

        // 先找到巷道
        var aisle = await db.Queryable<MdAisle>()
            .Where(a => a.WarehouseId == request.WarehouseId && a.AisleCode == request.AisleCode)
            .FirstAsync();
        if (aisle == null)
            return (ServiceResult<List<AdhocOutboundLineRequest>>.Fail(
                "巷道不存在"), null, null);

        // 查询该巷道号下的所有货位编码
        var locationCodesInAisle = await db.Queryable<MdLocation>()
            .Where(l => l.WarehouseId == request.WarehouseId && l.AisleNo == aisle.AisleNo)
            .Select(l => l.LocationCode)
            .ToListAsync();

        if (locationCodesInAisle.Count == 0)
            return (ServiceResult<List<AdhocOutboundLineRequest>>.Fail(
                "该巷道下没有货位"), null, null);

        // 查询这些货位上的库存头
        var invHeaders = await db.Queryable<InvInventoryHeader>()
            .Where(h => h.WarehouseId == request.WarehouseId
                && locationCodesInAisle.Contains(h.LocationCode!))
            .ToListAsync();

        if (invHeaders.Count == 0)
            return (ServiceResult<List<AdhocOutboundLineRequest>>.Fail(
                "该巷道下没有库存"), null, null);

        var allDetails = new List<InvInventoryDetail>();
        foreach (var h in invHeaders)
        {
            var details = await detailRepo.GetListAsync(d => d.HeaderId == h.Id);
            if (details != null && details.Count > 0)
                allDetails.AddRange(details);
        }

        var lines = BuildOutboundLines(allDetails, request.AllOutbound, request.SelectedLines);
        if (lines.Count == 0)
            return (ServiceResult<List<AdhocOutboundLineRequest>>.Fail(
                WMSErrorMessages.GetMessage(ADHOC_NO_AVAILABLE_INVENTORY)),
                null, null);

        return (ServiceResult<List<AdhocOutboundLineRequest>>.Ok(lines),
            invHeaders.First().LocationId, invHeaders.First().LocationCode);
    }

    /// <summary>
    /// 根据出库口构建出库行（查出库口关联库区，再查库区下货位的库存）
    /// </summary>
    private async Task<(ServiceResult<List<AdhocOutboundLineRequest>> Result,
        long? LocationId, string? LocationCode)>
        BuildOutboundLinesFromPortAsync(AdhocOutboundByPortRequest request)
    {
        var detailRepo = unitOfWork.GetRepository<InvInventoryDetail, long>();

        // 先找到出库口
        var port = await db.Queryable<MdPort>()
            .Where(p => p.WarehouseId == request.WarehouseId && p.PortCode == request.PortCode)
            .FirstAsync();
        if (port == null)
            return (ServiceResult<List<AdhocOutboundLineRequest>>.Fail(
                "出库口不存在"), null, null);

        if (port.ZoneId == null)
            return (ServiceResult<List<AdhocOutboundLineRequest>>.Fail(
                "出库口未关联库区"), null, null);

        // 查询该库区下所有货位编码
        var locationCodesInZone = await db.Queryable<MdLocation>()
            .Where(l => l.WarehouseId == request.WarehouseId && l.ZoneId == port.ZoneId)
            .Select(l => l.LocationCode)
            .ToListAsync();

        if (locationCodesInZone.Count == 0)
            return (ServiceResult<List<AdhocOutboundLineRequest>>.Fail(
                "该出库口所属库区下没有货位"), null, null);

        // 查询这些货位上的库存头
        var invHeaders = await db.Queryable<InvInventoryHeader>()
            .Where(h => h.WarehouseId == request.WarehouseId
                && locationCodesInZone.Contains(h.LocationCode!))
            .ToListAsync();

        if (invHeaders.Count == 0)
            return (ServiceResult<List<AdhocOutboundLineRequest>>.Fail(
                "该出库口所属库区下没有库存"), null, null);

        var allDetails = new List<InvInventoryDetail>();
        foreach (var h in invHeaders)
        {
            var details = await detailRepo.GetListAsync(d => d.HeaderId == h.Id);
            if (details != null && details.Count > 0)
                allDetails.AddRange(details);
        }

        var lines = BuildOutboundLines(allDetails, request.AllOutbound, request.SelectedLines);
        if (lines.Count == 0)
            return (ServiceResult<List<AdhocOutboundLineRequest>>.Fail(
                WMSErrorMessages.GetMessage(ADHOC_NO_AVAILABLE_INVENTORY)),
                null, null);

        return (ServiceResult<List<AdhocOutboundLineRequest>>.Ok(lines),
            invHeaders.First().LocationId, invHeaders.First().LocationCode);
    }

    // ==================== 路线校验 ====================

    /// <inheritdoc />
    public async Task<ServiceResult<AdhocRouteCheckResult>> CheckRouteAsync(AdhocRouteCheckRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FromLocationCode))
            return ServiceResult<AdhocRouteCheckResult>.Fail("起始库位编码不能为空");
        if (string.IsNullOrWhiteSpace(request.ToLocationCode))
            return ServiceResult<AdhocRouteCheckResult>.Fail("目标库位编码不能为空");

        var fromLocation = await db.Queryable<MdLocation>()
            .Where(l => l.LocationCode == request.FromLocationCode)
            .FirstAsync();
        if (fromLocation == null)
            return ServiceResult<AdhocRouteCheckResult>.Ok(
                new AdhocRouteCheckResult { Reachable = false, Message = $"起始库位 {request.FromLocationCode} 不存在" });

        var toLocation = await db.Queryable<MdLocation>()
            .Where(l => l.LocationCode == request.ToLocationCode)
            .FirstAsync();
        if (toLocation == null)
            return ServiceResult<AdhocRouteCheckResult>.Ok(
                new AdhocRouteCheckResult { Reachable = false, Message = $"目标库位 {request.ToLocationCode} 不存在" });

        // 同库区，可达
        if (fromLocation.ZoneId == toLocation.ZoneId && fromLocation.ZoneId != null)
            return ServiceResult<AdhocRouteCheckResult>.Ok(
                new AdhocRouteCheckResult { Reachable = true, Message = "同库区内，路线可达" });

        // 不同库区但同一仓库：检查两个库区是否都存在且均未禁用
        if (fromLocation.WarehouseId == toLocation.WarehouseId)
        {
            var fromZone = await db.Queryable<MdWarehouseZone>()
                .Where(z => z.Id == fromLocation.ZoneId)
                .FirstAsync();
            var toZone = await db.Queryable<MdWarehouseZone>()
                .Where(z => z.Id == toLocation.ZoneId)
                .FirstAsync();

            if (fromZone == null || toZone == null)
                return ServiceResult<AdhocRouteCheckResult>.Ok(
                    new AdhocRouteCheckResult { Reachable = false, Message = "库区信息不完整，无法校验" });

            if (fromZone.Status == AlgoConstants.Status.DISABLED || toZone.Status == AlgoConstants.Status.DISABLED)
                return ServiceResult<AdhocRouteCheckResult>.Ok(
                    new AdhocRouteCheckResult { Reachable = false, Message = "库区已禁用，路线不可达" });

            return ServiceResult<AdhocRouteCheckResult>.Ok(
                new AdhocRouteCheckResult { Reachable = true, Message = "跨库区同仓库，路线可达" });
        }

        // 不同仓库，不可达
        return ServiceResult<AdhocRouteCheckResult>.Ok(
            new AdhocRouteCheckResult { Reachable = false, Message = "跨仓库，路线不可达" });
    }

}
