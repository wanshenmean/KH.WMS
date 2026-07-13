using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace KH.WMS.Core.Helpers
{
    public static class UtilConvert
    {
        private static DateTime dateStart = new DateTime(1970, 1, 1, 8, 0, 0);

        private static long longTime = 621355968000000000;

        private static int samllTime = 10000000;

        /// <summary>
        /// 将时间戳转换为本地日期时间
        /// </summary>
        /// <param name="timeStamp">要转换的时间戳对象</param>
        /// <returns>转换后的本地DateTime对象</returns>
        /// <remarks>如果参数为null，则返回默认起始日期</remarks>
        public static DateTime GetTimeSpmpToDate(this object timeStamp)
        {
            if (timeStamp == null) return dateStart;
            DateTime dateTime = new DateTime(longTime + Convert.ToInt64(timeStamp) * samllTime, DateTimeKind.Utc).ToLocalTime();
            return dateTime;
        }

        /// <summary>
        /// 将对象序列化为JSON字符串
        /// </summary>
        /// <param name="obj">要序列化的对象</param>
        /// <param name="formatDate">日期格式化设置，默认为"yyyy-MM-dd HH:mm:ss"格式</param>
        /// <returns>序列化后的JSON字符串，如果对象为null则返回null</returns>
        public static string Serialize(this object obj, JsonSerializerSettings formatDate = null)
        {
            if (obj == null) return null;
            formatDate = formatDate ?? new JsonSerializerSettings
            {
                DateFormatString = "yyyy-MM-dd HH:mm:ss"
            };
            return JsonConvert.SerializeObject(obj, formatDate);
        }

        /// <summary>
        /// 将JSON字符串反序列化为指定类型的对象
        /// </summary>
        /// <typeparam name="T">目标对象类型</typeparam>
        /// <param name="json">要反序列化的JSON字符串</param>
        /// <returns>反序列化后的对象实例，如果输入为空则返回默认值</returns>
        /// <remarks>
        /// 当输入为"{}"时会自动转换为"[]"处理
        /// </remarks>
        public static T DeserializeObject<T>(this string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return default(T);
            }
            if (json == "{}")
            {
                json = "[]";
            }
            return JsonConvert.DeserializeObject<T>(json);
        }

        /// <summary>
        /// 将字符串的首字母转换为小写
        /// </summary>
        /// <param name="thisValue">要处理的字符串</param>
        /// <returns>首字母小写的字符串，若输入为空则返回空字符串</returns>
        public static string FirstLetterToLower(this string thisValue)
        {
            if (string.IsNullOrEmpty(thisValue)) return string.Empty;
            string result = thisValue.Substring(0, 1).ToLower() + thisValue.Substring(1, thisValue.Length - 1);
            return result;
        }

        /// <summary>
        /// 将字符串的首字母转换为大写
        /// </summary>
        /// <param name="thisValue">要处理的字符串</param>
        /// <returns>首字母大写的字符串，若输入为空则返回空字符串</returns>
        public static string FirstLetterToUpper(this string thisValue)
        {
            if (string.IsNullOrEmpty(thisValue)) return string.Empty;
            string result = thisValue.Substring(0, 1).ToUpper() + thisValue.Substring(1, thisValue.Length - 1);
            return result;
        }

        /// <summary>
        /// 将对象转换为整型值
        /// </summary>
        /// <param name="thisValue">要转换的对象</param>
        /// <returns>转换后的整型值，转换失败返回0</returns>
        /// <remarks>
        /// 支持处理null值、DBNull.Value和枚举类型
        /// </remarks>
        public static int ObjToInt(this object thisValue)
        {
            int reval = 0;
            if (thisValue == null) return 0;
            if (thisValue is Enum && thisValue != DBNull.Value && Enum.TryParse(thisValue.GetType(), thisValue.ToString(), out var val))
            {
                return Convert.ToInt32(val.ChangeType(typeof(int)));
            }
            if (thisValue != DBNull.Value && int.TryParse(thisValue.ToString(), out reval))
            {
                return reval;
            }

            return reval;
        }

        /// <summary>
        /// 将双精度浮点数转换为32位有符号整数
        /// </summary>
        /// <param name="thisValue">要转换的双精度浮点数</param>
        /// <returns>转换后的32位有符号整数</returns>
        public static int DoubleToInt(this double thisValue)
        {
            int reval = 0;

            return Convert.ToInt32(thisValue);
        }

        /// <summary>
        /// 将对象转换为整数，若转换失败则返回指定的错误值
        /// </summary>
        /// <param name="thisValue">要转换的对象</param>
        /// <param name="errorValue">转换失败时返回的错误值</param>
        /// <returns>转换后的整数值或指定的错误值</returns>
        public static int ObjToInt(this object thisValue, int errorValue)
        {
            int reval = 0;
            if (thisValue != null && thisValue != DBNull.Value && int.TryParse(thisValue.ToString(), out reval))
            {
                return reval;
            }

            return errorValue;
        }

        /// <summary>
        /// 将对象转换为长整型(long)
        /// </summary>
        /// <param name="thisValue">要转换的对象</param>
        /// <returns>转换后的长整型值，转换失败或为null时返回0</returns>
        /// <remarks>
        /// 处理DBNull.Value情况，并尝试将对象转换为long类型
        /// </remarks>
        public static long ObjToLong(this object thisValue)
        {
            long reval = 0;
            if (thisValue == null) return 0;
            if (thisValue != DBNull.Value && long.TryParse(thisValue.ToString(), out reval))
            {
                return reval;
            }

            return reval;
        }

        /// <summary>
        /// 将对象转换为金额(double类型)。若转换失败则返回0。
        /// </summary>
        /// <param name="thisValue">待转换的对象</param>
        /// <returns>转换后的金额值，失败返回0</returns>
        public static double ObjToMoney(this object thisValue)
        {
            double reval = 0;
            if (thisValue != null && thisValue != DBNull.Value && double.TryParse(thisValue.ToString(), out reval))
            {
                return reval;
            }

            return 0;
        }

        /// <summary>
        /// 将对象转换为金额(double)类型，转换失败时返回指定的错误值
        /// </summary>
        /// <param name="thisValue">要转换的对象</param>
        /// <param name="errorValue">转换失败时返回的默认值</param>
        /// <returns>转换成功的double值或指定的错误值</returns>
        public static double ObjToMoney(this object thisValue, double errorValue)
        {
            double reval = 0;
            if (thisValue != null && thisValue != DBNull.Value && double.TryParse(thisValue.ToString(), out reval))
            {
                return reval;
            }

            return errorValue;
        }

        /// <summary>
        /// 将对象转换为字符串，若对象为null则返回空字符串
        /// </summary>
        /// <param name="thisValue">要转换的对象</param>
        /// <returns>去除前后空格的字符串结果</returns>
        public static string ObjToString(this object thisValue)
        {
            if (thisValue != null) return thisValue.ToString().Trim();
            return "";
        }

        /// <summary>
        /// 判断对象是否不为空或null值
        /// </summary>
        /// <param name="thisValue">要判断的对象</param>
        /// <returns>当对象不为空且不等于"undefined"或"null"字符串时返回true，否则返回false</returns>
        public static bool IsNotEmptyOrNull(this object thisValue)
        {
            return ObjToString(thisValue) != "" && ObjToString(thisValue) != "undefined" && ObjToString(thisValue) != "null";
        }

        /// <summary>
        /// 将对象转换为字符串，如果对象为null则返回指定的错误值
        /// </summary>
        /// <param name="thisValue">要转换的对象</param>
        /// <param name="errorValue">当对象为null时返回的值</param>
        /// <returns>对象的字符串表示或指定的错误值</returns>
        public static string ObjToString(this object thisValue, string errorValue)
        {
            if (thisValue != null) return thisValue.ToString().Trim();
            return errorValue;
        }

        /// <summary>
        /// 判断对象是否为null、DBNull或空字符串
        /// </summary>
        /// <param name="thisValue">要检查的对象</param>
        /// <returns>如果对象为null、DBNull或空字符串则返回true，否则返回false</returns>
        public static bool IsNullOrEmpty(this object thisValue) => thisValue == null || thisValue == DBNull.Value || string.IsNullOrWhiteSpace(thisValue.ToString());

        /// <summary>
        /// 将对象转换为Decimal类型
        /// </summary>
        /// <param name="thisValue">要转换的对象</param>
        /// <returns>转换后的Decimal值，转换失败返回0</returns>
        public static Decimal ObjToDecimal(this object thisValue)
        {
            Decimal reval = 0;
            if (thisValue != null && thisValue != DBNull.Value && decimal.TryParse(thisValue.ToString(), out reval))
            {
                return reval;
            }

            return 0;
        }

        /// <summary>
        /// 将对象转换为Decimal类型，若转换失败则返回指定的默认值
        /// </summary>
        /// <param name="thisValue">要转换的对象</param>
        /// <param name="errorValue">转换失败时返回的默认值</param>
        /// <returns>转换成功的Decimal值或指定的默认值</returns>
        public static Decimal ObjToDecimal(this object thisValue, decimal errorValue)
        {
            Decimal reval = 0;
            if (thisValue != null && thisValue != DBNull.Value && decimal.TryParse(thisValue.ToString(), out reval))
            {
                return reval;
            }

            return errorValue;
        }

        /// <summary>
        /// 将对象转换为DateTime类型
        /// </summary>
        /// <param name="thisValue">要转换的对象</param>
        /// <returns>转换后的DateTime值，转换失败返回DateTime.MinValue</returns>
        public static DateTime ObjToDate(this object thisValue)
        {
            DateTime reval = DateTime.MinValue;
            if (thisValue != null && thisValue != DBNull.Value && DateTime.TryParse(thisValue.ToString(), out reval))
            {
                reval = Convert.ToDateTime(thisValue);
            }

            return reval;
        }

        /// <summary>
        /// 将对象转换为DateTime类型，转换失败时返回指定的默认值
        /// </summary>
        /// <param name="thisValue">要转换的对象</param>
        /// <param name="errorValue">转换失败时返回的默认值</param>
        /// <returns>转换成功的DateTime值或指定的默认值</returns>
        public static DateTime ObjToDate(this object thisValue, DateTime errorValue)
        {
            DateTime reval = DateTime.MinValue;
            if (thisValue != null && thisValue != DBNull.Value && DateTime.TryParse(thisValue.ToString(), out reval))
            {
                return reval;
            }

            return errorValue;
        }

        /// <summary>
        /// 将对象转换为布尔值
        /// </summary>
        /// <param name="thisValue">要转换的对象</param>
        /// <returns>转换后的布尔值，转换失败时返回false</returns>
        public static bool ObjToBool(this object thisValue)
        {
            bool reval = false;
            if (thisValue != null && thisValue != DBNull.Value && bool.TryParse(thisValue.ToString(), out reval))
            {
                return reval;
            }

            return reval;
        }

        /// <summary>
        /// 将DateTime转换为Unix时间戳（秒级）
        /// </summary>
        /// <param name="thisValue">需要转换的DateTime对象</param>
        /// <returns>返回表示Unix时间戳的字符串</returns>
        public static string DateToTimeStamp(this DateTime thisValue)
        {
            TimeSpan ts = thisValue - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds).ToString();
        }

        /// <summary>
        /// 将对象转换为指定类型
        /// </summary>
        /// <param name="value">要转换的对象</param>
        /// <param name="type">目标类型</param>
        /// <returns>转换后的对象</returns>
        /// <remarks>
        /// 支持处理null值、枚举类型、泛型类型、Guid、Version等特殊类型的转换。
        /// 如果转换失败或类型不匹配，将返回原始值或抛出异常。
        /// </remarks>
        public static object ChangeType(this object value, Type type)
        {
            if (value == null && type.IsGenericType) return Activator.CreateInstance(type);
            if (value == null) return null;
            if (type == value.GetType()) return value;
            if (type.IsEnum)
            {
                if (value is string)
                    return Enum.Parse(type, value as string);
                else
                    return Enum.ToObject(type, value);
            }

            if (!type.IsInterface && type.IsGenericType)
            {
                Type innerType = type.GetGenericArguments()[0];
                object innerValue = ChangeType(value, innerType);
                return Activator.CreateInstance(type, new object[] { innerValue });
            }

            if (value is string && type == typeof(Guid)) return new Guid(value as string);
            if (value is string && type == typeof(Version)) return new Version(value as string);
            if (!(value is IConvertible)) return value;
            return Convert.ChangeType(value, type);
        }

        /// <summary>
        /// 将对象值转换为指定类型的泛型列表。
        /// </summary>
        /// <param name="value">待转换的对象值</param>
        /// <param name="type">目标列表元素类型</param>
        /// <returns>转换后的泛型列表对象，若输入为null则返回默认值</returns>
        /// <remarks>
        /// 支持处理以括号包裹的字符串格式（如"(1,2,3)"或"("a","b")"），
        /// 自动去除分隔符并将元素转换为目标类型后添加到列表。
        /// </remarks>
        public static object ChangeTypeList(this object value, Type type)
        {
            if (value == null) return default;

            var gt = typeof(List<>).MakeGenericType(type);
            dynamic lis = Activator.CreateInstance(gt);

            var addMethod = gt.GetMethod("Add");
            string values = value.ToString();
            if (values != null && values.StartsWith("(") && values.EndsWith(")"))
            {
                string[] splits;
                if (values.Contains("\",\""))
                {
                    splits = values.Remove(values.Length - 2, 2)
                        .Remove(0, 2)
                        .Split("\",\"");
                }
                else
                {
                    splits = values.Remove(0, 1)
                        .Remove(values.Length - 2, 1)
                        .Split(",");
                }

                foreach (var split in splits)
                {
                    var str = split;
                    if (split.StartsWith("\"") && split.EndsWith("\""))
                    {
                        str = split.Remove(0, 1)
                            .Remove(split.Length - 2, 1);
                    }

                    addMethod.Invoke(lis, new object[] { ChangeType(str, type) });
                }
            }

            return lis;
        }

        /// <summary>
        /// 将对象序列化为JSON字符串
        /// </summary>
        /// <param name="value">要序列化的对象</param>
        /// <returns>对象的JSON字符串表示</returns>
        public static string ToJson(this object value)
        {
            return JsonConvert.SerializeObject(value);
        }

        /// <summary>
        /// 判断对象是否可以转换为整数
        /// </summary>
        /// <param name="obj">要判断的对象</param>
        /// <returns>如果可以转换为整数返回true，否则返回false</returns>
        public static bool IsInt(this object obj)
        {
            if (obj == null)
                return false;
            bool reslut = Int32.TryParse(obj.ToString(), out int _number);
            return reslut;

        }

        /// <summary>
        /// 判断对象是否为有效日期格式
        /// </summary>
        /// <param name="str">待检测的对象</param>
        /// <returns>true表示是有效日期，false则不是</returns>
        /// <remarks>此方法是扩展方法，通过调用IsDate(out _)实现</remarks>
        public static bool IsDate(this object str)
        {
            return str.IsDate(out _);
        }

        /// <summary>
        /// 判断对象是否可以转换为有效的日期时间
        /// </summary>
        /// <param name="str">要判断的对象</param>
        /// <param name="dateTime">输出转换后的日期时间值</param>
        /// <returns>如果转换成功返回true，否则返回false</returns>
        public static bool IsDate(this object str, out DateTime dateTime)
        {
            dateTime = DateTime.Now;
            if (str == null || str.ToString() == "")
            {
                return false;
            }
            return DateTime.TryParse(str.ToString(), out dateTime);
        }

        /// <summary>
        /// 判断字符串是否可以转换为数字
        /// </summary>
        /// <param name="str">要检查的字符串</param>
        /// <param name="formatString">格式字符串（未使用）</param>
        /// <returns>如果字符串可以转换为数字则返回true，否则返回false</returns>
        /// <remarks>
        /// 使用正则表达式验证字符串是否为数字格式，支持正负号和小数点
        /// </remarks>
        public static bool IsNumber(this string str, string formatString)
        {
            if (string.IsNullOrEmpty(str)) return false;

            return Regex.IsMatch(str, @"^[+-]?\d*[.]?\d*$");
        }

        /// <summary>
        /// 判断字符串是否为有效的Guid格式
        /// </summary>
        /// <param name="guid">要验证的字符串</param>
        /// <returns>如果字符串是有效的Guid格式则返回true，否则返回false</returns>
        public static bool IsGuid(this string guid)
        {
            Guid newId;
            return guid.GetGuid(out newId);
        }

        /// <summary>
        /// 将字符串转换为Guid类型
        /// </summary>
        /// <param name="guid">要转换的字符串</param>
        /// <param name="outId">输出转换后的Guid</param>
        /// <returns>转换成功返回true，否则返回false</returns>
        public static bool GetGuid(this string guid, out Guid outId)
        {
            Guid emptyId = Guid.Empty;
            return Guid.TryParse(guid, out outId);
        }

        ///// <summary>
        ///// 将字符串类型的条件转换为对应的Linq表达式类型
        ///// </summary>
        ///// <param name="stringType">表示条件的字符串</param>
        ///// <returns>对应的Linq表达式类型枚举值</returns>
        ///// <remarks>
        ///// 该方法用于将前端传递的条件字符串映射为后端Linq查询使用的表达式类型，
        ///// 支持包含、大于等于、小于等于、大于、小于、模糊匹配等常见条件类型，
        ///// 默认返回等于(Equal)类型
        ///// </remarks>
        //public static LinqExpressionType GetLinqCondition(this string stringType)
        //{
        //    LinqExpressionType linqExpression;
        //    switch (stringType)
        //    {
        //        case HtmlElementType.Contains:
        //        case HtmlElementType.selectlist:
        //        case nameof(HtmlElementType.Contains):
        //            linqExpression = LinqExpressionType.In;
        //            break;
        //        case HtmlElementType.ThanOrEqual:
        //        case nameof(HtmlElementType.ThanOrEqual):
        //        case HtmlElementType.thanorequal:
        //            linqExpression = LinqExpressionType.ThanOrEqual;
        //            break;
        //        case HtmlElementType.LessOrEqual:
        //        case nameof(HtmlElementType.LessOrEqual):
        //        case HtmlElementType.lessorequal:
        //            linqExpression = LinqExpressionType.LessThanOrEqual;
        //            break;
        //        case HtmlElementType.GT:
        //        case nameof(HtmlElementType.GT):
        //            linqExpression = LinqExpressionType.GreaterThan;
        //            break;
        //        case HtmlElementType.lt:
        //            linqExpression = LinqExpressionType.LessThan;
        //            break;
        //        case HtmlElementType.like:
        //            linqExpression = LinqExpressionType.Contains;
        //            break;
        //        default:
        //            linqExpression = LinqExpressionType.Equal;
        //            break;
        //    }
        //    return linqExpression;
        //}
    }
}
