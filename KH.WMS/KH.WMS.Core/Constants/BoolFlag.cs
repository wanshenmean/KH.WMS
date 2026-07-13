namespace KH.WMS.Core.Constants;

/// <summary>
/// 布尔标志，用于 byte 类型的布尔字段，替代数据库中的 bit 类型。
/// 基础常量，所有项目共用。
/// </summary>
public static class BoolFlag
{
    /// <summary>
    /// 是。表示启用、激活、允许等肯定含义。
    /// </summary>
    public const byte YES = 1;

    /// <summary>
    /// 否。表示禁用、停用、不允许等否定含义。
    /// </summary>
    public const byte NO = 0;
}
