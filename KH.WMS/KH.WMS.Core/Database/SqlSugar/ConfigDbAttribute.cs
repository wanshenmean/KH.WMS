namespace KH.WMS.Core.Database.SqlSugar;

/// <summary>
/// 标记实体存入配置库（SQLite），加在实体类上即可自动路由
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class ConfigDbAttribute : Attribute;
