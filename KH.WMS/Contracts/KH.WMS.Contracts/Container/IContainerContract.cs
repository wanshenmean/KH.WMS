namespace KH.WMS.Contracts.Container;

/// <summary>
/// 容器跨模块接口契约
/// 供 Inbound/Outbound 等模块操作容器
/// BaseDataModule 负责实现
/// </summary>
public interface IContainerContract
{
    /// <summary>
    /// 根据容器号批量注册容器（已存在的会跳过）
    /// </summary>
    /// <param name="containerNos">容器编号列表</param>
    /// <returns>新注册的容器数量</returns>
    Task<int> RegisterContainersAsync(List<string> containerNos);

    /// <summary>
    /// 批量更新容器状态
    /// </summary>
    /// <param name="containerNos">容器编号列表</param>
    /// <param name="status">目标状态</param>
    Task UpdateStatusAsync(List<string> containerNos, string status);
}
