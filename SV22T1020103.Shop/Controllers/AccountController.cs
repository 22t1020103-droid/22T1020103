using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SV22T1020103.BusinessLayers;
using SV22T1020103.Models.Partner;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Threading.Tasks;

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
        /// Xử lý đăng nhập và lưu Cookie Authentication (Thay thế Session)
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ Email và Mật khẩu!";
                return View();
            }

            var user = UserAccountService.Authorize(email, password);
            if (user != null)
            {
                // 1. Tạo danh sách định danh (Claims)
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.CustomerID.ToString()),
                    new Claim(ClaimTypes.Name, user.CustomerName),
                    new Claim(ClaimTypes.Email, user.Email ?? "")
                };

                // 2. Tạo Identity
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                // 3. Cấu hình thuộc tính Cookie (Ghi nhớ đăng nhập)
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true, // Lưu cookie kể cả khi đóng trình duyệt
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30)
                };

                // 4. Thực hiện đăng nhập bằng Cookie
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                // Giữ lại Session nếu các phần khác trong web vẫn đang gọi tới Session
                HttpContext.Session.SetString("UserId", user.CustomerID.ToString());
                HttpContext.Session.SetString("CustomerName", user.CustomerName);

                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Email hoặc mật khẩu không chính xác!";
            return View();
        }

        /// <summary>
        /// Xử lý đăng xuất, xóa cả Session và Cookie Authentication
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            // 1. Xóa sạch Session trên Server
            HttpContext.Session.Clear();

            // 2. Đăng xuất khỏi cơ chế Cookie Authentication
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // 3. Xóa Cookie định danh cũ (Nếu có)
            Response.Cookies.Delete(".ShopModern.Session");

            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// Hiển thị thông tin cá nhân (Lấy từ Cookie/Session)
        /// </summary>
        [HttpGet]
        public IActionResult Profile()
        {
            // Thử lấy từ Claim trước (Cookie), nếu không có thì lấy từ Session
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier)
                        ?? HttpContext.Session.GetString("UserId");

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
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier)
                        ?? HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(idStr)) return RedirectToAction("Login");

            data.CustomerID = int.Parse(idStr);
            if (string.IsNullOrEmpty(data.CustomerName))
            {
                ViewBag.Error = "Tên không được để trống";
                return View(data);
            }

            UserAccountService.Update(data);

            // Cập nhật lại Session nếu cần
            HttpContext.Session.SetString("CustomerName", data.CustomerName);

            ViewBag.Message = "Cập nhật thông tin thành công!";
            return View("Profile", data);
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
            catch (Exception ex)
            {
                ViewBag.Error = "Lỗi đăng ký: " + ex.Message;
                return View(data);
            }
        }
    }
}