using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace KH.WMS.Core.UserProvide
{
    public interface IUserContext
    {
        /// <summary>
        /// 名称
        /// </summary>
        string? UserName { get; }

        /// <summary>
        /// UserId
        /// </summary>
        long? UserId { get; }

        /// <summary>
        /// 角色Id
        /// </summary>
        long? RoleId { get; }

        /// <summary>
        /// token
        /// </summary>
        string? Token { get; }

        /// <summary>
        /// 当前用户权限列表
        /// </summary>
        List<string> Permissions { get; }

        /// <summary>
        /// 菜单类型
        /// </summary>
        int? MenuType { get; }

        /// <summary>
        /// 获取当前用户是否已认证
        /// </summary>
        bool IsAuthenticated { get; }

        /// <summary>
        /// 是否是超级管理员（拥有系统最高权限，绕过权限检查）
        /// </summary>
        bool IsSuperAdmin { get; }

        /// <summary>
        /// 获取当前HTTP上下文
        /// </summary>
        HttpContext? HttpContext { get; }

        /// <summary>
        /// 获取当前用户的认证令牌
        /// </summary>
        /// <returns>用户认证令牌字符串</returns>
        string? GetToken();

        /// <summary>
        /// 获取当前用户的所有Claims
        /// </summary>
        IEnumerable<Claim> GetClaims();

    }
}
