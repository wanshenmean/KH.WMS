using KH.WMS.Contracts.Tasks;
using KH.WMS.Core.Database.UnitOfWorks;
using KH.WMS.Core.DependencyInjection.ServiceLifetimes;
using KH.WMS.Core.Logging.WMSError;
using KH.WMS.Core.Models;
using KH.WMS.Core.Services;
using KH.WMS.Entities.Constants;
using KH.WMS.Entities.Task;
using Microsoft.Extensions.DependencyInjection;
using static KH.WMS.Core.Logging.WMSError.WMSErrorCodes;

namespace KH.WMS.Modules.TaskModule.Contracts
{
    [RegisteredService(Lifetime = ServiceLifetime.Scoped, ServiceType = typeof(ITaskContract))]
    public class TaskContract(
        IUnitOfWork unitOfWork,
        ICodeGeneratorService codeGenerator) : ITaskContract
    {
        /// <inheritdoc />
        public async Task<ServiceResult<string>> CreatePutawayTaskAsync(PutawayTaskRequest request)
        {
            var taskNo = await codeGenerator.GenerateAsync(BizConstants.CodeRuleTypes.TASK_NO);

            var taskHeader = new TaskHeader
            {
                TaskNo = taskNo,
                TaskType = BizConstants.TaskTypes.PUTAWAY,
                TaskPriority = BizConstants.TaskPriority.NORMAL,
                WarehouseId = request.WarehouseId,
                DocId = request.DocId,
                DocType = request.DocType ?? BizConstants.DocTypes.INBOUND_CONTAINER_BIND,
                DocNo = request.DocNo,
                TaskStatus = BizConstants.TaskStatus.PENDING,
                ExecutionMode = BizConstants.ExecutionMode.AUTO,
                TotalLines = request.Lines.Count,
                ContainerNo = request.ContainerNo,
                FromLocationCode = request.FromLocationCode,
                ToLocationId = request.ToLocationId,
                ToLocationCode = request.ToLocationCode,
                ToZoneId = request.ToZoneId,
            };

            var taskRepo = unitOfWork.GetRepository<TaskHeader, long>();
            var taskId = await taskRepo.AddAsync(taskHeader);

            var taskLineRepo = unitOfWork.GetRepository<TaskLine, long>();
            var lineNo = 1;
            var taskLines = request.Lines.Select(l => new TaskLine
            {
                TaskId = taskId,
                LineNo = lineNo++,
                MaterialId = l.MaterialId,
                MaterialCode = l.MaterialCode,
                MaterialName = l.MaterialName,
                BatchNo = l.BatchNo,
            }).ToList();

            await taskLineRepo.AddAsync(taskLines);

            return ServiceResult<string>.Ok(taskNo, WMSErrorMessages.GetMessage(PUTAWAY_TASK_CREATED_WITH_NO, taskNo));
        }

        /// <inheritdoc />
        public async Task<ServiceResult<string>> CreatePickingTaskAsync(PickingTaskRequest request)
        {
            var taskNo = await codeGenerator.GenerateAsync(BizConstants.CodeRuleTypes.TASK_NO);

            var taskHeader = new TaskHeader
            {
                TaskNo = taskNo,
                TaskType = BizConstants.TaskTypes.PICKING,
                TaskPriority = BizConstants.TaskPriority.NORMAL,
                WarehouseId = request.WarehouseId,
                DocId = request.DocId,
                DocType = request.DocType ?? BizConstants.DocTypes.OUTBOUND_ORDER,
                DocNo = request.DocNo,
                TaskStatus = BizConstants.TaskStatus.PENDING,
                ExecutionMode = BizConstants.ExecutionMode.AUTO,
                TotalLines = request.Lines.Count,
                ContainerId = request.ContainerId,
                ContainerNo = request.ContainerNo,
                FromLocationId = request.FromLocationId,
                FromLocationCode = request.FromLocationCode,
                FromZoneId = request.FromZoneId,
                ToLocationId = request.ToLocationId,
                ToLocationCode = request.ToLocationCode,
                ToZoneId = request.ToZoneId,
            };

            var taskRepo = unitOfWork.GetRepository<TaskHeader, long>();
            var taskId = await taskRepo.AddAsync(taskHeader);

            var taskLineRepo = unitOfWork.GetRepository<TaskLine, long>();
            var lineNo = 1;
            var taskLines = request.Lines.Select(l => new TaskLine
            {
                TaskId = taskId,
                LineNo = lineNo++,
                MaterialId = l.MaterialId,
                MaterialCode = l.MaterialCode,
                MaterialName = l.MaterialName,
                BatchNo = l.BatchNo,
                InventoryHeaderId = l.InventoryHeaderId,
            }).ToList();

            await taskLineRepo.AddAsync(taskLines);

            return ServiceResult<string>.Ok(taskNo, WMSErrorMessages.GetMessage(OUTBOUND_TASK_CREATED, 1));
        }
    }
}
