using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KH.WMS.Algorithms.Strategy.Enums;

namespace KH.WMS.Algorithms.Strategy.Interfaces
{
    /// <summary>
    /// 策略接口
    /// </summary>
    public interface IPolicyStrategy
    {
        /// <summary>
        /// 策略名称
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 策略编码
        /// </summary>
        string Code { get; }

        /// <summary>
        /// 策略优先级（数字越大优先级越高）
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// 作者
        /// </summary>
        string Author { get; }

        /// <summary>
        /// 描述
        /// </summary>
        string Description { get; }

        /// <summary>
        /// 策略类型
        /// </summary>
        PolicyType PolicyType { get; }

        /// <summary>
        /// 是否启用
        /// </summary>
        bool IsEnabled { get; }

        /// <summary>
        /// 适用仓库ID列表（空表示适用所有仓库）
        /// </summary>
        IEnumerable<long>? ApplicableWarehouseIds { get; }

        /// <summary>
        /// 适用库区ID列表（空表示适用所有库区）
        /// </summary>
        IEnumerable<long>? ApplicableZoneIds { get; }

        /// <summary>
        /// 适用物料ID列表（空表示适用所有物料）
        /// </summary>
        IEnumerable<long>? ApplicableMaterialIds { get; }

        /// <summary>
        /// 适用单据类型列表（空表示适用所有单据类型）
        /// </summary>
        IEnumerable<string>? ApplicableDocTypes { get; }

        /// <summary>
        /// 判断策略是否适用
        /// </summary>
        Task<bool> IsApplicableAsync(IPolicyContext context, CancellationToken cancellationToken = default);

        /// <summary>
        /// 执行策略
        /// </summary>
        Task<IPolicyResult> ExecuteAsync(IPolicyContext context, CancellationToken cancellationToken = default);
    }
}
