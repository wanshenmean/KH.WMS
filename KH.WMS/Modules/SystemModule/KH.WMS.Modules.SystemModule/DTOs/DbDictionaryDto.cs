namespace KH.WMS.Modules.SystemModule.DTOs;

/// <summary>表字典（JSON 返回用）</summary>
public class TableDictionary
{
    public string TableName { get; set; } = string.Empty;
    public string? TableDescription { get; set; }
    public List<ColumnDictionary> Columns { get; set; } = new();
}

/// <summary>列字典</summary>
public class ColumnDictionary
{
    public string ColumnName { get; set; } = string.Empty;
    public string PropertyName { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public bool IsNullable { get; set; }
    public bool IsPrimaryKey { get; set; }
    public int? Length { get; set; }
    public string? Description { get; set; }
}

/// <summary>Excel 导出行（中文属性名自动做表头）</summary>
public class ColumnExportRow
{
    public string 表名 { get; set; } = string.Empty;
    public string 表注释 { get; set; } = string.Empty;
    public string 列名 { get; set; } = string.Empty;
    public string 属性名 { get; set; } = string.Empty;
    public string 数据类型 { get; set; } = string.Empty;
    public string 可空 { get; set; } = string.Empty;
    public string 主键 { get; set; } = string.Empty;
    public string 长度 { get; set; } = string.Empty;
    public string 注释 { get; set; } = string.Empty;
}
