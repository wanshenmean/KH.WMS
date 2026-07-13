using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KH.WMS.Algorithms.Strategy.Interfaces
{
    /// <summary>
    /// 策略上下文接口
    /// </summary>
    public interface IPolicyContext
    {
        /// <summary>
        /// 输入参数
        /// </summary>
        object? InputParameters { get; set; }

        /// <summary>
        /// 上下文数据字典
        /// </summary>
        IDictionary<string, object?> ContextData { get; }

        /// <summary>
        /// 业务单据号
        /// </summary>
        string? BusinessCode { get; set; }

        /// <summary>
        /// 仓库ID
        /// </summary>
        long? WarehouseId { get; set; }

        /// <summary>
        /// 仓库编码
        /// </summary>
        string? WarehouseCode { get; set; }

        /// <summary>
        /// 库区ID
        /// </summary>
        long? ZoneId { get; set; }

        /// <summary>
        /// 物料ID
        /// </summary>
        long? MaterialId { get; set; }

        /// <summary>
        /// 物料分类ID
        /// </summary>
        long? MaterialCategoryId { get; set; }

        /// <summary>
        /// 单据类型（如 PURCHASE_INBOUND / SALES_OUTBOUND 等）
        /// </summary>
        string? DocType { get; set; }

        /// <summary>
        /// 订单类型（如 PURCHASE / SALES / TRANSFER 等，用于策略链匹配）
        /// 与 DocType 的区别：DocType 是具体单据方向（入库/出库），OrderType 是业务类型
        /// </summary>
        string? OrderType { get; set; }

        /// <summary>
        /// 获取上下文数据
        /// </summary>
        T? GetData<T>(string key);

        /// <summary>
        /// 设置上下文数据
        /// </summary>
        void SetData<T>(string key, T? value);
    }
}
