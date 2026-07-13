using System.ComponentModel;
using System.Reflection;
using KH.WMS.Core.DependencyInjection.ServiceLifetimes;
using KH.WMS.Core.ImportExport;
using KH.WMS.Modules.SystemModule.DTOs;
using KH.WMS.Modules.SystemModule.Interfaces;
using SqlSugar;

namespace KH.WMS.Modules.SystemModule.Services;

/// <summary>
/// 数据库字典服务：反射读 KH.WMS.Entities 程序集中所有 SqlSugar 实体的特性，
/// 生成表/列字典，支持 JSON 返回和 Excel 导出。
/// </summary>
[RegisteredService(ServiceType = typeof(IDbDictionaryService))]
public class DbDictionaryService : IDbDictionaryService
{
    /// <inheritdoc />
    public Task<List<TableDictionary>> GetAsync()
    {
        return Task.FromResult(BuildTables());
    }

    /// <inheritdoc />
    public async Task<byte[]> ExportAsync()
    {
        var tables = BuildTables();
        var rows = tables.SelectMany(t => t.Columns.Select(c => new ColumnExportRow
        {
            表名 = t.TableName,
            表注释 = t.TableDescription ?? string.Empty,
            列名 = c.ColumnName,
            属性名 = c.PropertyName,
            数据类型 = c.DataType,
            可空 = c.IsNullable ? "是" : "否",
            主键 = c.IsPrimaryKey ? "是" : string.Empty,
            长度 = c.Length?.ToString() ?? string.Empty,
            注释 = c.Description ?? string.Empty,
        })).ToList();

        return await ExcelHelper.ExportAsync(rows, "数据库字典");
    }

    /// <summary>
    /// 反射读实体程序集，构建表字典
    /// </summary>
    private static List<TableDictionary> BuildTables()
    {
        var assembly = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(a => a.GetName().Name == "KH.WMS.Entities");

        if (assembly == null)
            return new List<TableDictionary>();

        var result = new List<TableDictionary>();

        var entityTypes = assembly.GetTypes()
            .Where(t => t.GetCustomAttribute<SugarTable>() != null && !t.IsAbstract)
            .OrderBy(t => t.GetCustomAttribute<SugarTable>()!.TableName);

        foreach (var type in entityTypes)
        {
            var tableAttr = type.GetCustomAttribute<SugarTable>()!;
            var table = new TableDictionary
            {
                TableName = tableAttr.TableName,
                TableDescription = type.GetCustomAttribute<DescriptionAttribute>()?.Description,
                Columns = new List<ColumnDictionary>()
            };

            foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var colAttr = prop.GetCustomAttribute<SugarColumn>();
                if (colAttr == null || colAttr.IsIgnore) continue;

                table.Columns.Add(new ColumnDictionary
                {
                    ColumnName = !string.IsNullOrEmpty(colAttr.ColumnName) ? colAttr.ColumnName : prop.Name,
                    PropertyName = prop.Name,
                    DataType = !string.IsNullOrEmpty(colAttr.ColumnDataType)
                        ? colAttr.ColumnDataType
                        : MapClrType(prop.PropertyType),
                    IsNullable = colAttr.IsNullable,
                    IsPrimaryKey = prop.Name == "Id" || colAttr.IsPrimaryKey,
                    Length = colAttr.Length > 0 ? colAttr.Length : null,
                    Description = colAttr.ColumnDescription
                });
            }

            result.Add(table);
        }

        return result;
    }

    /// <summary>
    /// 将 CLR 类型映射为数据库类型名（SugarColumn.ColumnDataType 为空时兜底）
    /// </summary>
    private static string MapClrType(Type type)
    {
        var underlying = Nullable.GetUnderlyingType(type) ?? type;
        return underlying.Name switch
        {
            "String" => "nvarchar",
            "Int32" => "int",
            "Int64" => "bigint",
            "Decimal" => "decimal",
            "DateTime" => "datetime",
            "Boolean" => "bit",
            "Byte" => "tinyint",
            "DateOnly" => "date",
            "TimeSpan" => "time",
            "Guid" => "uniqueidentifier",
            _ => underlying.Name.ToLower()
        };
    }
}
