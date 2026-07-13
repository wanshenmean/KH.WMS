using KH.WMS.Core.Api.Responses;
using KH.WMS.Core.Controllers;
using KH.WMS.Entities.System;
using KH.WMS.Modules.SystemModule.DTOs;
using KH.WMS.Modules.SystemModule.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace KH.WMS.Modules.SystemModule.Controllers
{
    [ApiController]
    [Route("api/permission")]
    public class PermissionController(ISysPermissionService permissionService) : CrudController<SysPermission>(permissionService)
    {
        private readonly ISysPermissionService _permissionService = permissionService;

        /// <summary>
        /// 获取菜单树（含按钮）
        /// </summary>
        [HttpGet("menu-tree")]
        public async Task<ApiResponse> GetMenuTree()
        {
            return await _permissionService.GetMenuTreeAsync();
        }

        /// <summary>
        /// 根据角色获取菜单树（含按钮）
        /// </summary>
        [HttpGet("menu-tree/role/{roleId}")]
        public async Task<ApiResponse> GetMenuTreeByRole(long roleId)
        {
            return await _permissionService.GetMenuTreeByRoleIdAsync(roleId);
        }

        /// <summary>
        /// 获取所有目录列表
        /// </summary>
        [HttpGet("directories")]
        public async Task<ApiResponse> GetDirectories()
        {
            return await _permissionService.GetDirectoriesAsync();
        }

        /// <summary>
        /// 根据目录ID获取菜单列表
        /// </summary>
        [HttpGet("menus/{directoryId}")]
        public async Task<ApiResponse> GetMenusByDirectory(long directoryId)
        {
            return await _permissionService.GetMenusByDirectoryIdAsync(directoryId);
        }

        /// <summary>
        /// 新增/编辑菜单（含按钮）
        /// </summary>
        [HttpPost("save")]
        public async Task<ApiResponse> SaveMenu([FromBody] SaveMenuDto dto)
        {
            return await _permissionService.SaveMenuAsync(dto);
        }

        /// <summary>
        /// 分配角色菜单权限
        /// </summary>
        [HttpPost("assign")]
        public async Task<ApiResponse> AssignPermissions([FromBody] AssignPermissionDto dto)
        {
            return await _permissionService.AssignRolePermissionsAsync(dto);
        }
    }
}
