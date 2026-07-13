using System.Text.Json;
using KH.WMS.Algorithms.Strategy.Constants;
using KH.WMS.Algorithms.Strategy.DTOs;
using KH.WMS.Algorithms.Strategy.Enums;
using KH.WMS.Algorithms.Strategy.Interfaces;

namespace KH.WMS.Algorithms.Strategy.Strategies
{
    /// <summary>
    /// 库存分配策略基类
    /// 下架时选择最优库存，如先进先出、先过期先出、按批次出库
    /// </summary>
    public abstract class InventoryAllocationStrategyBase : PolicyStrategyBase
    {
        public sealed override PolicyType PolicyType => PolicyType.InventoryAllocation;

        protected abstract Task<InventoryAllocationResult> SelectInventoryAsync(IPolicyContext context, CancellationToken cancellationToken = default);

        public sealed override async Task<IPolicyResult> ExecuteAsync(IPolicyContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await SelectInventoryAsync(context, cancellationToken);
                if (result == null || !result.Items.Any())
                    return PolicyResult.Skipped("无可用库存");

                // 计算是否足额满足（#4）：调用方可据此区分"完全满足"与"部分满足"
                result.RequiredQty = context.GetData<decimal>(StrategyParams.InventoryAllocationInput.REQUIRED_QTY);
                result.IsFullySatisfied = result.RequiredQty <= 0 || result.TotalAllocatedQty >= result.RequiredQty;
                result.ShortageQty = result.IsFullySatisfied ? 0 : Math.Max(0, result.RequiredQty - result.TotalAllocatedQty);

                context.SetData(StrategyParams.InventoryAllocationOutput.RESULT, result);
                return PolicyResult.Success(result);
            }
            catch (Exception ex)
            {
                return PolicyResult.Failure($"库存分配失败: {ex.Message}");
            }
        }

        // =====================================================================
        //  出库偏好（原 OutboundAllocation 层下沉而来，A2）
        //  通过 StepParams/StrategyParams JSON 配置，对全仓候选库存做稳定排序后再按 FIFO/FEFO 分配。
        //  不配置时即纯跨仓 FIFO/FEFO（正确默认）。
        // =====================================================================

        /// <summary>读取步骤参数 JSON（StepParams 优先，回退 StrategyParams；键与 LocationAllocation 一致，由 PolicyChain 注入）</summary>
        protected string? GetParamsJson(IPolicyContext context)
        {
            return context.GetData<string>(StrategyParams.LocationAllocationInput.STEP_PARAMS)
                ?? context.GetData<string>(StrategyParams.LocationAllocationInput.STRATEGY_PARAMS);
        }

        /// <summary>
        /// 对候选库存应用出库偏好排序（稳定排序，不改变同组内的 FIFO/FEFO 顺序）：
        /// ① 分区优先级 ZonePriorityList —— 配置的库区按其 Priority 升序排前，未配置库区排后；
        /// ② 整托优先 EnableWholePalletFirst —— 能一次性满足整单需求量的库位排前。
        /// </summary>
        protected List<InventoryInfoDTO> ApplyPreferences(List<InventoryInfoDTO> candidates, decimal requiredQty, IPolicyContext context)
        {
            if (candidates == null || candidates.Count <= 1)
                return candidates ?? new List<InventoryInfoDTO>();

            var zoneMap = ParseZonePriorityMap(context);
            var wholePallet = ParseBoolField(context, "EnableWholePalletFirst");
            if (zoneMap == null && !wholePallet)
                return candidates;

            var unlistedPriority = (zoneMap != null && zoneMap.Values.Any() ? zoneMap.Values.Max() : 0) + 1;
            return candidates
                .OrderBy(i =>
                {
                    var code = i.ZoneCode;
                    return !string.IsNullOrEmpty(code) && zoneMap != null && zoneMap.ContainsKey(code)
                        ? zoneMap[code] : unlistedPriority;
                })
                .ThenBy(i => wholePallet ? ((i.Qty - i.LockedQty) >= requiredQty ? 0 : 1) : 0)
                .ToList();
        }

        private Dictionary<string, int>? ParseZonePriorityMap(IPolicyContext context)
        {
            var json = GetParamsJson(context);
            if (string.IsNullOrWhiteSpace(json)) return null;
            try
            {
                using var doc = JsonDocument.Parse(json);
                if (!doc.RootElement.TryGetProperty("ZonePriorityList", out var arr) || arr.ValueKind != JsonValueKind.Array)
                    return null;
                var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                foreach (var item in arr.EnumerateArray())
                {
                    if (!item.TryGetProperty("ZoneCode", out var zc) || zc.ValueKind != JsonValueKind.String) continue;
                    var code = zc.GetString();
                    if (string.IsNullOrWhiteSpace(code)) continue;
                    var pri = item.TryGetProperty("Priority", out var p) && p.ValueKind == JsonValueKind.Number
                        ? p.GetInt32() : int.MaxValue;
                    map[code] = pri;
                }
                return map.Count == 0 ? null : map;
            }
            catch { return null; }
        }

        private bool ParseBoolField(IPolicyContext context, string field)
        {
            var json = GetParamsJson(context);
            if (string.IsNullOrWhiteSpace(json)) return false;
            try
            {
                using var doc = JsonDocument.Parse(json);
                return doc.RootElement.TryGetProperty(field, out var v) && v.ValueKind == JsonValueKind.True;
            }
            catch { return false; }
        }
    }

    /// <summary>
    /// 库存分配结果
    /// </summary>
    public class InventoryAllocationResult
    {
        /// <summary>分配明细列表（按优先级排序）</summary>
        public List<InventoryAllocationItem> Items { get; set; } = new();

        /// <summary>总分配数量</summary>
        public decimal TotalAllocatedQty => Items.Sum(x => x.AllocatedQty);

        /// <summary>需求总量（由基类从上下文 REQUIRED_QTY 填充）</summary>
        public decimal RequiredQty { get; set; }

        /// <summary>是否足额满足（TotalAllocatedQty &gt;= RequiredQty）</summary>
        public bool IsFullySatisfied { get; set; }

        /// <summary>缺口数量（不足时为正，足额时为 0）</summary>
        public decimal ShortageQty { get; set; }
    }

    /// <summary>
    /// 库存分配明细项
    /// </summary>
    public class InventoryAllocationItem
    {
        /// <summary>库存明细ID</summary>
        public long InventoryDetailId { get; set; }

        /// <summary>库存头ID</summary>
        public long InventoryHeaderId { get; set; }

        /// <summary>货位ID</summary>
        public string LocationId { get; set; } = string.Empty;

        /// <summary>货位编码</summary>
        public string LocationCode { get; set; } = string.Empty;

        /// <summary>容器编号</summary>
        public string? ContainerCode { get; set; }

        /// <summary>物料ID</summary>
        public long MaterialId { get; set; }

        /// <summary>批次号</summary>
        public string? BatchNo { get; set; }

        /// <summary>序列号</summary>
        public string? SerialNo { get; set; }

        /// <summary>可用数量</summary>
        public decimal AvailableQty { get; set; }

        /// <summary>分配数量</summary>
        public decimal AllocatedQty { get; set; }

        /// <summary>生产日期</summary>
        public DateOnly? ManufactureDate { get; set; }

        /// <summary>过期日期</summary>
        public DateOnly? ExpiryDate { get; set; }

        /// <summary>入库时间</summary>
        public DateTime? InboundTime { get; set; }

        /// <summary>优先级（数字越小越优先）</summary>
        public int Priority { get; set; }

        /// <summary>分配理由</summary>
        public string Reason { get; set; } = string.Empty;
    }
}
