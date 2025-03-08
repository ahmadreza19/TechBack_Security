using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;
using TechBack_security.Models;

namespace TechBack_security.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private IConfiguration _configuration;
        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }
        //[ValidateAntiForgeryToken]
        //این یک ویژگی(attribute) در ASP.NET است که برای محافظت از برنامه‌های وب در برابر حملات
        //Cross-Site Request Forgery (CSRF یا XSRF) استفاده می‌شود.

        //عملکرد:
        //- این ویژگی زمانی که روی یک متد کنترلر قرار می‌گیرد، بررسی می‌کند که درخواست‌های ارسالی
        // به آن متد حاوی یک توکن معتبر Anti-Forgery باشند
        //- این توکن باید با هلپر @Html.AntiForgeryToken() در فرم مربوطه ایجاد شده باشد
        //- اگر توکن وجود نداشته باشد یا نامعتبر باشد، درخواست رد می‌شود

        [ValidateAntiForgeryToken]
        public string Index()
        {
            return _configuration["password"].ToString();
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
