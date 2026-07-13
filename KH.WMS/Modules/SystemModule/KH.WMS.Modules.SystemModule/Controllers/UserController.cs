using KH.WMS.Core.Api.Responses;
using KH.WMS.Core.Controllers;
using KH.WMS.Core.Models.Dtos;
using KH.WMS.Core.Security.Encryption;
using KH.WMS.Entities.System;
using KH.WMS.Modules.SystemModule.DTOs;
using KH.WMS.Modules.SystemModule.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KH.WMS.Modules.SystemModule.Controllers
{
    [ApiController]
    [Route("api/user")]
    public class UserController(ISysUserService userService, IRsaCryptoService rsaCryptoService) : CrudController<SysUser>(userService)
    {
        private readonly ISysUserService _userService = userService;
        private readonly IRsaCryptoService _rsaCryptoService = rsaCryptoService;

        [HttpPost("login"), AllowAnonymous]
        public async Task<ApiResponse> LoginAsync([FromBody] LoginDTO loginDTO)
        {
            return await _userService.LoginAsync(loginDTO);
        }

        /// <summary>
        /// 获取RSA公钥（登录密码加密用）
        /// </summary>
        [HttpGet("public-key"), AllowAnonymous]
        public ApiResponse GetPublicKey()
        {
            return ApiResponse.Ok(_rsaCryptoService.GetPublicKey());
        }

        [HttpPost("logout"), AllowAnonymous]
        public async Task<ApiResponse> LogoutAsync()
        {
            return await _userService.LogoutAsync();
        }

        /// <summary>
        /// 新增/编辑用户
        /// </summary>
        [HttpPost("save")]
        public async Task<ApiResponse> Save([FromBody] SaveUserDto dto)
        {
            return await _userService.SaveUserAsync(dto);
        }

        /// <summary>
        /// 用户分配角色
        /// </summary>
        [HttpPost("assign-roles")]
        public async Task<ApiResponse> AssignRoles([FromBody] AssignUserRoleDto dto)
        {
            return await _userService.AssignRolesAsync(dto);
        }

        /// <summary>
        /// 重置密码
        /// </summary>
        [HttpPost("reset-password/{id}")]
        public async Task<ApiResponse> ResetPassword(long id)
        {
            return await _userService.ResetPasswordAsync(id);
        }

        /// <summary>
        /// 切换用户状态（重写基类方法，保留系统用户校验逻辑）
        /// </summary>
        [HttpPut("status/{id}")]
        public override async Task<ApiResponse> SetStatus(long id, [FromBody] SetStatusDto dto)
        {
            return await _userService.ToggleStatusAsync(id);
        }
    }
}
