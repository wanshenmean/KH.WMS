namespace KH.WMS.Core.Constants;

/// <summary>
/// 角色相关常量
/// </summary>
public static class RoleConstants
{
    /// <summary>
    /// 超级管理员角色ID。
    /// <para>约定：RoleId 等于此值的用户为超级管理员，拥有全部权限，不受角色-权限绑定约束。</para>
    /// <para>该值须与初始化种子数据中的超管角色ID保持一致，变更时需同步调整种子数据。</para>
    /// </summary>
    public const long SUPER_ADMIN_ROLE_ID = 1;
}
