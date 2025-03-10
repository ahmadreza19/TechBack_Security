using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
namespace TechBack_security.Models.Service
{
    public class GoogleRecaptcha
    {
        // متغیر readonly برای ذخیره کلید امنیتی (Secret Key) که فقط هنگام مقداردهی اولیه قابل تنظیم است
        private readonly string _secretKey;

        /// <summary>
        /// سازنده کلاس که مقدار SecretKey را از appsettings.json دریافت می‌کند.
        /// </summary>
        /// <param name="configuration">اینترفیس IConfiguration برای خواندن تنظیمات</param>
        public GoogleRecaptcha(IConfiguration configuration)
        {
            // مقداردهی متغیر _secretKey از فایل تنظیمات
            _secretKey = configuration["GoogleRecaptcha:SecretKey"];
        }

        /// <summary>
        /// متد بررسی اعتبار ReCaptcha با ارسال درخواست به سرور گوگل
        /// </summary>
        /// <param name="googleResponse">کدی که کاربر پس از حل ReCaptcha دریافت می‌کند</param>
        /// <returns>برمی‌گرداند true اگر کپچا معتبر باشد، در غیر این صورت false</returns>
        public async Task<bool> VerifyAsync(string googleResponse)
        {
            using (HttpClient httpClient = new HttpClient()) // ایجاد HttpClient برای ارسال درخواست HTTP
            {
                // ارسال درخواست POST به API گوگل برای بررسی صحت کپچا
                var response = await httpClient.PostAsync(
                    $"https://www.google.com/recaptcha/api/siteverify?secret={_secretKey}&response={googleResponse}",
                    null);

                // اگر درخواست موفقیت‌آمیز نبود، مقدار false برگردان
                if (!response.IsSuccessStatusCode)
                {
                    return false;
                }

                // خواندن پاسخ API به‌صورت متنی
                string content = await response.Content.ReadAsStringAsync();

                // تبدیل متن JSON پاسخ گوگل به یک آبجکت داینامیک
                dynamic jsonData = JObject.Parse(content);

                // بررسی مقدار success در پاسخ دریافتی (true یعنی معتبر، false یعنی نامعتبر)
                return jsonData.success == true;
            }
        }
    }

}
