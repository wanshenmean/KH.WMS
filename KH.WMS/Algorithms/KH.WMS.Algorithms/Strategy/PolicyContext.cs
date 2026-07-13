using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KH.WMS.Algorithms.Strategy.Interfaces;

namespace KH.WMS.Algorithms.Strategy
{
    /// <summary>
    /// 策略上下文实现
    /// </summary>
    public class PolicyContext : IPolicyContext
    {
        public object? InputParameters { get; set; }
        public IDictionary<string, object?> ContextData { get; } = new Dictionary<string, object?>();
        public string? BusinessCode { get; set; }
        public long? WarehouseId { get; set; }
        public string? WarehouseCode { get; set; }
        public long? ZoneId { get; set; }
        public long? MaterialId { get; set; }
        public long? MaterialCategoryId { get; set; }
        public string? DocType { get; set; }
        public string? OrderType { get; set; }

        public T? GetData<T>(string key)
        {
            // #41：键缺失或值为 null 直接返回 default，避免对值类型拆箱 null 抛 NRE
            if (!ContextData.TryGetValue(key, out var value) || value == null)
                return default;

            // 类型匹配直接返回
            if (value is T matched)
                return matched;

            // 类型不匹配（如存 int 取 long）尝试转换，避免拆箱 InvalidCastException
            try
            {
                var targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
                return (T?)Convert.ChangeType(value, targetType);
            }
            catch
            {
                return default;
            }
        }

        public void SetData<T>(string key, T? value)
        {
            ContextData[key] = value;
        }
    }
}
