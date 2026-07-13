using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KH.WMS.Algorithms.Strategy.Enums
{
    /// <summary>
    /// 策略类型枚举
    /// </summary>
    public enum PolicyType
    {
        /// <summary>
        /// 上架策略（定义物料入库上架时的作业规则和控制逻辑）
        /// </summary>
        Putaway = 1,

        /// <summary>
        /// 货位分配策略（入库上架时根据物料属性、仓库布局等因素计算最优货位）
        /// </summary>
        LocationAllocation = 2,

        /// <summary>
        /// 下架策略（高位立库中物料从货位移出到出库站台的控制逻辑）
        /// </summary>
        Picking = 3,

        /// <summary>
        /// 库存分配策略（下架时选择最优库存，如先进先出、先过期先出、按批次出库）
        /// </summary>
        InventoryAllocation = 4,

        /// <summary>
        /// 波次策略
        /// </summary>
        Wave = 6,

        /// <summary>
        /// 【已废弃 A2】出库分配策略层已移除——出库改为单步 InventoryAllocation 全仓选批次，
        /// 原"整托优先/分区优先/散件"作为出库偏好（排序参数）下沉到 InventoryAllocation。
        /// 枚举值保留仅为兼容数据库中可能已存的 chain_type 字符串，不应再用于新建策略链。
        /// </summary>
        [Obsolete("出库分配层已移除(A2)，出库统一走 InventoryAllocation。保留枚举值仅为兼容历史 chain_type 数据。")]
        OutboundAllocation = 7
    }
}
