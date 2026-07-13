using KH.WMS.Core.Api.Responses;
using KH.WMS.Core.Controllers;
using KH.WMS.Core.Models;
using KH.WMS.Entities.Task;
using KH.WMS.Modules.TaskModule.DTOs;
using KH.WMS.Modules.TaskModule.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace KH.WMS.Modules.TaskModule.Controllers
{
    [Route("api/task-header")]
    public class TaskHeaderController(ITaskHeaderService taskHeaderService) : CrudController<TaskHeader>(taskHeaderService)
    {
        /// <summary>
        /// WCS任务完成回调（WCS设备搬运完成后通知WMS）
        /// </summary>
        [HttpPost("complete-by-wcs")]
        public async Task<ApiResponse> CompleteByWcs([FromBody] WcsTaskCompleteDto dto)
        {
            var result = await taskHeaderService.CompleteTaskByWcsAsync(dto);
            if (result.Success)
                return ApiResponse.Ok(message: result.Message);
            return ApiResponse.Fail(ResponseCode.BAD_REQUEST, result.Message ?? "任务完成失败");
        }

        /// <summary>
        /// 取消任务（仅 PENDING 状态可取消）
        /// </summary>
        [HttpPost("cancel/{id}")]
        public async Task<ApiResponse> Cancel(long id)
        {
            var result = await taskHeaderService.CancelTaskAsync(id);
            if (result.Success)
                return ApiResponse.Ok(message: result.Message);
            return ApiResponse.Fail(ResponseCode.BAD_REQUEST, result.Message ?? "任务取消失败");
        }

        /// <summary>
        /// 上架任务货位分配（Phase 2：WCS在接驳口调用，为上架任务分配具体目标库位）
        /// </summary>
        [HttpPost("allocate-putaway-location/{id}")]
        public async Task<ApiResponse> AllocatePutawayLocation(long id)
        {
            var result = await taskHeaderService.AllocatePutawayLocationAsync(id);
            if (result.Success)
                return ApiResponse.Ok(message: result.Message, data: result.Data);
            return ApiResponse.Fail(ResponseCode.BAD_REQUEST, result.Message ?? "货位分配失败");
        }
    }
}
