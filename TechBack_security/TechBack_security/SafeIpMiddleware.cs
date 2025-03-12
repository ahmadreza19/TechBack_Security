using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Threading.Tasks;

namespace TechBack_security
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class SafeIpMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string _SafeIpAddress;

        /// <summary>
        /// سازنده‌ی میان‌افزار برای فیلتر کردن آی‌پی‌های غیرمجاز
        /// </summary>
        /// <param name="next">درخواست بعدی در خط پردازش HTTP</param>
        /// <param name="SafeIpAddress">لیست آی‌پی‌های مجاز که از تنظیمات خوانده می‌شود</param>
        public SafeIpMiddleware(RequestDelegate next, string SafeIpAddress)
        {
            _SafeIpAddress = SafeIpAddress;
            _next = next;
        }

        /// <summary>
        /// متد اصلی میان‌افزار که درخواست‌های ورودی را بررسی می‌کند
        /// </summary>
        public async Task Invoke(HttpContext httpContext)
        {
            var UserIp = httpContext.Connection.RemoteIpAddress;

            // بررسی اینکه مقدار RemoteIpAddress نال نباشد
            if (UserIp == null)
            {
                httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
                await httpContext.Response.WriteAsync("Access Denied");
                return;
            }

            // بررسی مقدار تنظیم‌شده برای آی‌پی‌های مجاز
            if (string.IsNullOrEmpty(_SafeIpAddress))
            {
                await httpContext.Response.WriteAsync("Configuration Error: SafeIpAddress is missing.");
                return;
            }

            // تبدیل لیست آی‌پی‌های مجاز به آرایه
            string[] safeIp = _SafeIpAddress.Split(";");

            // بررسی آیا آی‌پی کاربر در لیست مجاز است یا نه
            var userIpBytes = UserIp.GetAddressBytes();
            bool isBlocked = true;

            foreach (var item in safeIp)
            {
                if (IPAddress.TryParse(item, out var allowedIp))
                {
                    if (allowedIp.GetAddressBytes().SequenceEqual(userIpBytes))
                    {
                        isBlocked = false;
                        break;
                    }
                }
            }

            // اگر آی‌پی مجاز نباشد، درخواست را مسدود کن
            if (isBlocked)
            {
                httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
                await httpContext.Response.WriteAsync("Access Denied");
                return;
            }

            // ادامه‌ی پردازش درخواست در صورتی که آی‌پی مجاز باشد
            await _next(httpContext);
        }
    }

    /// <summary>
    /// متد الحاقی برای افزودن این میان‌افزار به خط پردازش درخواست‌ها
    /// </summary>
    public static class SafeIpMiddlewareExtensions
    {
        /// <summary>
        /// افزودن میان‌افزار بررسی آی‌پی به خط پردازش
        /// </summary>
        /// <param name="builder">ساختار اپلیکیشن</param>
        /// <returns>برمی‌گرداند builder با میان‌افزار اضافه‌شده</returns>
        public static IApplicationBuilder UseSafeIpMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SafeIpMiddleware>();
        }
    }
}
