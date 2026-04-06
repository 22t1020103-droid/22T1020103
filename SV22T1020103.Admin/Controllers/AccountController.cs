using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020103.Admin.Models;
using SV22T1020103.BusinessLayers;

namespace SV22T1020103.Admin.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View(new AccountChangePasswordViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(AccountChangePasswordViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.OldPassword))
                ModelState.AddModelError(nameof(model.OldPassword), "Vui lòng nhập mật khẩu cũ");
            if (string.IsNullOrWhiteSpace(model.NewPassword))
                ModelState.AddModelError(nameof(model.NewPassword), "Vui lòng nhập mật khẩu mới");
            if (model.NewPassword != model.ConfirmPassword)
                ModelState.AddModelError(nameof(model.ConfirmPassword), "Xác nhận mật khẩu không khớp");

            var userData = User.GetUserData();
            if (userData == null || string.IsNullOrWhiteSpace(userData.UserId) || !int.TryParse(userData.UserId, out int employeeId))
                return RedirectToAction(nameof(Login));

            if (!ModelState.IsValid)
                return View(model);

            var oldHash = CryptHelper.HashMD5(model.OldPassword);
            var newHash = CryptHelper.HashMD5(model.NewPassword);

            bool ok = await HRDataService.ChangeEmployeePasswordAsync(employeeId, oldHash, newHash);
            if (!ok)
            {
                ModelState.AddModelError("Error", "Mật khẩu cũ không đúng hoặc tài khoản không tồn tại");
                return View(model);
            }

            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync();
            TempData["Message"] = "Đổi mật khẩu thành công. Vui lòng đăng nhập lại.";
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string returnUrl = "")
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string username, string password, string returnUrl = "")
        {
            ViewBag.Username = username;
            ViewBag.ReturnUrl = returnUrl;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("Error", "Nhập đủ tên và mật khẩu");
                return View();
            }

            // Mã hóa mật khẩu MD5 trước khi so khớp với DB
            var passwordHash = CryptHelper.HashMD5(password);

            // SỬA LỖI: Gọi đúng tên class SecurityDataService
            var userAccount = await SecurityDataService.EmployeeAuthorizeAsync(username, passwordHash);

            if (userAccount == null)
            {
                ModelState.AddModelError("Error", "Đăng nhập thất bại. Sai tên đăng nhập hoặc mật khẩu.");
                return View();
            }

            var userData = new WebUserData()
            {
                UserId = userAccount.UserId,
                UserName = userAccount.UserName,
                DisplayName = userAccount.DisplayName,
                Email = userAccount.Email,
                Photo = userAccount.Photo,
                Roles = (userAccount.RoleNames ?? "")
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .ToList()
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                userData.CreatePrincipal()
            );

            // Nếu có trang cũ đang đợi thì quay lại, không thì về trang chủ
            if (string.IsNullOrWhiteSpace(returnUrl))
                return RedirectToAction("Index", "Home");

            return Redirect(returnUrl);
        }

        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync();
            return RedirectToAction("Login");
        }

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}