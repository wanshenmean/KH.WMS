using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KH.WMS.Algorithms.Strategy.DTOs;

namespace KH.WMS.Algorithms.Strategy.QueryServices
{
    public interface IInventoryQueryService
    {
        /// <summary>
        /// 获取按FIFO排序的库存列表
        /// </summary>
        Task<List<InventoryInfoDTO>> GetByFIFOAsync(string warehouseCode, string materialCode, CancellationToken cancellationToken = default);

        /// <summary>
        /// 获取按FEFO排序的库存列表（按过期时间排序）
        /// </summary>
        Task<List<InventoryInfoDTO>> GetByFEFOAsync(string warehouseCode, string materialCode, CancellationToken cancellationToken = default);

        /// <summary>
        /// 获取指定批次的库存列表
        /// </summary>
        Task<List<InventoryInfoDTO>> GetByBatchAsync(string warehouseCode, string materialCode, string batchNo, CancellationToken cancellationToken = default);

        /// <summary>
        /// 获取指定物料的库存列表
        /// </summary>
        Task<List<InventoryInfoDTO>> GetByMaterialAsync(string warehouseCode, string materialCode, CancellationToken cancellationToken = default);

        /// <summary>
        /// 获取指定货位的库存列表
        /// </summary>
        Task<List<InventoryInfoDTO>> GetByLocationAsync(string locationCode, CancellationToken cancellationToken = default);

        /// <summary>
        /// 获取指定区域的库存列表
        /// </summary>
        Task<List<InventoryInfoDTO>> GetByAreaAsync(string warehouseCode, string areaCode, string materialCode, CancellationToken cancellationToken = default);
    }
}
