namespace KH.WMS.Modules.SystemModule.DTOs
{
    /// <summary>
    /// 保存系统参数请求 DTO
    /// </summary>
    public class SaveParameterDto
    {
        public long? Id { get; set; }
        public string ParamCode { get; set; } = string.Empty;
        public string ParamName { get; set; } = string.Empty;
        public string ParamValue { get; set; } = string.Empty;
        public string? ParamGroup { get; set; }
        public string ParamType { get; set; } = "STRING";
        public string? Remark { get; set; }
    }
}
