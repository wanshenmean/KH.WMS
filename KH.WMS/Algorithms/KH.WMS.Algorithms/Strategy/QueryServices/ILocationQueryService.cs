using KH.WMS.Algorithms.Strategy.DTOs;

namespace KH.WMS.Algorithms.Strategy.QueryServices
{
    /// <summary>
    /// 货位查询服务接口
    /// </summary>
    public interface ILocationQueryService
    {
        /// <summary>
        /// 查询空闲可用货位
        /// </summary>
        Task<List<MdLocationDTO>> GetEmptyLocationsAsync(long warehouseId, long? zoneId = null,
            string? locationType = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// 查询指定库区的货位（含占用状态）
        /// </summary>
        Task<List<MdLocationDTO>> GetLocationsByZoneAsync(long zoneId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 查询某货位附近的空闲货位（同巷道优先，其次同库区）
        /// </summary>
        Task<List<MdLocationDTO>> GetLocationsNearAsync(string locationCode, long warehouseId,
            int maxCount = 10, CancellationToken cancellationToken = default);

        /// <summary>
        /// 查询已有指定物料库存的货位编码列表
        /// </summary>
        Task<List<string>> GetLocationCodesWithInventoryAsync(long warehouseId, long materialId,
            string? batchNo = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// 查询指定货位的前排/后排配对货位
        /// 根据相同仓库+巷道+排+列+层，不同Depth查找
        /// </summary>
        /// <param name="locationId">参考货位ID</param>
        /// <param name="cancellationToken"></param>
        /// <returns>配对货位（可能为null，单深库位无配对）</returns>
        Task<MdLocationDTO?> GetPairLocationAsync(long locationId, CancellationToken cancellationToken = default);

        /// <summary>
        /// 检查指定货位的前排货位是否被占用（用于双深货位后排分配前验证）
        /// </summary>
        Task<string?> GetFrontLocationStatusAsync(long locationId, CancellationToken cancellationToken = default);
    }
}
