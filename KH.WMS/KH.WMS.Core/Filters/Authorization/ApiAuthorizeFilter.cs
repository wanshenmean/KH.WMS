using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using KH.WMS.Core.Caching;
using KH.WMS.Core.Constants;
using KH.WMS.Core.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using ICacheService = KH.WMS.Core.Caching.ICacheService;
using KH.WMS.Core.Authentication;

namespace KH.WMS.Core.Filters.Authorization
{
    public class ApiAuthorizeFilter : IAuthorizationFilter
    {
        private readonly ICacheService _cacheService;
        private readonly IJwtTokenService _jwtTokenService;

        public ApiAuthorizeFilter(ICacheService cacheService, IJwtTokenService jwtTokenService)
        {
            _cacheService = cacheService;
            _jwtTokenService = jwtTokenService;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // 匿名接口直接放行
            if (context.ActionDescriptor.EndpointMetadata.Any(item => item is IAllowAnonymous))
            {
                return;
            }

            // JwtBearer 中间件已对每个携带 Bearer token 的请求完成 HS256 签名校验，
            // 验签通过即置 HttpContext.User 为已认证。未通过认证（无 token / 签名无效 / 已过期）一律 401。
            if (!(context.HttpContext.User.Identity?.IsAuthenticated ?? false))
            {
                WriteUnauthorized(context, ErrorConstants.Authentication.TOKEN_INVALID);
                return;
            }

            // 已认证请求：从已验签的 token 中提取 userId，做多端登录踢下线与临近过期续签
            var token = GetTokenFromHeader(context);
            var userId = _jwtTokenService.GetUserIdFromToken(token);
            if (userId == null)
            {
                WriteUnauthorized(context, ErrorConstants.Authentication.TOKEN_INVALID);
                return;
            }

            var cachedToken = _cacheService.Get<string>(CacheConstants.Token.PREFIX + userId);

            // ALLOW_MULTI_LOGIN=false 时，缓存中的 token 与当前请求的 token 不一致说明已被挤下线
            if (!IsMultiLoginAllowed() && !string.IsNullOrEmpty(cachedToken) && cachedToken != token)
            {
                WriteUnauthorized(context, ErrorConstants.Authentication.ACCOUNT_KICKED);
                return;
            }

            // Token 即将过期时，通过响应头通知前端替换 token
            if (_jwtTokenService.ShouldRefreshToken(token))
            {
                RefreshTokenIfNeeded(context, userId.Value, token);
            }
        }

        /// <summary>
        /// Token 快过期时，生成新 token 并通过响应头下发给前端，同时更新缓存
        /// </summary>
        private void RefreshTokenIfNeeded(AuthorizationFilterContext context, long userId, string currentToken)
        {
            var username = _jwtTokenService.GetUsernameFromToken(currentToken);
            if (username == null) return;

            // 生成新 token 和 refresh token
            var newAccessToken = _jwtTokenService.GenerateAccessToken(userId, username, 0);
            var newRefreshToken = _jwtTokenService.GenerateRefreshToken();

            // 更新缓存中的 token（过期时间与登录时一致，避免续期后永驻）
            _cacheService.SetOrCreate(CacheConstants.Token.PREFIX + userId, newAccessToken, TimeSpan.FromMinutes(GetTokenExpireMinutes()));

            // 通过响应头通知前端静默替换 token
            context.HttpContext.Response.Headers[HeaderConstants.Authentication.X_ACCESS_TOKEN] = newAccessToken;
            context.HttpContext.Response.Headers[HeaderConstants.Authentication.X_REFRESH_TOKEN] = newRefreshToken;
        }

        private bool IsMultiLoginAllowed()
        {
            var value = _cacheService.Get<string>(CacheConstants.SysParam.GetKey(SysParamConstants.ALLOW_MULTI_LOGIN));
            if (string.IsNullOrEmpty(value)) return true;
            return value == SysParamConstants.MULTI_LOGIN_ENABLED
                || string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 获取 Token 过期时间（分钟），与登录写入缓存时保持一致
        /// </summary>
        private int GetTokenExpireMinutes()
        {
            var value = _cacheService.Get<string>(CacheConstants.SysParam.GetKey(SysParamConstants.TOKEN_EXPIRE_MIN));
            if (!string.IsNullOrEmpty(value) && int.TryParse(value, out var minutes) && minutes > 0)
                return minutes;
            return int.Parse(SysParamConstants.TOKEN_EXPIRE_MIN_DEFAULT);
        }

        private void WriteUnauthorized(AuthorizationFilterContext context, string message)
        {
            context.Result = new ContentResult()
            {
                Content = new { message, status = false, code = (int)HttpStatusCode.Unauthorized }.Serialize(),
                ContentType = HeaderConstants.ContentTypes.APPLICATION_JSON,
                StatusCode = (int)HttpStatusCode.Unauthorized
            };
        }

        /// <summary>
        /// 从请求头提取 Bearer Token
        /// </summary>
        private string GetTokenFromHeader(AuthorizationFilterContext context)
        {
            return context.HttpContext.Request.Headers[HeaderConstants.Authentication.AUTHORIZATION]
                .ObjToString().Replace(HeaderConstants.Authentication.BEARER_PREFIX, "").Trim();
        }
    }
}
