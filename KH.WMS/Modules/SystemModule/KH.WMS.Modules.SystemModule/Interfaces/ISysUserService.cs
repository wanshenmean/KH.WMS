using KH.WMS.Core.Api.Responses;
using KH.WMS.Core.Services;
using KH.WMS.Entities.System;
using KH.WMS.Modules.SystemModule.DTOs;

namespace KH.WMS.Modules.SystemModule.Interfaces
{
    public interface ISysUserService: ICrudService<SysUser>
    {
        Task<ApiResponse> LoginAsync(LoginDTO loginDTO);

        Task<ApiResponse> LogoutAsync();

        /// <summary>
        /// 用户详情
        /// </summary>
        Task<ApiResponse> GetUserDetailAsync(long userId);

        /// <summary>
        /// 新增/编辑用户
        /// </summary>
        Task<ApiResponse> SaveUserAsync(SaveUserDto dto);

        /// <summary>
        /// 删除用户
        /// </summary>
        Task<ApiResponse> DeleteUserAsync(long userId);

        /// <summary>
        /// 用户分配角色
        /// </summary>
        Task<ApiResponse> AssignRolesAsync(AssignUserRoleDto dto);

        /// <summary>
        /// 重置密码
        /// </summary>
        Task<ApiResponse> ResetPasswordAsync(long userId);

        /// <summary>
        /// 切换用户状态
        /// </summary>
        Task<ApiResponse> ToggleStatusAsync(long userId);
    }
}
