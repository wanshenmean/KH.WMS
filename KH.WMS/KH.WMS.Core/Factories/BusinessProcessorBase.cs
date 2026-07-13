using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace KH.WMS.Core.Factories
{
    /// <summary>
    /// 业务处理器基类
    /// </summary>
    public abstract class BusinessProcessorBase : IBusinessProcessor
    {
        public abstract string ProcessorType { get; }

        public abstract Task<(bool Success, object? Data, string? ErrorMessage)> ProcessAsync(
            JToken jsonData,
            IServiceProvider serviceProvider);

        /// <summary>
        /// 安全获取字符串属性
        /// </summary>
        protected string GetStringProperty(JToken element, string propertyName, string defaultValue = "")
        {
            var prop = element[propertyName];
            return prop != null && prop.Type == JTokenType.String
                ? prop.Value<string>() ?? defaultValue
                : prop?.ToString() ?? defaultValue;
        }

        /// <summary>
        /// 安全获取数字属性
        /// </summary>
        protected decimal GetDecimalProperty(JToken element, string propertyName, decimal defaultValue = 0)
        {
            var prop = element[propertyName];
            return prop != null && prop.Type == JTokenType.Float || prop.Type == JTokenType.Integer
                ? prop.Value<decimal>()
                : defaultValue;
        }

        /// <summary>
        /// 安全获取日期属性
        /// </summary>
        protected DateTime? GetDateTimeProperty(JToken element, string propertyName)
        {
            var prop = element[propertyName];
            if (prop != null)
            {
                if (prop.Type == JTokenType.String && DateTime.TryParse(prop.Value<string>(), out var date))
                    return date;
                if (prop.Type == JTokenType.Null)
                    return null;
            }
            return null;
        }
    }
}
