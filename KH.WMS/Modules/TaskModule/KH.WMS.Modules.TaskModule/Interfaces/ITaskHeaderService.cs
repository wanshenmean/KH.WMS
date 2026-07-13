using KH.WMS.Core.Models;
using KH.WMS.Core.Services;
using KH.WMS.Entities.Task;
using KH.WMS.Modules.TaskModule.DTOs;

namespace KH.WMS.Modules.TaskModule.Interfaces
{
    public interface ITaskHeaderService : ICrudService<TaskHeader>
    {
        /// <summary>
        /// WCS回调完成任务（WCS设备搬运完成后通知WMS）
        /// </summary>
        Task<ServiceResult> CompleteTaskByWcsAsync(WcsTaskCompleteDto dto);

        /// <summary>
        /// 取消任务（仅 PENDING 状态可取消，PUTAWAY 任务会回滚组盘/库位/容器状态）
        /// </summary>
        Task<ServiceResult> CancelTaskAsync(long taskId);

        /// <summary>
        /// 上架任务货位分配（Phase 2：WCS在接驳口调用，为上架任务分配具体目标库位）
        /// </summary>
        Task<ServiceResult<string>> AllocatePutawayLocationAsync(long taskId);
    }
}
