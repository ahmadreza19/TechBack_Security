using System;
using System.Diagnostics;
using System.Xml.Linq;
using Ganss.Xss;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;
using TechBack_security.Data;
using TechBack_security.Models;
using TechBack_security.Models.Entity;

namespace TechBack_security.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private IConfiguration _configuration;
        private readonly DataBaseContext _context;
        public HomeController(ILogger<HomeController> logger, IConfiguration configuration, DataBaseContext context)
        {
            _logger = logger;
            _configuration = configuration;
            _context = context;
        }
        //[ValidateAntiForgeryToken]
        //این یک ویژگی(attribute) در ASP.NET است که برای محافظت از برنامه‌های وب در برابر حملات
        //Cross-Site Request Forgery (CSRF یا XSRF) استفاده می‌شود.

        //عملکرد:
        //- این ویژگی زمانی که روی یک متد کنترلر قرار می‌گیرد، بررسی می‌کند که درخواست‌های ارسالی
        // به آن متد حاوی یک توکن معتبر Anti-Forgery باشند
        //- این توکن باید با هلپر @Html.AntiForgeryToken() در فرم مربوطه ایجاد شده باشد
        //- اگر توکن وجود نداشته باشد یا نامعتبر باشد، درخواست رد می‌شود

        public IActionResult Index()
        {
            // ایجاد یک شیء CookieOptions برای تنظیم کوکی‌ها با ویژگی‌های امنیتی
            CookieOptions options = new CookieOptions()
            {
                Expires = DateTime.Now.AddDays(10), // تعیین زمان انقضای کوکی برای 10 روز بعد
                HttpOnly = true // جلوگیری از دسترسی کوکی از طریق جاوا اسکریپت (محافظت در برابر XSS)
            };

            // اضافه کردن کوکی به پاسخ HTTP
            Response.Cookies.Append("MyCookie", "this is a test value");

            // ایجاد یک شیء HtmlSanitizer برای پاکسازی ورودی‌های کاربر از کدهای مخرب
            var sanitizer = new HtmlSanitizer();

            // دریافت لیست نظرات از دیتابیس و مرتب‌سازی بر اساس شناسه به صورت نزولی
            var result = _context.comments.OrderByDescending(p => p.Id).ToList()
                .Select(p => new Comment
                {
                    Body = sanitizer.Sanitize(p.Body), // پاکسازی محتوای ورودی برای جلوگیری از حملات XSS
                    Id = p.Id,
                    Name = p.Name
                });

            // ارسال داده‌های پردازش شده به ویو
            return View(result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // جلوگیری از حملات CSRF
        public IActionResult SendComment(Comment comment)
        {
            // ایجاد یک شیء HtmlSanitizer برای پاکسازی محتوای ورودی
            var sanitizer = new HtmlSanitizer();

            // پاکسازی محتوای نظر برای جلوگیری از تزریق کدهای مخرب XSS
            var result = sanitizer.Sanitize(comment.Body);
            comment.Body = result;

            // افزودن نظر پاکسازی‌شده به پایگاه داده
            _context.comments.Add(comment);
            _context.SaveChanges();

            // بازگشت به صفحه اصلی پس از ثبت نظر
            return RedirectToAction("Index");
        }


        [ValidateAntiForgeryToken]
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
