using KH.WMS.Algorithms.Strategy.DTOs;

namespace KH.WMS.Algorithms.Strategy.QueryServices
{
    /// <summary>
    /// 仓库查询服务接口
    /// </summary>
    public interface IWarehouseQueryService
    {
        /// <summary>
        /// 获取物料ABC周转分类
        /// </summary>
        Task<string?> GetMaterialTurnoverClassAsync(long materialId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 根据ABC分类获取匹配的库区列表
        /// </summary>
        Task<List<MdWarehouseZoneDTO>> GetZonesByAbcClassAsync(long warehouseId, string abcClass,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 获取库区下可用巷道列表
        /// </summary>
        Task<List<MdAisleDTO>> GetAislesByZoneAsync(long zoneId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 获取巷道负载（已占用货位数/总货位数）
        /// </summary>
        Task<(int Total, int Occupied)> GetAisleLoadAsync(int aisleNo, long warehouseId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 获取负载最低的巷道
        /// </summary>
        Task<MdAisleDTO?> GetLeastLoadedAisleAsync(long warehouseId, long? zoneId = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 根据单据类型获取推荐站台
        /// </summary>
        Task<MdPortDTO?> GetPortByDocTypeAsync(long docTypeId, string direction, long warehouseId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 获取仓库下所有启用的库区
        /// </summary>
        Task<List<MdWarehouseZoneDTO>> GetZonesByWarehouseAsync(long warehouseId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 获取仓库下可用于上架的存储类库区（STORAGE/ASRS/FLAT/RAW_MATERIAL）
        /// 只返回有巷道和货位的存储区域，排除收货区、发货区、暂存区等功能区域
        /// </summary>
        Task<List<MdWarehouseZoneDTO>> GetStorageZonesAsync(long warehouseId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 获取仓库下指定逻辑分区类型的逻辑分区及其物理库区映射
        /// zoneType 为空时返回所有逻辑分区
        /// </summary>
        Task<List<LogicalZoneDTO>> GetLogicalZonesAsync(long warehouseId, string? zoneType = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 根据巷道ID获取入库接驳口（服务该巷道的 INBOUND 或 MIXED 类型接驳口）
        /// </summary>
        Task<MdTransferPointDTO?> GetInboundTransferPointAsync(long warehouseId, long aisleId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 根据输送线ID获取入库口（该输送线上的 INBOUND 或 MIXED 类型站台）
        /// </summary>
        Task<MdPortDTO?> GetInboundPortByConveyorAsync(long warehouseId, long conveyorLineId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 根据巷道号获取出库口（巷道 → 接驳口 → 输送线 → 站台链路查找）
        /// 优先 OUTBOUND 类型，其次 MIXED 类型
        /// </summary>
        Task<MdPortDTO?> GetOutboundPortByAisleAsync(long warehouseId, int aisleNo,
            CancellationToken cancellationToken = default);
    }
}
