using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SV22T1020103.BusinessLayers;
using SV22T1020103.Models.Partner;
using System;

namespace SV22T1020103.Shop.Controllers
{
    /// <summary>
    /// Controller quản lý các chức năng tài khoản của khách hàng
    /// </summary>
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public class AccountController : Controller
    {
        /// <summary>
        /// Giao diện đăng nhập
        /// </summary>
        [HttpGet]
        public IActionResult Login(string email = "")
        {
            ViewBag.RegisteredEmail = email;
            return View();
        }

        /// <summary>
        /// Xử lý đăng nhập và lưu Session
        /// </summary>
        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ Email và Mật khẩu!";
                return View();
            }

            var user = UserAccountService.Authorize(email, password);
            if (user != null)
            {
                HttpContext.Session.SetString("UserId", user.CustomerID.ToString());
                HttpContext.Session.SetString("CustomerName", user.CustomerName);
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Email hoặc mật khẩu không chính xác!";
            return View();
        }

        /// <summary>
        /// Xử lý đăng xuất, xóa Session và Cookie
        /// </summary>
        [HttpGet]
        public IActionResult Logout()
        {
            // 1. Xóa sạch Session trên Server
            HttpContext.Session.Clear();

            // 2. Xóa Cookie trên trình duyệt (Khớp tên với Program.cs)
            Response.Cookies.Delete(".ShopModern.Session");

            // 3. Quay về Home
            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// Hiển thị thông tin cá nhân của khách hàng
        /// </summary>
        [HttpGet]
        public IActionResult Profile()
        {
            var idStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(idStr)) return RedirectToAction("Login");
            var model = UserAccountService.GetUser(int.Parse(idStr));
            return View(model);
        }

        /// <summary>
        /// Cập nhật thông tin cá nhân
        /// </summary>
        [HttpPost]
        public IActionResult UpdateProfile(Customer data)
        {
            var idStr = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(idStr)) return RedirectToAction("Login");

            data.CustomerID = int.Parse(idStr);
            if (string.IsNullOrEmpty(data.CustomerName))
            {
                ViewBag.Error = "Tên không được để trống";
                return View(data);
            }
            UserAccountService.Update(data);
            HttpContext.Session.SetString("CustomerName", data.CustomerName);
            return RedirectToAction("Profile");
        }

        /// <summary>
        /// Giao diện đăng ký khách hàng mới
        /// </summary>
        [HttpGet]
        public IActionResult Register() => View();

        /// <summary>
        /// Xử lý đăng ký tài khoản mới
        /// </summary>
        [HttpPost]
        public IActionResult Register(Customer data)
        {
            try
            {
                UserAccountService.RegisterCustomer(data);
                return RedirectToAction("Login", new { email = data.Email });
            }
            catch
            {
                ViewBag.Error = "Lỗi đăng ký!";
                return View(data);
            }
        }
    }
}