using KH.WMS.Core.Models;

namespace KH.WMS.Contracts.Tasks;

/// <summary>
/// 任务跨模块接口契约
/// 供 Inbound、Outbound 等模块创建任务
/// TaskModule 负责实现
/// </summary>
public interface ITaskContract
{
    /// <summary>
    /// 创建上架任务（任务头 + 任务行）
    /// </summary>
    Task<ServiceResult<string>> CreatePutawayTaskAsync(PutawayTaskRequest request);

    /// <summary>
    /// 创建拣货任务（任务头 + 任务行）
    /// </summary>
    Task<ServiceResult<string>> CreatePickingTaskAsync(PickingTaskRequest request);
}
