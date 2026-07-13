using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using KH.WMS.Core.Helpers;
using Microsoft.AspNetCore.Http;
using static KH.WMS.Core.Constants.HeaderConstants;

namespace KH.WMS.Core.Extensions
{
    //public static class SwaggerContextExtension
    //{
    //    public const string SwaggerCodeKey = "swagger-code";
    //    public const string SwaggerJwt = "swagger-jwt";

    //    /// <summary>
    //    /// 检查当前Swagger请求是否验证成功
    //    /// </summary>
    //    /// <returns>如果Session中Swagger验证码为"success"则返回true，否则返回false</returns>
    //    public static bool IsSuccessSwagger()
    //    {
    //        return App.HttpContext?.GetSession()?.GetString(SwaggerCodeKey) == "success";
    //    }

    //    /// <summary>
    //    /// 判断当前Swagger请求是否已通过验证
    //    /// </summary>
    //    /// <param name="context">Http上下文对象</param>
    //    /// <returns>如果Swagger验证成功返回true，否则返回false</returns>
    //    public static bool IsSuccessSwagger(this HttpContext context)
    //    {
    //        return context.GetSession()?.GetString(SwaggerCodeKey) == "success";
    //    }

    //    /// <summary>
    //    /// 设置Swagger验证成功状态
    //    /// </summary>
    //    /// <remarks>
    //    /// 将Swagger验证状态码"success"存储到当前会话中
    //    /// </remarks>
    //    public static void SuccessSwagger()
    //    {
    //        App.HttpContext?.GetSession()?.SetString(SwaggerCodeKey, "success");
    //    }

    //    /// <summary>
    //    /// 设置Swagger操作成功的状态码
    //    /// </summary>
    //    /// <param name="context">Http上下文对象</param>
    //    public static void SuccessSwagger(this HttpContext context)
    //    {
    //        context.GetSession()?.SetString(SwaggerCodeKey, "success");
    //    }

    //    /// <summary>
    //    /// 为Swagger JWT认证设置成功上下文
    //    /// </summary>
    //    /// <param name="context">Http上下文对象</param>
    //    /// <param name="token">JWT令牌</param>
    //    /// <remarks>
    //    /// 将JWT令牌解析为ClaimsIdentity并添加到用户身份中，同时将令牌存储在会话中
    //    /// </remarks>
    //    public static void SuccessSwaggerJwt(this HttpContext context, string token)
    //    {
    //        var claims = new ClaimsIdentity(GetClaimsIdentity(token));
    //        context.User.AddIdentity(claims);
    //        context.GetSession().SetString(SwaggerJwt, token);
    //    }

    //    /// <summary>
    //    /// 从JWT令牌中获取声明(Claims)集合
    //    /// </summary>
    //    /// <param name="token">JWT令牌字符串</param>
    //    /// <returns>包含声明信息的集合，若令牌无效则返回空集合</returns>
    //    private static IEnumerable<Claim> GetClaimsIdentity(string token)
    //    {
    //        var jwtHandler = new JwtSecurityTokenHandler();
    //        // token校验
    //        if (token.IsNotEmptyOrNull() && jwtHandler.CanReadToken(token))
    //        {
    //            var jwtToken = jwtHandler.ReadJwtToken(token);

    //            return jwtToken.Claims;
    //        }

    //        return new List<Claim>();
    //    }

    //    /// <summary>
    //    /// 从当前HTTP上下文中获取成功的Swagger JWT令牌
    //    /// </summary>
    //    /// <param name="context">HTTP上下文</param>
    //    /// <returns>Swagger JWT令牌字符串</returns>
    //    public static string GetSuccessSwaggerJwt(this HttpContext context)
    //    {
    //        return context.GetSession().GetString(SwaggerJwt);
    //    }


    //    /// <summary>
    //    /// 重定向到Swagger登录页面
    //    /// </summary>
    //    /// <param name="context">Http上下文对象</param>
    //    /// <remarks>
    //    /// 将当前请求重定向到/swg-login.html页面，并附带当前URL作为返回地址参数
    //    /// </remarks>
    //    public static void RedirectSwaggerLogin(this HttpContext context)
    //    {
    //        var returnUrl = context.Request.GetDisplayUrl(); //获取当前url地址 
    //        context.Response.Redirect("/swg-login.html?returnUrl=" + returnUrl);
    //    }
    //}
}
