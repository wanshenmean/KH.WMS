using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using KH.WMS.Core.Authentication;
using KH.WMS.Core.Caching;
using KH.WMS.Core.Constants;
using KH.WMS.Core.DependencyInjection.ServiceLifetimes;
using KH.WMS.Core.Helpers;
using Microsoft.AspNetCore.Http;
using ICacheService = KH.WMS.Core.Caching.ICacheService;

namespace KH.WMS.Core.UserProvide
{
    [RegisteredService(WithoutInterceptor = true)]
    public class UserContext(IHttpContextAccessor httpContextAccessor, ICacheService cacheService, IJwtTokenService jwtTokenService) : IUserContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly ICacheService _cacheService = cacheService;
        private readonly IJwtTokenService _jwtTokenService = jwtTokenService;

        private string? _token = null;
        private string? _userName = null;
        private long? _userId = null;
        private long? _roleId = null;
        private bool _tokenResolved = false;

        /// <summary>
        /// 获取当前HTTP上下文
        /// </summary>
        public HttpContext? HttpContext => _httpContextAccessor.HttpContext;

        /// <summary>
        /// 获取当前用户名
        /// </summary>
        public string? UserName => GetUserName();

        /// <summary>
        /// 获取当前用户ID
        /// </summary>
        public long? UserId => GetUserId();

        public long? RoleId => GetRoleId();

        public string? Token => GetToken();

        public int? MenuType => (HttpContext?.Request.Headers.ContainsKey("uniapp") ?? false) ? 1 : 0;

        public bool IsAuthenticated => HttpContext?.User?.Identity?.IsAuthenticated == true;

        public bool IsSuperAdmin => RoleId == RoleConstants.SUPER_ADMIN_ROLE_ID; // 超级管理员角色ID，见 RoleConstants

        /// <summary>
        /// 当前用户权限列表
        /// </summary>
        public List<string> Permissions { get; private set; } = new();


        private long? GetUserId()
        {
            if (_userId == null || _userId == 0)
            {
                EnsureToken();
                _userId = _jwtTokenService.GetUserIdFromToken(_token ?? "");
            }

            return _userId;
        }

        private string? GetUserName()
        {
            if (string.IsNullOrEmpty(_userName))
            {
                EnsureToken();
                _userName = _jwtTokenService.GetUsernameFromToken(_token ?? "");
            }

            return _userName;
        }

        private long? GetRoleId()
        {
            if (_roleId == null || _roleId == 0)
            {
                EnsureToken();
                _roleId = int.TryParse(_jwtTokenService.GetRolesFromToken(_token ?? "")?.FirstOrDefault(), out var roleId) ? roleId : 0;
            }
            return _roleId;
        }

        public IEnumerable<Claim> GetClaims()
        {
            if (HttpContext?.User == null)
                return Enumerable.Empty<Claim>();

            return HttpContext.User.Claims;
        }

        public string? GetToken()
        {
            EnsureToken();
            return _token;
        }

        /// <summary>
        /// 同一请求内只解析一次 token，缓存结果。
        /// 当前仅从 Authorization: Bearer 头获取（JwtBearer 中间件已对其完成签名校验）。
        /// 用户信息（UserId/UserName/RoleId）统一从该 token 解析，不依赖缓存。
        /// </summary>
        private void EnsureToken()
        {
            if (_tokenResolved) return;
            _tokenResolved = true;

            var authHeader = HttpContext?.Request?.Headers[HeaderConstants.Authentication.AUTHORIZATION].ObjToString();
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith(HeaderConstants.Authentication.BEARER_PREFIX, StringComparison.OrdinalIgnoreCase))
            {
                _token = authHeader[HeaderConstants.Authentication.BEARER_PREFIX.Length..];
            }
            // 无 Bearer 头（如无 HTTP 上下文的后台任务）则 _token 保持 null，访问器返回 null
        }

    }
}
