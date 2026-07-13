using KH.WMS.Core.Api.Responses;
using KH.WMS.Core.Services;
using KH.WMS.Entities.System;
using KH.WMS.Modules.SystemModule.DTOs;

namespace KH.WMS.Modules.SystemModule.Interfaces
{
    /// <summary>
    /// 系统角色服务接口
    /// </summary>
    public interface ISysRoleService : ICrudService<SysRole>
    {
        /// <summary>
        /// 所有启用的角色（下拉选择用）
        /// </summary>
        Task<ApiResponse> GetAllRolesAsync();

        /// <summary>
        /// 新增/编辑角色
        /// </summary>
        Task<ApiResponse> SaveRoleAsync(SaveRoleDto dto);

        /// <summary>
        /// 角色分配用户
        /// </summary>
        Task<ApiResponse> AssignUsersAsync(AssignRoleUserDto dto);

        /// <summary>
        /// 获取角色下的用户列表
        /// </summary>
        Task<ApiResponse> GetRoleUsersAsync(long roleId);

        /// <summary>
        /// 获取指定角色及其所有子孙角色ID集合（包含自身角色）。
        /// 超级管理员传入时返回所有角色ID。
        /// </summary>
        Task<List<long>> GetDescendantRoleIdsAsync(long roleId);
    }
}
