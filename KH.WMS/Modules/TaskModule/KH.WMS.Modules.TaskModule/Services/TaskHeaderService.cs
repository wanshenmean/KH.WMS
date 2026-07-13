using SqlSugar;
using KH.WMS.Algorithms.Strategy;
using KH.WMS.Algorithms.Strategy.Constants;
using KH.WMS.Algorithms.Strategy.Enums;
using KH.WMS.Algorithms.Strategy.Interfaces;
using KH.WMS.Algorithms.Strategy.QueryServices;
using KH.WMS.Algorithms.Strategy.Strategies;
using KH.WMS.Contracts.Container;
using KH.WMS.Config.Abstractions;
using KH.WMS.Contracts.Inbound;
using KH.WMS.Contracts.Inventory;
using KH.WMS.Contracts.Outbound;
using KH.WMS.Contracts.Warehouse;
using KH.WMS.Core.Database.Repositories;
using KH.WMS.Core.Database.UnitOfWorks;
using KH.WMS.Core.DependencyInjection.ServiceLifetimes;
using KH.WMS.Core.Logging.WMSError;
using KH.WMS.Core.Models;
using KH.WMS.Core.Services;
using static KH.WMS.Core.Logging.WMSError.WMSErrorCodes;
using KH.WMS.Entities.Constants;
using KH.WMS.Entities.Inbound;
using KH.WMS.Entities.Inventory;
using KH.WMS.Entities.Task;
using KH.WMS.Entities.Warehouse;
using KH.WMS.Modules.TaskModule.DTOs;
using KH.WMS.Modules.TaskModule.Interfaces;

namespace KH.WMS.Modules.TaskModule.Services
{
    /// <summary>
    /// 货位分配策略输入参数
    /// </summary>
    internal class LocationAllocationStrategyInput
    {
        public long WarehouseId { get; init; }
        public long? ZoneId { get; init; }
        public string BusinessCode { get; init; } = string.Empty;
        public string DocType { get; init; } = string.Empty;
    }

    /// <summary>
    /// 货位分配策略输出
    /// </summary>
    internal class LocationAllocationStrategyOutput
    {
        public long LocationId { get; init; }
        public string LocationCode { get; init; } = string.Empty;
    }
    [RegisteredService(ServiceType = typeof(ITaskHeaderService))]
    public class TaskHeaderService(
        IRepository<TaskHeader, long> repository,
        IUnitOfWork unitOfWork,
        IDetailSaveService detailSaveService,
        IContainerContract containerContract,
        IInventoryContract inventoryContract,
        ILocationContract locationContract,
        IInboundOrderContract inboundOrderContract,
        IOutboundContract outboundContract,
        IPolicyEngine policyEngine,
        ILocationQueryService locationQueryService,
        IWarehouseQueryService warehouseQueryService,
        IConfigResolverContract configService) : CrudService<TaskHeader>(repository, unitOfWork, detailSaveService), ITaskHeaderService
    {
        private readonly IContainerContract _containerContract = containerContract;
        private readonly IInventoryContract _inventoryContract = inventoryContract;
        private readonly ILocationContract _locationContract = locationContract;
        private readonly IInboundOrderContract _inboundOrderContract = inboundOrderContract;
        private readonly IOutboundContract _outboundContract = outboundContract;
        private readonly IPolicyEngine _policyEngine = policyEngine;
        private readonly ILocationQueryService _locationQueryService = locationQueryService;
        private readonly IWarehouseQueryService _warehouseQueryService = warehouseQueryService;

        /// <inheritdoc />
        public async Task<ServiceResult> CompleteTaskByWcsAsync(WcsTaskCompleteDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.TaskNo) && string.IsNullOrWhiteSpace(dto.WcsTaskNo))
                return ServiceResult.Fail(WMSErrorMessages.GetMessage(TASK_AT_LEAST_ONE_ID));

            try
            {
                await unitOfWork.BeginTransactionAsync();

                // Step 1: 查找任务
                var taskRepo = unitOfWork.GetRepository<TaskHeader, long>();
                var findResult = await FindTaskByWcsDtoAsync(dto, taskRepo);
                if (findResult.Error != null)
                {
                    await unitOfWork.RollbackAsync();
                    return ServiceResult.Fail(findResult.Error);
                }

                var task = findResult.Task!;

                // 幂等/并发防护：对任务行加更新锁序列化并发 WCS 回调，并复核状态。
                // 第一个回调处理完(提交)后，并发回调拿到锁会看到已完成/失败状态 → 幂等返回成功，不重复触发副作用。
                var lockedTask = await unitOfWork.DbContext.Db.Queryable<TaskHeader>()
                    .Where(t => t.Id == task.Id)
                    .With(SqlWith.UpdLock)
                    .FirstAsync();
                if (lockedTask == null
                    || (lockedTask.TaskStatus != BizConstants.TaskStatus.PENDING
                        && lockedTask.TaskStatus != BizConstants.TaskStatus.IN_PROGRESS))
                {
                    await unitOfWork.RollbackAsync();
                    return ServiceResult.Ok($"任务 {task.TaskNo} 已处理，无需重复完成");
                }
                task = lockedTask;

                // Step 2: 按异常/正常分支处理
                ServiceResult result;
                if (dto.IsException)
                {
                    result = await HandleExceptionTaskAsync(task, dto, unitOfWork);
                }
                else
                {
                    result = await RecordNormalCompletionAsync(task, dto, unitOfWork);
                }

                if (!result.Success)
                {
                    await unitOfWork.RollbackAsync();
                    return result;
                }

                // Step 3: 按任务类型执行后续逻辑
                result = await PostTaskCompletionAsync(task, unitOfWork);
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
                return ServiceResult.Fail(WMSErrorMessages.GetMessage(TASK_COMPLETE_FAILED, ex.Message));
            }
        }

        // ==================== WCS 完成任务拆分方法 ====================

        /// <summary>
        /// 根据 DTO 查找任务并验证状态
        /// </summary>
        private async Task<(TaskHeader? Task, string? Error)> FindTaskByWcsDtoAsync(
            WcsTaskCompleteDto dto, IRepository<TaskHeader, long> taskRepo)
        {
            TaskHeader? task = null;

            if (!string.IsNullOrWhiteSpace(dto.TaskNo))
                task = await taskRepo.GetFirstOrDefaultAsync(t => t.TaskNo == dto.TaskNo);

            if (task == null && !string.IsNullOrWhiteSpace(dto.WcsTaskNo))
                task = await taskRepo.GetFirstOrDefaultAsync(t => t.WcsTaskNo == dto.WcsTaskNo);

            if (task == null)
                return (null, WMSErrorMessages.GetMessage(TASK_NOT_EXIST, $"TaskNo={dto.TaskNo}, WcsTaskNo={dto.WcsTaskNo}"));

            if (task.TaskStatus != BizConstants.TaskStatus.PENDING && task.TaskStatus != BizConstants.TaskStatus.IN_PROGRESS)
                return (null, WMSErrorMessages.GetMessage(TASK_STATUS_CANNOT_COMPLETE, task.TaskNo, task.TaskStatus));

            if (task.TaskType == BizConstants.TaskTypes.PUTAWAY && task.LocationAllocated == BizConstants.BoolFlag.NO)
                return (null, WMSErrorMessages.GetMessage(TASK_NOT_PUTAWAY, task.TaskNo));

            return (task, null);
        }

        /// <summary>
        /// 异常处理分支：标记任务失败 + 写入异常确认记录
        /// </summary>
        private async Task<ServiceResult> HandleExceptionTaskAsync(
            TaskHeader task, WcsTaskCompleteDto dto, IUnitOfWork uow)
        {
            var taskRepo = uow.GetRepository<TaskHeader, long>();
            task.MarkAsFailed();
            if (!string.IsNullOrWhiteSpace(dto.WcsTaskNo))
                task.WcsTaskNo = dto.WcsTaskNo;
            if (!string.IsNullOrWhiteSpace(dto.EquipmentCode))
                task.EquipmentCode = dto.EquipmentCode;
            await taskRepo.UpdateAsync(task);

            var confirmRepo = uow.GetRepository<TaskConfirm, long>();
            await confirmRepo.AddAsync(new TaskConfirm
            {
                TaskHeaderId = task.Id,
                ConfirmSource = BizConstants.ConfirmSource.WCS,
                IsException = BizConstants.BoolFlag.YES,
                ExceptionReason = dto.ExceptionReason,
                EquipmentCode = dto.EquipmentCode,
                WcsTaskNo = dto.WcsTaskNo,
                WcsActualPosition = dto.ActualPosition,
                WcsTravelTime = dto.TravelTime,
                WcsWeight = dto.Weight,
                ConfirmedTime = DateTime.Now,
            });

            return ServiceResult.Fail(WMSErrorMessages.GetMessage(TASK_EXCEPTION_OCCURRED, task.TaskNo, dto.ExceptionReason));
        }

        /// <summary>
        /// 正常完成分支：标记任务完成 + 写入正常确认记录
        /// </summary>
        private async Task<ServiceResult> RecordNormalCompletionAsync(
            TaskHeader task, WcsTaskCompleteDto dto, IUnitOfWork uow)
        {
            var taskRepo = uow.GetRepository<TaskHeader, long>();
            task.MarkAsCompleted();
            if (!string.IsNullOrWhiteSpace(dto.WcsTaskNo))
                task.WcsTaskNo = dto.WcsTaskNo;
            if (!string.IsNullOrWhiteSpace(dto.EquipmentCode))
                task.EquipmentCode = dto.EquipmentCode;
            await taskRepo.UpdateAsync(task);

            var taskConfirmRepo = uow.GetRepository<TaskConfirm, long>();
            await taskConfirmRepo.AddAsync(new TaskConfirm
            {
                TaskHeaderId = task.Id,
                ConfirmSource = BizConstants.ConfirmSource.WCS,
                IsException = BizConstants.BoolFlag.NO,
                EquipmentCode = dto.EquipmentCode,
                WcsTaskNo = dto.WcsTaskNo,
                WcsActualPosition = dto.ActualPosition,
                WcsTravelTime = dto.TravelTime,
                WcsWeight = dto.Weight,
                ConfirmedTime = DateTime.Now,
            });

            return ServiceResult.Ok();
        }

        /// <summary>
        /// 按任务类型执行后续逻辑：上架生成库存 / 下架移动库存位置 / 库位状态 / 容器状态 / 组盘状态
        /// </summary>
        private async Task<ServiceResult> PostTaskCompletionAsync(TaskHeader task, IUnitOfWork uow)
        {
            // 上架任务：生成库存
            if (task.TaskType == BizConstants.TaskTypes.PUTAWAY)
            {
                await GenerateInventoryViaContractAsync(task);
            }

            // 下架任务：将库存从存储货位移到拣货/暂存货位，不扣减库存
            if (task.TaskType == BizConstants.TaskTypes.PICKING)
            {
                if (task.FromLocationId.HasValue && task.ToLocationId.HasValue && !string.IsNullOrWhiteSpace(task.ContainerNo))
                {
                    await _inventoryContract.MoveInventoryLocationAsync(
                        new InventoryMoveLocationRequest
                        {
                            ContainerCode = task.ContainerNo,
                            WarehouseId = task.WarehouseId,
                            FromLocationId = task.FromLocationId.Value,
                            FromLocationCode = task.FromLocationCode ?? string.Empty,
                            ToLocationId = task.ToLocationId.Value,
                            ToLocationCode = task.ToLocationCode ?? string.Empty,
                            DocType = task.TaskType,
                            DocNo = task.TaskNo,
                        });
                }
            }

            // 更新库位状态
            if (task.TaskType == BizConstants.TaskTypes.PUTAWAY && task.ToLocationId.HasValue)
            {
                // 上架完成：释放锁定 + 目标库位从空闲 → 占用
                await _locationContract.UpdateLocationLockStatusAsync(task.ToLocationId.Value, BizConstants.LocationLockStatus.NONE);
                await _locationContract.UpdateLocationStatusAsync(task.ToLocationId.Value, BizConstants.LocationStatus.OCCUPIED);
            }
            else if (task.TaskType == BizConstants.TaskTypes.PICKING)
            {
                // 下架完成：原货位从占用 → 空闲可用
                if (task.FromLocationId.HasValue)
                    await _locationContract.UpdateLocationStatusAsync(task.FromLocationId.Value, BizConstants.LocationStatus.EMPTY);
                // 目标拣货位：释放锁定 + 标记为占用
                if (task.ToLocationId.HasValue)
                {
                    await _locationContract.UpdateLocationLockStatusAsync(task.ToLocationId.Value, BizConstants.LocationLockStatus.NONE);
                    await _locationContract.UpdateLocationStatusAsync(task.ToLocationId.Value, BizConstants.LocationStatus.OCCUPIED);
                }
            }

            // 更新容器状态
            if (!string.IsNullOrWhiteSpace(task.ContainerNo))
            {
                await _containerContract.UpdateStatusAsync(
                    new List<string> { task.ContainerNo }, BizConstants.ContainerStatus.IN_USE);
            }

            // 更新组盘状态（上架任务 → PUTTING_AWAY → PUT_AWAY）
            if (task.TaskType == BizConstants.TaskTypes.PUTAWAY && task.DocId.HasValue)
            {
                await _inboundOrderContract.MarkBindAsPutAwayAsync(task.DocId.Value);
            }

            return ServiceResult.Ok(WMSErrorMessages.GetMessage(TASK_COMPLETE_SUCCESS, task.TaskNo));
        }

        /// <summary>
        /// 无单据拣货完成：直接通过库存契约扣减库存（不经过出库契约）
        /// </summary>
        private async Task DeductAdhocPickingInventoryAsync(TaskHeader task, IUnitOfWork uow)
        {
            var taskLineRepo = uow.GetRepository<TaskLine, long>();
            var taskLines = await taskLineRepo.GetListAsync(l => l.TaskId == task.Id);

            foreach (var taskLine in taskLines)
            {
                if (taskLine.InventoryHeaderId.HasValue)
                {
                    // 根据库存头查询对应明细，找到匹配的物料行
                    var detailRepo = uow.GetRepository<KH.WMS.Entities.Inventory.InvInventoryDetail, long>();
                    var details = await detailRepo.GetListAsync(
                        d => d.HeaderId == taskLine.InventoryHeaderId.Value && d.MaterialId == taskLine.MaterialId);

                    foreach (var detail in details)
                    {
                        var deductQty = detail.LockedQty > 0 ? detail.LockedQty : detail.Qty;
                        if (deductQty > 0)
                        {
                            await _inventoryContract.DeductInventoryAsync(
                                new KH.WMS.Contracts.Inventory.InventoryDeductRequest
                                {
                                    InventoryDetailId = detail.Id,
                                    DeductQty = deductQty,
                                    WarehouseId = task.WarehouseId,
                                    DocType = task.DocType ?? BizConstants.DocTypes.ADHOC_OUTBOUND,
                                    DocNo = task.TaskNo,
                                    MovementType = BizConstants.MovementTypes.OUTBOUND,
                                });
                        }
                    }
                }
            }
        }

        /// <inheritdoc />
        public async Task<ServiceResult> CancelTaskAsync(long taskId)
        {
            try
            {
                await unitOfWork.BeginTransactionAsync();

                var taskRepo = unitOfWork.GetRepository<TaskHeader, long>();
                var task = await taskRepo.GetByIdAsync(taskId);

                if (task == null)
                    return ServiceResult.Fail(WMSErrorMessages.GetMessage(TASK_NOT_EXIST, taskId));

                if (!task.CanCancel)
                    return ServiceResult.Fail(WMSErrorMessages.GetMessage(TASK_STATUS_ERROR, task.TaskNo, task.TaskStatus));

                // 更新任务状态为 CANCELLED
                task.Cancel();
                await taskRepo.UpdateAsync(task);

                // 按任务类型回滚资源
                if (task.TaskType == BizConstants.TaskTypes.PUTAWAY)
                {
                    await RollbackPutawayResourcesAsync(task, unitOfWork);
                }
                else if (task.TaskType == BizConstants.TaskTypes.PICKING)
                {
                    await RollbackPickingResourcesAsync(task, unitOfWork);
                }

                await unitOfWork.CommitAsync();
                return ServiceResult.Ok(WMSErrorMessages.GetMessage(TASK_CANCEL_SUCCESS, task.TaskNo));
            }
            catch (Exception ex)
            {
                await unitOfWork.RollbackAsync();
                return ServiceResult.Fail(WMSErrorMessages.GetMessage(TASK_CANCEL_FAILED, ex.Message));
            }
        }

        // ==================== 取消任务资源回滚方法 ====================

        /// <summary>
        /// 上架取消回滚：库位 + 容器 + 组盘
        /// </summary>
        private async Task RollbackPutawayResourcesAsync(TaskHeader task, IUnitOfWork uow)
        {
            // 1. 释放实际目标库位（仅当 Phase 2 已分配实际货位时才释放）
            if (task.LocationAllocated == BizConstants.BoolFlag.YES && task.ToLocationId.HasValue)
            {
                await _locationContract.UpdateLocationLockStatusAsync(
                    task.ToLocationId.Value, BizConstants.LocationLockStatus.NONE);
            }

            // 2. 回退容器状态：MOVING → IN_USE
            if (!string.IsNullOrWhiteSpace(task.ContainerNo))
            {
                await _containerContract.UpdateStatusAsync(
                    new List<string> { task.ContainerNo }, BizConstants.ContainerStatus.IN_USE);
            }

            // 3. 回退组盘状态：PUTTING_AWAY → BOUND
            if (task.DocId.HasValue)
            {
                await _inboundOrderContract.RevertBindToBoundAsync(task.DocId.Value);
            }
        }

        /// <summary>
        /// 下架取消回滚：释放目标库位预留状态，并释放该任务对应分配明细锁定的库存
        /// </summary>
        private async Task RollbackPickingResourcesAsync(TaskHeader task, IUnitOfWork uow)
        {
            // 1. 释放目标拣货位的锁定状态
            if (task.ToLocationId.HasValue)
            {
                await _locationContract.UpdateLocationLockStatusAsync(
                    task.ToLocationId.Value, BizConstants.LocationLockStatus.NONE);
            }

            // 2. 释放该任务对应分配明细锁定的库存（UnlockInventoryAsync），并把明细回退为 ALLOCATED。
            //    源货位状态/库存位置不变（货物未实际移出），仅回收锁定量，避免取消后 LockedQty 永久泄漏。
            await _outboundContract.ResetAllocationByTaskAsync(task.Id);
        }

        /// <inheritdoc />
        public async Task<ServiceResult<string>> AllocatePutawayLocationAsync(long taskId)
        {
            try
            {
                await unitOfWork.BeginTransactionAsync();

                var taskRepo = unitOfWork.GetRepository<TaskHeader, long>();

                // 验证任务
                var task = await ValidateTaskForLocationAllocationAsync(taskId, taskRepo);
                if (task == null)
                    return ServiceResult<string>.Fail(WMSErrorMessages.GetMessage(TASK_NOT_EXIST, taskId));

                // 执行货位分配策略
                var strategyInput = new LocationAllocationStrategyInput
                {
                    WarehouseId = task.WarehouseId,
                    ZoneId = task.ToZoneId,
                    BusinessCode = task.DocNo ?? string.Empty,
                    DocType = task.DocType ?? string.Empty,
                };
                var (strategyError, strategyOutput) = await ExecuteLocationAllocationStrategyAsync(
                    strategyInput, task.ToZoneId, task.TaskNo);
                if (strategyOutput == null)
                    return ServiceResult<string>.Fail(strategyError!);


                // 更新任务目标库位 + 预留库位状态
                await ApplyLocationAllocationAsync(task, strategyOutput, taskRepo);

                await unitOfWork.CommitAsync();
                return ServiceResult<string>.Ok(strategyOutput.LocationCode, WMSErrorMessages.GetMessage(TASK_LOCATION_ALLOCATE_SUCCESS, strategyOutput.LocationCode));

            }
            catch (Exception ex)
            {
                await unitOfWork.RollbackAsync();
                return ServiceResult<string>.Fail(WMSErrorMessages.GetMessage(TASK_LOCATION_ALLOCATE_FAILED, ex.Message));
            }
        }

        // ==================== 验证方法 ====================

        /// <summary>
        /// 验证任务是否满足货位分配条件
        /// </summary>
        private async Task<TaskHeader?> ValidateTaskForLocationAllocationAsync(long taskId, IRepository<TaskHeader, long> taskRepo)
        {
            var task = await taskRepo.GetByIdAsync(taskId);
            if (task == null)
                return null;

            if (task.TaskType != BizConstants.TaskTypes.PUTAWAY)
                throw new InvalidOperationException(WMSErrorMessages.GetMessage(TASK_LOCATION_NOT_ALLOCATED, task.TaskNo));

            if (task.TaskStatus != BizConstants.TaskStatus.PENDING && task.TaskStatus != BizConstants.TaskStatus.IN_PROGRESS)
                throw new InvalidOperationException(WMSErrorMessages.GetMessage(TASK_STATUS_CANNOT_ALLOCATE, task.TaskNo, task.TaskStatus));

            if (task.LocationAllocated == BizConstants.BoolFlag.YES)
                throw new InvalidOperationException(WMSErrorMessages.GetMessage(TASK_ALREADY_ALLOCATED, task.TaskNo, task.ToLocationCode));

            return task;
        }

        // ==================== 策略执行方法 ====================

        /// <summary>
        /// 执行货位分配策略
        /// </summary>
        private async Task<(string? Error, LocationAllocationStrategyOutput? Output)> ExecuteLocationAllocationStrategyAsync(
            LocationAllocationStrategyInput input, long? zoneId, string taskNo)
        {
            var context = new PolicyContext
            {
                WarehouseId = input.WarehouseId,
                ZoneId = input.ZoneId,
                BusinessCode = input.BusinessCode,
                DocType = input.DocType,
            };

            if (zoneId.HasValue)
                context.SetData(StrategyParams.PutawayInput.TARGET_ZONE_ID, zoneId.Value);

            var policyResult = await _policyEngine.ExecuteAsync(PolicyType.LocationAllocation, context);
            if (!policyResult.IsSuccess)
                return (WMSErrorMessages.GetMessage(TASK_LOCATION_STRATEGY_FAILED, taskNo, policyResult.ErrorMessage), null);

            var allocationOutput = policyResult.Output as LocationAllocationResult;
            if (allocationOutput == null || allocationOutput.Locations.Count == 0)
                return (WMSErrorMessages.GetMessage(TASK_LOCATION_NO_AVAILABLE, taskNo), null);

            var targetLocation = allocationOutput.Locations[0];
            return (null, new LocationAllocationStrategyOutput
            {
                LocationId = long.Parse(targetLocation.LocationId),
                LocationCode = targetLocation.LocationCode,
            });
        }

        /// <summary>
        /// 应用货位分配结果：更新任务目标库位 + 预留库位状态
        /// </summary>
        private async Task ApplyLocationAllocationAsync(
            TaskHeader task, LocationAllocationStrategyOutput output, IRepository<TaskHeader, long> taskRepo)
        {
            task.ToLocationId = output.LocationId;
            task.ToLocationCode = output.LocationCode;
            task.LocationAllocated = BizConstants.BoolFlag.YES;
            await taskRepo.UpdateAsync(task);

            await _locationContract.UpdateLocationLockStatusAsync(output.LocationId, BizConstants.LocationLockStatus.INBOUND_RESERVED);
        }

        /// <summary>
        /// 通过契约生成库存记录（任务行 + 组盘数据 → 库存数据）
        /// </summary>
        private async Task GenerateInventoryViaContractAsync(TaskHeader task)
        {
            // 查询任务行（本模块实体，OK）
            var taskLineRepo = unitOfWork.GetRepository<TaskLine, long>();
            var taskLines = await taskLineRepo.GetListAsync(l => l.TaskId == task.Id);

            if (taskLines == null || taskLines.Count == 0)
                return;

            // 通过契约获取组盘明细和来源单号
            List<KH.WMS.Contracts.Inbound.BindDetailData>? bindDetails = null;
            string? sourceDocNo = null;

            if (task.DocId.HasValue)
            {
                bindDetails = await _inboundOrderContract.GetBindDetailsAsync(task.DocId.Value);
                sourceDocNo = await _inboundOrderContract.GetBindSourceDocNoAsync(task.DocId.Value);
            }

            // 构建库存生成请求
            var request = new KH.WMS.Contracts.Inventory.InventoryGenerationRequest
            {
                ContainerCode = task.ContainerNo ?? string.Empty,
                WarehouseId = task.WarehouseId,
                LocationId = task.ToLocationId,
                LocationCode = task.ToLocationCode,
                SourceDocNo = sourceDocNo,
                Lines = taskLines.Select(line =>
                {
                    var bindDetail = bindDetails?.FirstOrDefault(d => d.MaterialId == line.MaterialId);
                    return new KH.WMS.Contracts.Inventory.InventoryLineRequest
                    {
                        MaterialId = line.MaterialId,
                        MaterialCode = line.MaterialCode ?? string.Empty,
                        BatchNo = line.BatchNo ?? bindDetail?.BatchNo,
                        Qty = bindDetail?.Qty ?? 0,
                        ProductionDate = bindDetail?.ProductionDate,
                        ExpiryDate = bindDetail?.ExpiryDate,
                        InboundDocNo = sourceDocNo ?? task.DocNo,
                    };
                }).ToList(),
            };

            // 通过契约生成库存
            var headerId = await _inventoryContract.GenerateInventoryFromPutawayAsync(request);

            // 回写 TaskLine 的 InventoryHeaderId，建立追溯链
            foreach (var line in taskLines)
            {
                line.InventoryHeaderId = headerId;
            }
            await taskLineRepo.UpdateAsync(taskLines);
        }
    }
}
