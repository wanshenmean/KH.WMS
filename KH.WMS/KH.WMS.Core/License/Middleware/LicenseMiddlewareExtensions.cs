using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;

namespace KH.WMS.Core.License.Middleware
{
    /// <summary>
    /// License 中间件扩展方法
    /// </summary>
    public static class LicenseMiddlewareExtensions
    {
        /// <summary>
        /// 启用 License 验证中间件
        /// 应在 UseCustomExceptionHandler 之后、UseAuthorization 之前调用
        /// </summary>
        public static IApplicationBuilder UseLicenseValidation(this IApplicationBuilder app)
        {
            return app.UseMiddleware<LicenseValidationMiddleware>();
        }
    }
}
