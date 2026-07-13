namespace KH.WMS.Core.Validation;

/// <summary>
/// 校验器编码常量，用于 [ConfigValidation(ValidatorCodes.XXX)] 和 IValidator.Code
/// </summary>
public static class ValidatorCodes
{
    // 组盘校验
    public const string BIND_DATA_NOT_EMPTY = "BIND_DATA_NOT_EMPTY";
    public const string BIND_QUANTITY = "BIND_QUANTITY";
    public const string BATCH_NO_REQUIRED = "BATCH_NO_REQUIRED";
    public const string EXPIRY_DATE_REQUIRED = "EXPIRY_DATE_REQUIRED";
    public const string MIXED_MATERIAL = "MIXED_MATERIAL";
    public const string MIXED_BATCH = "MIXED_BATCH";
}
