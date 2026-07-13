using KH.WMS.Contracts.BaseData;
using KH.WMS.Config.Abstractions;
using KH.WMS.Core.Database.Repositories;
using KH.WMS.Core.Database.UnitOfWorks;
using KH.WMS.Core.DependencyInjection.ServiceLifetimes;
using KH.WMS.Core.Logging.WMSError;
using KH.WMS.Core.Models;
using static KH.WMS.Core.Logging.WMSError.WMSErrorCodes;
using KH.WMS.Core.Models.Dtos;
using KH.WMS.Core.Services;
using KH.WMS.Config.Abstractions;
using KH.WMS.Entities.Constants;
using KH.WMS.Entities.Outbound;
using KH.WMS.Modules.OutboundModule.DTOs;
using KH.WMS.Modules.OutboundModule.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using SqlSugar;
using System.Text.Json;

namespace KH.WMS.Modules.OutboundModule.Services
{
    [RegisteredService(Lifetime = ServiceLifetime.Scoped, ServiceType = typeof(IOutboundOrderService))]
    public class OutboundOrderService(
        IRepository<OutboundOrder, long> repository,
        IRepository<OutboundOrderLine, long> lineRepository,
        ICodeGeneratorService codeGenerator,
        IMaterialContract materialContract,
        IDocumentStatusValidatorContract documentStatusValidator,
        IUnitOfWork unitOfWork,
        IDetailSaveService detailSaveService,
        ICfgDocumentFieldExtContract extFieldService)
        : CrudService<OutboundOrder>(repository, unitOfWork, detailSaveService), IOutboundOrderService
    {
        private readonly IRepository<OutboundOrderLine, long> _lineRepository = lineRepository;
        private readonly ICodeGeneratorService _codeGenerator = codeGenerator;
        private readonly IMaterialContract _materialContract = materialContract;
        private readonly IDocumentStatusValidatorContract _statusValidator = documentStatusValidator;
        private readonly ICfgDocumentFieldExtContract _extFieldService = extFieldService;

        /// <inheritdoc />
        public async Task<long> CreateWithLinesAsync(OutboundOrderCreateDto dto)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var order = dto.Order;
                order.OrderNo = await _codeGenerator.GenerateAsync(BizConstants.CodeRuleTypes.OUTBOUND_DOC);
                order.TotalLines = dto.Lines.Count;

                // 从配置表获取初始状态
                order.OrderStatus = await _statusValidator.GetInitialStatusAsync(BizConstants.DocTypes.OUTBOUND_ORDER)
                    ?? throw new Exception("未找到出库单的初始状态配置");

                // 补全物料信息
                await FillMaterialInfoAsync(dto.Lines);

                for (int i = 0; i < dto.Lines.Count; i++)
                {
                    dto.Lines[i].LineNo = i + 1;
                }

                // 通过导航属性一次性插入主表 + 从表
                order.Items = dto.Lines;
                await _repository.AddWithNavAsync(order);

                await _unitOfWork.CommitAsync();
                return order.Id;
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<bool> UpdateWithLinesAsync(long id, OutboundOrderCreateDto dto)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var existing = await _repository.GetByIdAsync(id);
                if (existing == null)
                    throw new Exception("出库单不存在");

                await _statusValidator.ValidateAllowEditAsync(BizConstants.DocTypes.OUTBOUND_ORDER, existing.OrderStatus);

                var order = dto.Order;
                order.Id = id;
                order.OrderNo = existing.OrderNo;
                order.TotalLines = dto.Lines.Count;

                await _repository.UpdateAsync(order);

                // 删除旧明细，重新插入（全量替换）
                await _lineRepository.DeleteAsync(l => l.OrderId == id);

                // 补全物料信息
                await FillMaterialInfoAsync(dto.Lines);

                for (int i = 0; i < dto.Lines.Count; i++)
                {
                    dto.Lines[i].OrderId = id;
                    dto.Lines[i].LineNo = i + 1;
                }

                if (dto.Lines.Count > 0)
                    await _lineRepository.AddAsync(dto.Lines);

                await _unitOfWork.CommitAsync();
                return true;
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<OutboundOrderCreateDto?> GetDetailAsync(long id)
        {
            var order = await _repository.GetByIdAsync(id);
            if (order == null) return null;

            var lines = await _lineRepository.GetListAsync(l => l.OrderId == id)
                ?? new List<OutboundOrderLine>();

            var result = new OutboundOrderCreateDto { Order = order, Lines = lines };

            // 反序列化单据头扩展字段
            result.ExtDataFlattened = _extFieldService.DeserializeExtData(order.ExtData);

            // 反序列化行级扩展字段
            result.LineExtDataFlattened = new Dictionary<long, Dictionary<string, object?>>();
            foreach (var line in lines)
            {
                if (!string.IsNullOrEmpty(line.ExtData))
                {
                    result.LineExtDataFlattened[line.Id] = _extFieldService.DeserializeExtData(line.ExtData);
                }
            }

            return result;
        }

        /// <inheritdoc />
        protected override async Task BeforeDeleteAsync(long id, OutboundOrder entity)
        {
            await _statusValidator.ValidateAllowDeleteAsync(BizConstants.DocTypes.OUTBOUND_ORDER, entity.OrderStatus);

            // 先删除明细行
            await _lineRepository.DeleteAsync(l => l.OrderId == id);
        }

        /// <inheritdoc />
        public async Task<ServiceResult> ConfirmAsync(long id)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // 行锁序列化并发确认，防止重复确认覆盖
                var order = await _unitOfWork.DbContext.Db.Queryable<OutboundOrder>()
                    .Where(o => o.Id == id)
                    .With(SqlWith.UpdLock)
                    .FirstAsync();
                if (order == null)
                {
                    await _unitOfWork.RollbackAsync();
                    return ServiceResult.Fail(WMSErrorMessages.GetMessage(OUTBOUND_NOT_EXIST));
                }

                // 幂等：已确认则直接成功返回
                if (order.OrderStatus == BizConstants.OutboundOrderStatus.CONFIRMED)
                {
                    await _unitOfWork.RollbackAsync();
                    return ServiceResult.Ok(WMSErrorMessages.GetMessage(OUTBOUND_CONFIRM_SUCCESS));
                }

                var lines = await _lineRepository.GetListAsync(l => l.OrderId == id);
                if (lines.Count == 0)
                {
                    await _unitOfWork.RollbackAsync();
                    return ServiceResult.Fail(WMSErrorMessages.GetMessage(OUTBOUND_NO_DETAIL_FOR_CONFIRM));
                }

                // 从配置表获取确认后可流转到的状态
                var allowedTransitions = await _statusValidator.GetAllowedTransitionsAsync(
                    BizConstants.DocTypes.OUTBOUND_ORDER, order.OrderStatus);

                // 取 CONFIRMED 或第一个非取消状态
                var confirmedStatus = allowedTransitions.FirstOrDefault(s => s == BizConstants.OutboundOrderStatus.CONFIRMED)
                    ?? allowedTransitions.FirstOrDefault(s => s != BizConstants.OutboundOrderStatus.CANCELLED)
                    ?? BizConstants.OutboundOrderStatus.CONFIRMED;

                try
                {
                    await _statusValidator.ValidateTransitionAsync(
                        BizConstants.DocTypes.OUTBOUND_ORDER,
                        order.OrderStatus,
                        confirmedStatus);
                }
                catch (InvalidOperationException ex)
                {
                    await _unitOfWork.RollbackAsync();
                    return ServiceResult.Fail(ex.Message);
                }

                order.OrderStatus = confirmedStatus;
                await _repository.UpdateAsync(order);

                await _unitOfWork.CommitAsync();
                return ServiceResult.Ok(WMSErrorMessages.GetMessage(OUTBOUND_CONFIRM_SUCCESS));
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        // ==================== 状态配置驱动 ====================

        private const string DocTypeCode = BizConstants.DocTypes.OUTBOUND_ORDER;

        /// <summary>
        /// 通过契约批量查询物料信息，补全明细行的 MaterialId、MaterialName、UnitId
        /// </summary>
        private async Task FillMaterialInfoAsync(List<OutboundOrderLine> lines)
        {
            var materialCodes = lines.Select(l => l.MaterialCode).Where(c => !string.IsNullOrEmpty(c)).Distinct().ToList()!;
            var materialDict = materialCodes.Count > 0
                ? (await _materialContract.GetByCodesAsync(materialCodes!)).ToDictionary(m => m.MaterialCode)
                : new Dictionary<string, MaterialInfo>();

            foreach (var line in lines)
            {
                line.Id = 0;
                if (materialDict.TryGetValue(line.MaterialCode ?? string.Empty, out var material))
                {
                    line.MaterialId = material.Id;
                    if (string.IsNullOrEmpty(line.MaterialName))
                        line.MaterialName = material.MaterialName;
                    if (line.UnitId == null)
                        line.UnitId = material.BaseUnitId;
                }
            }
        }

        /// <summary>
        /// 分页查询后处理：为每行计算 AllowedActions
        /// </summary>
        protected override async Task<List<OutboundOrder>> AfterQueryAsync(AdvancedQueryRequestDto query, List<OutboundOrder> items)
        {
            items = await base.AfterQueryAsync(query, items);
            await ComputeAllowedActionsAsync(items);
            return items;
        }

        /// <summary>
        /// 根据状态配置为每条订单计算允许的操作列表
        /// </summary>
        private async Task ComputeAllowedActionsAsync(List<OutboundOrder> orders)
        {
            foreach (var order in orders)
            {
                var config = await _statusValidator.GetStatusConfigAsync(DocTypeCode, order.OrderStatus);
                if (config == null) continue;

                var actions = new List<string>();

                // 编辑/删除：直接取配置
                if (config.AllowEdit == BizConstants.BoolFlag.YES) actions.Add(BizConstants.DocActions.EDIT);
                if (config.AllowDelete == BizConstants.BoolFlag.YES) actions.Add(BizConstants.DocActions.DELETE);

                // 从 NextStatuses 解析可流转目标
                var nextList = ParseNextStatuses(config.NextStatuses);

                // 确认：NextStatuses 包含 CONFIRMED
                if (nextList.Contains(BizConstants.OutboundOrderStatus.CONFIRMED))
                    actions.Add(BizConstants.DocActions.CONFIRM);

                // 分配：NextStatuses 包含 RELEASING，或已在可分配状态
                if (nextList.Contains(BizConstants.OutboundOrderStatus.RELEASING)
                    || order.OrderStatus == BizConstants.OutboundOrderStatus.CONFIRMED)
                    actions.Add(BizConstants.DocActions.ALLOCATE);

                order.AllowedActions = actions;
            }
        }

        private static List<string> ParseNextStatuses(string? json)
        {
            if (string.IsNullOrWhiteSpace(json)) return new List<string>();
            try { return JsonSerializer.Deserialize<List<string>>(json) ?? new(); }
            catch { return new List<string>(); }
        }
    }
}
