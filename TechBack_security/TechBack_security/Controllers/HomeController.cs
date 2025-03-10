using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Xml.Linq;
using Elfie.Serialization;
using Ganss.Xss;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;
using TechBack_security.Data;
using TechBack_security.Models;
using TechBack_security.Models.Entity;
using TechBack_security.Models.Service;

namespace TechBack_security.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private IConfiguration _configuration;
        private readonly DataBaseContext _context; 
        private readonly GoogleRecaptcha _googleRecaptcha;
        public HomeController(ILogger<HomeController> logger, IConfiguration configuration, DataBaseContext context)
        {
            _logger = logger;
            _configuration = configuration;
            _context = context;
            _googleRecaptcha = new GoogleRecaptcha(configuration);
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
        [HttpGet]
        public IActionResult Regaster()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Regaster(string UserName, string Password)
        {
            string googleResponse = HttpContext.Request.Form["g-Recaptcha-Response"];

            if (!_googleRecaptcha.VerifyAsync(googleResponse).Result)
            {
                ViewBag.Message = "لطفا بر روی دکمه من ربات نیستم کلیک نمایید";
                return View();
            }

            // لیست کلمات و کاراکترهای مشکوک به SQL Injection برای جلوگیری از حملات امنیتی
            List<string> blackList = new List<string>()
    {
        "--", "or", "=", "and", ";", "/*", "*/", "@@", "@", "char", "nchar",
        "varchar", "nvarchar", "alter", "begin", "cast", "create", "cursor",
        "declare", "delete", "drop", "end", "exec", "execute", "fetch",
        "insert", "kill", "open", "select", "sys", "sysobjects", "syscolumns",
        "table", "update",
    };

            // بررسی وجود عبارات مشکوک در رمز عبور (جلوگیری از SQL Injection)
            var passwordCheck = blackList.FirstOrDefault(p => Password.ToUpper().Contains(p.ToUpper()));
            if (passwordCheck != null)
            {
                ViewBag.Message = "احتمال هک شدن وجود دارد";
                return View();
            }

            // بررسی وجود عبارات مشکوک در نام کاربری (جلوگیری از SQL Injection)
            var usernameCheck = blackList.FirstOrDefault(p => UserName.ToUpper().Contains(p.ToUpper()));
            if (usernameCheck != null)
            {
                ViewBag.Message = "احتمال هک شدن وجود دارد";
                return View();
            }

            // ایجاد ارتباط با پایگاه داده
            SqlConnection connection = new SqlConnection("Data Source=.; Initial Catalog=DbSecurity; Integrated Security=True; TrustServerCertificate=Yes");
            connection.Open();

            // اجرای کوئری برای بررسی ورود کاربر
            SqlCommand command = new SqlCommand($"SELECT * FROM Users WHERE UserName='{UserName}' AND Password='{Password}'", connection);

            // اجرای کوئری و دریافت نتیجه
            var result = command.ExecuteReader();

            if (result.Read()) // اگر کاربری با این مشخصات یافت شد
            {
                string name = result["UserName"].ToString();
                ViewBag.Message = $"سلام {name}  ورود شما با موفقیت انجام شد";
                return View();
            }
            else
            {
                ViewBag.Message = "ورود ناموفق";
                return View();
            }
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
