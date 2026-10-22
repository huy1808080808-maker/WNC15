using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using WNC15.Data;

var builder = WebApplication.CreateBuilder(args);

// 1. Cấu hình kết nối SQL Server với Entity Framework Core
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// 2. Cấu hình Xác thực Authentication bằng Cookie (cho Đăng nhập/Đăng xuất/Phân quyền)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/DangNhap";
        options.LogoutPath = "/Auth/DangXuat";
        options.AccessDeniedPath = "/Auth/TuChoiTruyCap";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
    // Việt hóa các thông báo lỗi Validation mặc định của ASP.NET Core
    options.ModelBindingMessageProvider.SetValueMustNotBeNullAccessor(
        _ => "Trường thông tin này không được để trống.");
    options.ModelBindingMessageProvider.SetMissingKeyOrValueAccessor(
        () => "Trường thông tin này là bắt buộc.");
    options.ModelBindingMessageProvider.SetMissingBindRequiredValueAccessor(
        _ => "Trường thông tin này là bắt buộc.");
    options.ModelBindingMessageProvider.SetAttemptedValueIsInvalidAccessor(
        (val, field) => $"Giá trị '{val}' không hợp lệ.");
    options.ModelBindingMessageProvider.SetUnknownValueIsInvalidAccessor(
        _ => "Giá trị nhập vào không hợp lệ.");
    options.ModelBindingMessageProvider.SetValueIsInvalidAccessor(
        _ => "Giá trị nhập vào không hợp lệ.");
    options.ModelBindingMessageProvider.SetValueMustBeANumberAccessor(
        _ => "Trường này phải là một số hợp lệ.");
    options.ModelBindingMessageProvider.SetNonPropertyAttemptedValueIsInvalidAccessor(
        val => $"Giá trị '{val}' không hợp lệ.");
    options.ModelBindingMessageProvider.SetNonPropertyUnknownValueIsInvalidAccessor(
        () => "Giá trị không hợp lệ.");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Thứ tự quan trọng: Authentication -> Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
