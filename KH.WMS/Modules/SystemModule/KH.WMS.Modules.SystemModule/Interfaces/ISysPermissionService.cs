using KH.WMS.Core.Api.Responses;
using KH.WMS.Core.Services;
using KH.WMS.Entities.System;
using KH.WMS.Modules.SystemModule.DTOs;

namespace KH.WMS.Modules.SystemModule.Interfaces
{
    /// <summary>
    /// 系统菜单权限服务接口
    /// </summary>
    public interface ISysPermissionService : ICrudService<SysPermission>
    {
        /// <summary>
        /// 获取菜单树（含按钮）
        /// </summary>
        Task<ApiResponse> GetMenuTreeAsync();

        /// <summary>
        /// 根据角色ID获取菜单树（含按钮）
        /// </summary>
        Task<ApiResponse> GetMenuTreeByRoleIdAsync(long roleId);

        /// <summary>
        /// 获取所有目录列表
        /// </summary>
        Task<ApiResponse> GetDirectoriesAsync();

        /// <summary>
        /// 根据目录ID获取菜单列表
        /// </summary>
        Task<ApiResponse> GetMenusByDirectoryIdAsync(long directoryId);

        /// <summary>
        /// 添加/编辑菜单（含按钮）
        /// </summary>
        Task<ApiResponse> SaveMenuAsync(SaveMenuDto dto);

        /// <summary>
        /// 分配角色菜单权限
        /// </summary>
        Task<ApiResponse> AssignRolePermissionsAsync(AssignPermissionDto dto);
    }
}
