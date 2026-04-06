using SV22T1020103.BusinessLayers;

var builder = WebApplication.CreateBuilder(args);

// Load cấu hình
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// 1. Cấu hình Services
builder.Services.AddControllersWithViews();
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    // Đặt tên Cookie cố định để dễ dàng xóa khi Logout
    options.Cookie.Name = ".ShopModern.Session";
});

builder.Services.AddHttpContextAccessor();

// 2. Khởi tạo Connection và Services
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (!string.IsNullOrEmpty(connectionString))
{
    Configuration.Initialize(connectionString);
    UserAccountService.Initialize(connectionString);
    ProductService.Initialize(connectionString);
    CatalogDataService.Initialize(connectionString);
}

var app = builder.Build();

// 3. Cấu hình Middleware (Thứ tự là bắt buộc)
app.UseStaticFiles();
app.UseRouting();

app.UseSession(); // Nằm sau UseRouting và trước UseAuthorization
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();