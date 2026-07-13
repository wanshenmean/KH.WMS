using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using KH.WMS.Core.Models.Dtos;

namespace KH.WMS.Core.Helpers;

/// <summary>
/// 表达式树组合工具
/// </summary>
public static class ExpressionHelper
{
    /// <summary>
    /// 创建恒为 true 的表达式
    /// </summary>
    public static Expression<Func<T, bool>> True<T>()
    {
        return _ => true;
    }

    /// <summary>
    /// 创建恒为 false 的表达式
    /// </summary>
    public static Expression<Func<T, bool>> False<T>()
    {
        return _ => false;
    }

    /// <summary>
    /// 组合两个 Expression 为 And 关系
    /// </summary>
    public static Expression<Func<T, bool>> And<T>(
        this Expression<Func<T, bool>> first,
        Expression<Func<T, bool>> second)
    {
        return Combine(first, second, Expression.AndAlso);
    }

    /// <summary>
    /// 组合两个 Expression 为 Or 关系
    /// </summary>
    public static Expression<Func<T, bool>> Or<T>(
        this Expression<Func<T, bool>> first,
        Expression<Func<T, bool>> second)
    {
        return Combine(first, second, Expression.OrElse);
    }

    /// <summary>
    /// 根据 FilterCondition 列表动态构建 Expression（自动过滤无效字段）
    /// </summary>
    public static Expression<Func<T, bool>> BuildFilter<T>(List<FilterCondition>? filters)
    {
        if (filters == null || filters.Count == 0)
            return True<T>();

        var validProperties = GetValidPropertyNames(typeof(T));
        var expression = True<T>();

        foreach (var filter in filters)
        {
            if (string.IsNullOrWhiteSpace(filter.Field))
                continue;

            // 过滤掉不属于实体属性的字段
            if (!validProperties.Contains(filter.Field))
                continue;

            var condition = BuildSingleFilter<T>(filter);
            if (condition != null)
                expression = expression.And(condition);
        }

        return expression;
    }

    /// <summary>
    /// 获取类型所有可公开读写的属性名集合
    /// </summary>
    public static HashSet<string> GetValidPropertyNames(Type type)
    {
        return new HashSet<string>(type.GetProperties()
            .Where(p => p.CanRead && p.CanWrite)
            .Select(p => p.Name), StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// 构建单个过滤条件表达式
    /// </summary>
    private static Expression<Func<T, bool>>? BuildSingleFilter<T>(FilterCondition filter)
    {
        var parameter = Expression.Parameter(typeof(T), "x");
        var property = Expression.PropertyOrField(parameter, filter.Field);
        var propertyType = property.Type;

        Expression body;
        var op = filter.Operator.ToLowerInvariant();

        switch (op)
        {
            case "equals":
                var equalValue = ConvertValue(filter.Value, propertyType);
                body = Expression.Equal(property, Expression.Constant(equalValue, propertyType));
                break;

            case "contains":
            case "startswith":
            case "endwith":
                if (propertyType == typeof(string))
                {
                    var strValue = Expression.Constant(filter.Value?.ToString() ?? "", typeof(string));
                    MethodInfo stringMethod;
                    if (op == "contains")
                        stringMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) })!;
                    else if (op == "startswith")
                        stringMethod = typeof(string).GetMethod("StartsWith", new[] { typeof(string) })!;
                    else
                        stringMethod = typeof(string).GetMethod("EndsWith", new[] { typeof(string) })!;
                    body = Expression.Call(property, stringMethod, strValue);
                }
                else
                {
                    var convertedValue = ConvertValue(filter.Value, propertyType);
                    body = Expression.Equal(property, Expression.Constant(convertedValue, propertyType));
                }
                break;

            case "gt":
                body = Expression.GreaterThan(property,
                    Expression.Constant(ConvertValue(filter.Value, propertyType), propertyType));
                break;

            case "lt":
                body = Expression.LessThan(property,
                    Expression.Constant(ConvertValue(filter.Value, propertyType), propertyType));
                break;

            case "gte":
                body = Expression.GreaterThanOrEqual(property,
                    Expression.Constant(ConvertValue(filter.Value, propertyType), propertyType));
                break;

            case "lte":
                body = Expression.LessThanOrEqual(property,
                    Expression.Constant(ConvertValue(filter.Value, propertyType), propertyType));
                break;

            case "notnull":
                body = Expression.NotEqual(property, Expression.Constant(null, propertyType));
                break;

            case "isnull":
                body = Expression.Equal(property, Expression.Constant(null, propertyType));
                break;

            case "in":
                body = BuildInExpression(property, filter.Value, propertyType);
                break;

            default:
                return null;
        }

        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }

    /// <summary>
    /// 构建 IN 表达式
    /// </summary>
    private static Expression BuildInExpression(MemberExpression property, object? value, Type propertyType)
    {
        if (value == null)
            return Expression.Constant(false);

        var ty = value.ToString();

        var values = new List<object?>();
        if (value is string strValue && strValue.Contains(","))
        {
            values = strValue.Split(',').Select(v => ConvertValue(v, propertyType)).ToList();
        }
        else if (value is System.Collections.IEnumerable enumerableValue && !(value is string))
        {
            values = enumerableValue.Cast<object>().Select(v => ConvertValue(v, propertyType)).ToList();
        }
        else if (value is Array array && array.Length > 0)
        {
            values = array.Cast<object>().Select(v => ConvertValue(v, propertyType)).ToList();
        }
        else if(value is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Array)
        {
            values = jsonElement.EnumerateArray().Select(v => ConvertValue(v.ToString(), propertyType)).ToList();
        }

        if (values == null)
        {
            var singleValue = ConvertValue(value, propertyType);
            return Expression.Equal(property, Expression.Constant(singleValue, propertyType));
        }

        var valueList = values.Cast<object>().Select(v => ConvertValue(v, propertyType)).ToList();
        if (valueList.Count == 0)
            return Expression.Constant(false);

        // 构建 OR 链: x == v1 OR x == v2 OR ...
        Expression? orBody = null;
        foreach (var val in valueList)
        {
            var equal = Expression.Equal(property, Expression.Constant(val, propertyType));
            orBody = orBody == null ? equal : Expression.OrElse(orBody, equal);
        }

        return orBody ?? Expression.Constant(false);
    }

    /// <summary>
    /// 将值转换为目标类型
    /// </summary>
    private static object? ConvertValue(object? value, Type targetType)
    {
        if (value == null)
            return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;

        var valueType = value.GetType();

        if (targetType.IsAssignableFrom(valueType))
            return value;

        var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        // 处理 JSON 反序列化产生的中间类型（JObject/JArray/JsonElement 等），
        // 这些类型未实现 IConvertible，需要先转为字符串再进行转换。
        if (value is not IConvertible)
        {
            var str = value.ToString();
            if (str == null)
                return targetType.IsValueType ? Activator.CreateInstance(targetType) : null;

            if (underlyingType == typeof(Guid))
                return Guid.Parse(str);
            if (underlyingType.IsEnum)
                return Enum.Parse(underlyingType, str);
            if (underlyingType == typeof(DateTime))
                return DateTime.Parse(str);
            if (underlyingType == typeof(decimal))
                return decimal.Parse(str);
            if (underlyingType == typeof(double))
                return double.Parse(str);
            if (underlyingType == typeof(float))
                return float.Parse(str);
            if (underlyingType == typeof(long))
                return long.Parse(str);
            if (underlyingType == typeof(int))
                return int.Parse(str);

            return Convert.ChangeType(str, underlyingType);
        }

        if (underlyingType == typeof(Guid) && value is string guidStr)
            return Guid.Parse(guidStr);

        if (underlyingType.IsEnum)
            return Enum.Parse(underlyingType, value.ToString()!);

        return Convert.ChangeType(value, underlyingType);
    }

    /// <summary>
    /// 组合两个 Lambda 表达式
    /// </summary>
    private static Expression<Func<T, bool>> Combine<T>(
        Expression<Func<T, bool>> first,
        Expression<Func<T, bool>> second,
        Func<Expression, Expression, Expression> combineFunc)
    {
        var parameter = Expression.Parameter(typeof(T), "x");

        var leftVisitor = new ReplaceExpressionVisitor(first.Parameters[0], parameter);
        var left = leftVisitor.Visit(first.Body);

        var rightVisitor = new ReplaceExpressionVisitor(second.Parameters[0], parameter);
        var right = rightVisitor.Visit(second.Body);

        return Expression.Lambda<Func<T, bool>>(
            combineFunc(left!, right!), parameter);
    }

    private class ReplaceExpressionVisitor(
        Expression oldValue,
        Expression newValue) : ExpressionVisitor
    {
        private readonly Expression _oldValue = oldValue;
        private readonly Expression _newValue = newValue;

        public override Expression? Visit(Expression? node)
        {
            return node == _oldValue ? _newValue : base.Visit(node);
        }
    }
}
