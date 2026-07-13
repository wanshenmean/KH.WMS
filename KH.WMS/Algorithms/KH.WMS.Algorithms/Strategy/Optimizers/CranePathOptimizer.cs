using KH.WMS.Algorithms.Strategy.Constants;
using KH.WMS.Algorithms.Strategy.Strategies;

namespace KH.WMS.Algorithms.Strategy.Optimizers
{
    /// <summary>
    /// 堆垛机路径优化器
    /// 针对 ASRS（自动化高位立库）场景，优化堆垛机的取货路径
    ///
    /// 核心原则：
    /// - 堆垛机水平移动速度快于垂直移动，同一巷道内优先按列排序再按层排序
    /// - 不同巷道之间不能直接跨越，需返回巷道口再进入另一巷道
    /// - S型路线：奇数层正向（列升序），偶数层反向（列降序），减少空跑
    /// - Z型路线：每层都正向
    /// - U型路线：先到最远端再逐层返回
    ///
    /// 使用方式：传入 PickingTaskItem 列表，返回排序后的列表
    /// </summary>
    public static class CranePathOptimizer
    {
        /// <summary>
        /// 对下架任务列表进行路径优化排序
        /// </summary>
        /// <param name="tasks">原始任务列表</param>
        /// <param name="pathMode">路径优化模式（S_SHAPE / Z_SHAPE / U_SHAPE）</param>
        /// <returns>按最优路径排序后的任务列表</returns>
        public static List<PickingTaskItem> Optimize(List<PickingTaskItem> tasks, string pathMode)
        {
            if (tasks == null || tasks.Count <= 1)
                return tasks ?? new();

            var items = tasks.Select(ParseLocation).ToList();

            // #29：pathMode 为空时回退默认 S 型，避免 ToUpperInvariant 抛 NRE
            var mode = string.IsNullOrWhiteSpace(pathMode)
                ? AlgoConstants.PathOptimization.S_SHAPE
                : pathMode.ToUpperInvariant();

            // #31：无法解析库位编码的任务（核心坐标缺失）排到路径末尾，避免污染前段路径顺序
            var valid = items.Where(i => i.IsValid).ToList();
            var invalid = items.Where(i => !i.IsValid).ToList();

            var ordered = mode switch
            {
                AlgoConstants.PathOptimization.S_SHAPE => OptimizeSShape(valid),
                AlgoConstants.PathOptimization.Z_SHAPE => OptimizeZShape(valid),
                AlgoConstants.PathOptimization.U_SHAPE => OptimizeUShape(valid),
                _ => OptimizeSShape(valid) // 默认 S 型
            };

            ordered.AddRange(invalid.Select(i => i.Task));
            return ordered;
        }

        /// <summary>
        /// S型路线优化
        /// 奇数层正向（列升序），偶数层反向（列降序）
        /// 减少堆垛机在同一层内的往返空跑距离
        /// </summary>
        private static List<PickingTaskItem> OptimizeSShape(List<LocationParseItem> items)
        {
            return items
                .OrderBy(i => i.AisleNo)                    // 按巷道分组（不同巷道间必须切换）
                .ThenBy(i => i.LayerNo)                      // 按层号排序
                .ThenBy(i => i.LayerNo % 2 == 1
                    ? i.ColNo                                // 奇数层：列升序（正向）
                    : -i.ColNo)                              // 偶数层：列降序（反向）
                .ThenBy(i => i.RowNo)                        // 同位置按排号
                .ThenBy(i => i.Depth)                        // #49：前排(1)先于后排(2)
                .ThenBy(i => i.Side)                         // 左右排兜底
                .Select(i => i.Task)
                .ToList();
        }

        /// <summary>
        /// Z型路线优化
        /// 每层都按列升序排列（水平优先于垂直）
        /// 适合货架较矮、巷道较长的场景
        /// </summary>
        private static List<PickingTaskItem> OptimizeZShape(List<LocationParseItem> items)
        {
            return items
                .OrderBy(i => i.AisleNo)
                .ThenBy(i => i.LayerNo)
                .ThenBy(i => i.ColNo)
                .ThenBy(i => i.RowNo)
                .ThenBy(i => i.Depth)        // #49：前排先于后排
                .ThenBy(i => i.Side)
                .Select(i => i.Task)
                .ToList();
        }

        /// <summary>
        /// U型路线优化（#30：修正为"先到最远端再逐层折返"的蛇形/犁田式）
        /// 按巷道分组；巷道内以层为主序，同层内列方向按层奇偶交替（偶数层列升序、奇数层列降序），
        /// 使堆垛机到达最远列后上一层折返返回，真正减少空跑。
        /// 适合同一巷道内有大量取货点的场景。
        /// </summary>
        private static List<PickingTaskItem> OptimizeUShape(List<LocationParseItem> items)
        {
            var grouped = items
                .GroupBy(i => i.AisleNo)
                .OrderBy(g => g.Key)
                .ToList();

            var result = new List<PickingTaskItem>();
            foreach (var group in grouped)
            {
                var layerIndex = 0;
                foreach (var layer in group.GroupBy(i => i.LayerNo).OrderBy(g => g.Key))
                {
                    var layerItems = layerIndex % 2 == 0
                        ? layer.OrderBy(i => i.ColNo).ThenBy(i => i.RowNo).ThenBy(i => i.Depth).ThenBy(i => i.Side)
                        : layer.OrderByDescending(i => i.ColNo).ThenBy(i => i.RowNo).ThenBy(i => i.Depth).ThenBy(i => i.Side);
                    result.AddRange(layerItems.Select(i => i.Task));
                    layerIndex++;
                }
            }

            return result;
        }

        /// <summary>
        /// 解析库位编码，提取地址坐标
        /// 库位编码格式: WarehouseCode-AisleNo-RowNo-ColNo-LayerNo-Side-Depth
        /// 例如: WH01-1-1-5-3-1-1 表示 WH01仓库 巷道1 排1 列5 层3 左排 前排
        /// </summary>
        private static LocationParseItem ParseLocation(PickingTaskItem task)
        {
            var item = new LocationParseItem { Task = task };

            if (string.IsNullOrWhiteSpace(task.FromLocationCode))
                return item;

            var parts = task.FromLocationCode.Split('-');
            if (parts.Length >= 5)
            {
                // #31：核心坐标（巷道/列/层）解析成功才标记有效，无效编码将被排到路径末尾
                int aisleNo = 0, colNo = 0, layerNo = 0;
                item.IsValid =
                    int.TryParse(parts[1], out aisleNo) &&
                    int.TryParse(parts[3], out colNo) &&
                    int.TryParse(parts[4], out layerNo);
                item.AisleNo = aisleNo;
                item.ColNo = colNo;
                item.LayerNo = layerNo;
                if (int.TryParse(parts[2], out int rowNo)) item.RowNo = rowNo;
            }
            if (parts.Length >= 6)
            {
                if (int.TryParse(parts[5], out int side)) item.Side = side;
            }
            if (parts.Length >= 7)
            {
                if (int.TryParse(parts[6], out int depth)) item.Depth = depth;
            }

            return item;
        }

        /// <summary>
        /// 解析后的库位坐标项
        /// </summary>
        private class LocationParseItem
        {
            public PickingTaskItem Task { get; set; } = null!;
            public int AisleNo { get; set; }
            public int RowNo { get; set; }
            public int ColNo { get; set; }
            public int LayerNo { get; set; }
            public int Side { get; set; }
            public int Depth { get; set; }
            /// <summary>核心坐标是否解析成功（#31：失败项排到路径末尾）</summary>
            public bool IsValid { get; set; }
        }
    }
}
