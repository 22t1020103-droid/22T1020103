using Microsoft.AspNetCore.Authentication.Cookies;
using SV22T1020103.Admin;
using System.Globalization;
using SV22T1020103.BusinessLayers;
using System.IO; // thêm để debug path

var builder = WebApplication.CreateBuilder(args);

// =======================
// FIX: đảm bảo load file config và debug
// =======================
builder.Configuration.SetBasePath(Directory.GetCurrentDirectory()) // đảm bảo đúng path
                     .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// 1. Cấu hình Services (giữ nguyên)
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllersWithViews()
                .AddMvcOptions(option => {
                    option.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
                });

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(option => {
                    option.Cookie.Name = "SV22T1020103.Admin";
                    option.LoginPath = "/Account/Login";
                    option.ExpireTimeSpan = TimeSpan.FromDays(7);
                });

builder.Services.AddSession(option => {
    option.IdleTimeout = TimeSpan.FromHours(2);
    option.Cookie.HttpOnly = true;
    option.Cookie.IsEssential = true;
});

// Chốt builder tại đây
var app = builder.Build();

// 2. Cấu hình Middleware (giữ nguyên)
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// 3. Cấu hình Culture & Context (giữ nguyên)
var cultureInfo = new CultureInfo("vi-VN");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

// QUAN TRỌNG: Sử dụng app.Services và app.Configuration (giữ nguyên)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    ApplicationContext.Configure(
        httpContextAccessor: services.GetRequiredService<IHttpContextAccessor>(),
        webHostEnvironment: services.GetRequiredService<IWebHostEnvironment>(),
        configuration: app.Configuration
    );
}

// =======================
// 4. KHỞI TẠO DỮ LIỆU (FIX chính)
// =======================

// DEBUG: kiểm tra file config và connection string
Console.WriteLine(">>> Config file path: " + Path.GetFullPath("appsettings.json"));
Console.WriteLine(">>> DefaultConnection: " + builder.Configuration.GetConnectionString("DefaultConnection"));

string connectionString = app.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Không tìm thấy chuỗi kết nối 'DefaultConnection' trong appsettings.json."
    );

// *** Thêm dòng này để gán ConnectionString cho Configuration tĩnh của BusinessLayers ***
Configuration.Initialize(connectionString);

// Nạp cho mảng Catalog (Loại hàng, Mặt hàng...)
SV22T1020103.BusinessLayers.CatalogDataService.Initialize(connectionString);

// Nạp cho mảng Partner (Khách hàng, Nhà cung cấp, Người giao hàng)
SV22T1020103.BusinessLayers.PartnerDataService.Initialize(connectionString);

// Nạp cho mảng HR (Nhân viên)
SV22T1020103.BusinessLayers.HRDataService.Initialize(connectionString);

// Nạp cho mảng Sales (Đơn hàng)
SV22T1020103.BusinessLayers.SalesDataService.Initialize(connectionString);

// Nạp cho mảng Account (Tài khoản/Đăng nhập)
SV22T1020103.BusinessLayers.UserAccountService.Initialize(connectionString);

// Thêm dòng này để nạp ConnectionString cho DictionaryDataService (Tỉnh thành, loại hàng...)
SV22T1020103.BusinessLayers.DictionaryDataService.Initialize(connectionString);

app.Run();