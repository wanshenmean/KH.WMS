using KH.WMS.Core.Api.Responses;
using KH.WMS.Core.Controllers;
using KH.WMS.Entities.System;
using KH.WMS.Modules.SystemModule.DTOs;
using KH.WMS.Modules.SystemModule.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace KH.WMS.Modules.SystemModule.Controllers
{
    [ApiController]
    [Route("api/role")]
    public class RoleController(ISysRoleService roleService) : CrudController<SysRole>(roleService)
    {
        private readonly ISysRoleService _roleService = roleService;

        /// <summary>
        /// 获取角色选项（当前角色及下级，用于级联选择）
        /// </summary>
        [HttpGet("options")]
        public async Task<ApiResponse> GetOptions()
        {
            return await _roleService.GetAllRolesAsync();
        }

        /// <summary>
        /// 新增/编辑角色
        /// </summary>
        [HttpPost("save")]
        public async Task<ApiResponse> Save([FromBody] SaveRoleDto dto)
        {
            return await _roleService.SaveRoleAsync(dto);
        }

        /// <summary>
        /// 角色分配用户
        /// </summary>
        [HttpPost("assign-users")]
        public async Task<ApiResponse> AssignUsers([FromBody] AssignRoleUserDto dto)
        {
            return await _roleService.AssignUsersAsync(dto);
        }

        /// <summary>
        /// 获取角色下的用户列表
        /// </summary>
        [HttpGet("users/{roleId}")]
        public async Task<ApiResponse> GetRoleUsers(long roleId)
        {
            return await _roleService.GetRoleUsersAsync(roleId);
        }
    }
}
