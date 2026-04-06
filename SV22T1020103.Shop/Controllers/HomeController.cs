using Microsoft.AspNetCore.Mvc;
using SV22T1020103.Shop.Models;
using System.Diagnostics;

namespace SV22T1020103.Shop.Controllers
{
    /// <summary>
    /// Controller điều hướng các trang cơ bản của hệ thống như Trang chủ, Chính sách và xử lý lỗi.
    /// </summary>
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        /// <summary>
        /// Khởi tạo HomeController với dịch vụ Logger để ghi lại nhật ký hoạt động.
        /// </summary>
        /// <param name="logger">Dịch vụ ghi Log hệ thống.</param>
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Hiển thị trang chủ (Index) của website Shop.
        /// </summary>
        /// <returns>View trang chủ.</returns>
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Hiển thị trang các chính sách bảo mật và điều khoản của cửa hàng.
        /// </summary>
        /// <returns>View trang chính sách.</returns>
        public IActionResult Privacy()
        {
            return View();
        }

        /// <summary>
        /// Xử lý và hiển thị thông tin lỗi khi hệ thống xảy ra sự cố.
        /// Cấu hình ResponseCache để đảm bảo trang lỗi không bị lưu lại trong bộ nhớ đệm.
        /// </summary>
        /// <returns>View hiển thị thông tin lỗi kèm theo Request ID.</returns>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}