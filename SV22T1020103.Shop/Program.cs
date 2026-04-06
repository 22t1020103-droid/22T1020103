using Microsoft.AspNetCore.Authentication.Cookies; // Thêm namespace này
using SV22T1020103.BusinessLayers;

var builder = WebApplication.CreateBuilder(args);

// Load cấu hình
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// --- 1. CẤU HÌNH SERVICES ---
builder.Services.AddControllersWithViews();

// Thay thế AddSession bằng AddAuthentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = ".ShopModern.Auth"; // Tên cookie mới
        options.LoginPath = "/Account/Login";     // Đường dẫn trang đăng nhập
        options.LogoutPath = "/Account/Logout";
        options.ExpireTimeSpan = TimeSpan.FromDays(30); // Lưu đăng nhập 30 ngày
        options.SlidingExpiration = true;
    });

// (Tùy chọn) Bạn có thể giữ lại Session nếu vẫn muốn dùng cho Giỏ hàng, 
// nhưng không dùng để lưu UserId/Login nữa.
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddHttpContextAccessor();

// --- 2. KHỞI TẠO CONNECTION ---
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (!string.IsNullOrEmpty(connectionString))
{
    Configuration.Initialize(connectionString);
    UserAccountService.Initialize(connectionString);
    ProductService.Initialize(connectionString);
    CatalogDataService.Initialize(connectionString);
}

var app = builder.Build();

// --- 3. CẤU HÌNH MIDDLEWARE (THỨ TỰ QUAN TRỌNG) ---
app.UseStaticFiles();
app.UseRouting();

// THÊM UseAuthentication vào ĐÂY (trước Authorization)
app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();