using KH.WMS.Core.Api.Responses;
using KH.WMS.Core.Database.Repositories;
using KH.WMS.Core.Database.UnitOfWorks;
using KH.WMS.Core.DependencyInjection.ServiceLifetimes;
using KH.WMS.Core.Services;
using KH.WMS.Core.UserProvide;
using KH.WMS.Entities.BaseData;
using KH.WMS.Entities.Constants;
using KH.WMS.Entities.Inventory;
using KH.WMS.Entities.Outbound;
using KH.WMS.Modules.BaseDataModule.DTOs;
using KH.WMS.Modules.BaseDataModule.Interfaces;
using SqlSugar;

namespace KH.WMS.Modules.BaseDataModule.Services
{
    [RegisteredService(ServiceType = typeof(IMaterialTurnoverService))]
    public class MaterialTurnoverService(
        IRepository<MdMaterialTurnover, long> repository,
        ISqlSugarClient db,
        IUnitOfWork unitOfWork,
        IDetailSaveService detailSaveService,
        IRepository<CfgTurnoverClass, long> turnoverClassRepository) : CrudService<MdMaterialTurnover>(repository, unitOfWork, detailSaveService), IMaterialTurnoverService
    {
        private readonly ISqlSugarClient _db = db;
        private readonly IRepository<CfgTurnoverClass, long> _turnoverClassRepository = turnoverClassRepository;
        /// <summary>
        /// 执行ABC分类计算
        /// </summary>
        public async Task<ApiResponse> CalculateAsync(TurnoverCalculateRequest request)
        {
            // 1. 获取ABC分类配置
            var classConfigs = (await _turnoverClassRepository.GetListAsync(c => c.Status == BizConstants.BoolFlag.YES))
                .OrderBy(c => c.CumulativeRatioMin)
                .ToList();

            if (classConfigs.Count == 0)
                return ApiResponse.Fail(ResponseCode.VALIDATION_ERROR, "未配置ABC分类规则，请先在周转分类配置中添加分类规则");

            // 2. 汇总出库数据（按物料维度，基于库存流水）
            var startDate = request.StartDate.Date;
            var endDate = request.EndDate.Date.AddDays(1);

            var outboundSummary = await _db.Queryable<InvMovement>()
                .InnerJoin<MdMaterial>((m, mat) => m.MaterialId == mat.Id)
                .Where((m, mat) => m.MovementType == BizConstants.MovementTypes.OUTBOUND
                    && m.Direction == BizConstants.MovementDirections.DECREASE
                    && m.MovementTime >= startDate
                    && m.MovementTime < endDate)
                .GroupBy((m, mat) => new { m.MaterialId, mat.MaterialCode, mat.MaterialName })
                .Select((m, mat) => new
                {
                    MaterialId = m.MaterialId,
                    MaterialCode = mat.MaterialCode,
                    MaterialName = mat.MaterialName,
                    OutboundCount = SqlFunc.AggregateCount(m.Id),
                    OutboundQty = SqlFunc.AggregateSum(m.MovementQty),
                })
                .ToListAsync();

            if (outboundSummary.Count == 0)
                return ApiResponse.Fail(ResponseCode.VALIDATION_ERROR, "所选时间范围内无出库数据");

            // 3. 根据分析维度计算排序值
            var materialStats = outboundSummary.Select(x => new MaterialStat
            {
                MaterialId = x.MaterialId,
                MaterialCode = x.MaterialCode ?? string.Empty,
                MaterialName = x.MaterialName ?? string.Empty,
                OutboundCount = x.OutboundCount,
                OutboundQty = x.OutboundQty,
                SortValue = request.AnalysisDimension switch
                {
                    "OUTBOUND_QTY" => x.OutboundQty,
                    "OUTBOUND_FREQ" => x.OutboundCount,
                    _ => x.OutboundQty
                }
            }).ToList();

            // 4. 按分析维度降序排序
            materialStats.Sort((a, b) => b.SortValue.CompareTo(a.SortValue));

            // 5. 计算累计占比并划分ABC分类
            var totalValue = materialStats.Sum(x => x.SortValue);
            var cumulative = 0m;
            var results = new List<MdMaterialTurnover>();

            foreach (var stat in materialStats)
            {
                var ratio = totalValue > 0 ? stat.SortValue / totalValue * 100 : 0;
                cumulative += ratio;

                // 根据累计占比匹配分类
                var classCode = MatchClass(classConfigs, cumulative);

                results.Add(new MdMaterialTurnover
                {
                    MaterialId = stat.MaterialId,
                    MaterialCode = stat.MaterialCode,
                    MaterialName = stat.MaterialName,
                    ClassCode = classCode,
                    Period = request.Period,
                    OutboundCount = stat.OutboundCount,
                    OutboundQty = stat.OutboundQty,
                    CumulativeRatio = Math.Round(cumulative, 2),
                    CalculatedAt = DateTime.Now
                });
            }

            // 6. 删除该周期的旧数据，写入新结果
            await _db.Deleteable<MdMaterialTurnover>()
                .Where(x => x.Period == request.Period)
                .ExecuteCommandAsync();

            await _db.Insertable(results).ExecuteCommandAsync();

            return ApiResponse.Ok("ABC分类计算完成");
        }

        /// <summary>
        /// 查询指定周期的分类结果
        /// </summary>
        public async Task<List<MaterialTurnoverDto>> GetResultsAsync(string period)
        {
            var classConfigs = await _turnoverClassRepository.GetAllAsync();
            var classDict = classConfigs.ToDictionary(c => c.ClassCode, c => c.ClassName);

            var results = await _db.Queryable<MdMaterialTurnover>()
                .Where(t => t.Period == period)
                .Select(t => new MaterialTurnoverDto
                {
                    Id = t.Id,
                    MaterialId = t.MaterialId,
                    MaterialCode = t.MaterialCode,
                    MaterialName = t.MaterialName,
                    ClassCode = t.ClassCode,
                    ClassName = SqlFunc.IsNull(classDict.ContainsKey(t.ClassCode) ? classDict[t.ClassCode] : null, t.ClassCode),
                    Period = t.Period,
                    OutboundCount = t.OutboundCount,
                    OutboundQty = t.OutboundQty,
                    CumulativeRatio = t.CumulativeRatio,
                    CalculatedAt = t.CalculatedAt
                })
                .OrderBy(t => t.CumulativeRatio)
                .ToListAsync();

            return results;
        }

        /// <summary>
        /// 根据累计占比匹配ABC分类
        /// </summary>
        private static string MatchClass(List<CfgTurnoverClass> configs, decimal cumulativeRatio)
        {
            foreach (var config in configs)
            {
                if (cumulativeRatio >= config.CumulativeRatioMin && cumulativeRatio < config.CumulativeRatioMax)
                    return config.ClassCode;
            }
            // 兜底：返回最后一个分类（通常 cumulativeRatio == 100 时上限可能等于100取不到）
            return configs[^1].ClassCode;
        }

        /// <summary>
        /// 物料出库统计中间结构
        /// </summary>
        private class MaterialStat
        {
            public long MaterialId { get; set; }
            public string MaterialCode { get; set; } = string.Empty;
            public string MaterialName { get; set; } = string.Empty;
            public int OutboundCount { get; set; }
            public decimal OutboundQty { get; set; }
            public decimal SortValue { get; set; }
        }
    }
}
