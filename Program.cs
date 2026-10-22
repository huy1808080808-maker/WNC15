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

// Tự động chuẩn hóa dữ liệu tiếng Việt có dấu trong CSDL mỗi khi ứng dụng chạy
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        // 1. Chuẩn hóa tài khoản Quản trị viên
        var admin = context.NguoiDungs.FirstOrDefault(u => u.Email == "admin@hiephoi.vn");
        if (admin != null)
        {
            admin.HoTen = "Quản trị viên Hiệp hội";
            admin.DonViCongTac = "Ban Chấp Hành Hiệp Hội";
            admin.ChucDanh = "Quản trị hệ thống";
            admin.GioiThieu = "Tài khoản quản trị hệ thống Hiệp hội Khoa học & Công nghệ.";
            admin.MatKhauMaHoa = "123456";
            admin.VaiTro = "quan_tri";
            admin.TrangThai = "hoat_dong";
        }

        // 2. Chuẩn hóa tài khoản Trần Thị Mai
        var mai = context.NguoiDungs.FirstOrDefault(u => u.Email == "mai.tran@example.com");
        if (mai != null)
        {
            mai.HoTen = "Trần Thị Mai";
            mai.DonViCongTac = "Trường Đại học Bách Khoa";
            mai.ChucDanh = "Tiến sĩ / Giảng viên";
            mai.LinhVuc = "Công nghệ thông tin & Trí tuệ nhân tạo";
            mai.GioiThieu = "Mong muốn kết nối và chia sẻ tri thức với cộng đồng nghề nghiệp.";
            mai.MatKhauMaHoa = "123456";
            mai.TrangThai = "hoat_dong";
        }

        // 3. Chuẩn hóa tài khoản Phạm Văn Nam
        var nam = context.NguoiDungs.FirstOrDefault(u => u.Email == "nam.pham@example.com");
        if (nam != null)
        {
            nam.HoTen = "Phạm Văn Nam";
            nam.DonViCongTac = "Viện Nghiên cứu Điện tử & Tin học";
            nam.ChucDanh = "Nghiên cứu viên chính";
            nam.LinhVuc = "Hệ thống nhúng & IoT";
            nam.GioiThieu = "Mong muốn tham gia để học hỏi kinh nghiệm và đóng góp cho Hiệp hội.";
            nam.MatKhauMaHoa = "123456";
            nam.TrangThai = "cho_duyet";
        }

        // 4. Chuẩn hóa danh mục bài viết
        var dm1 = context.DanhMucs.Find(1);
        if (dm1 != null) { dm1.TenDanhMuc = "Nghiên cứu & Công nghệ"; dm1.MoTa = "Các công trình và xu hướng công nghệ mới"; }
        var dm2 = context.DanhMucs.Find(2);
        if (dm2 != null) { dm2.TenDanhMuc = "Thông báo Hiệp hội"; dm2.MoTa = "Tin tức điều hành và kế hoạch hoạt động"; }
        var dm3 = context.DanhMucs.Find(3);
        if (dm3 != null) { dm3.TenDanhMuc = "Sinh hoạt Hội viên"; dm3.MoTa = "Các buổi giao lưu và hoạt động học thuật"; }

        // 5. Chuẩn hóa bài viết
        var bv1 = context.BaiViets.Find(1);
        if (bv1 != null)
        {
            bv1.TieuDe = "Khai mạc Hội thảo chuyên ngành Công nghệ thông tin năm 2026";
            bv1.TomTat = "Tóm tắt về hội thảo chuyên ngành công nghệ thông tin và khoa học dữ liệu năm 2026.";
            bv1.NoiDung = "Trong bối cảnh kỷ nguyên số bùng nổ, việc nghiên cứu và ứng dụng trí tuệ nhân tạo đã trở thành một yêu cầu thực tiễn cấp bách. Các hội viên và nhà khoa học của Hiệp hội đã không ngừng đóng góp các sáng kiến, giải pháp công nghệ nhằm hỗ trợ thiết thực cho hoạt động của cộng đồng.";
        }
        var bv2 = context.BaiViets.Find(2);
        if (bv2 != null)
        {
            bv2.TieuDe = "Thông báo kế hoạch tuyển chọn và kết nạp Hội viên đợt 1 năm 2026";
            bv2.TomTat = "Hiệp hội chính thức mở cổng tiếp nhận hồ sơ đăng ký gia nhập hội viên mới trong năm 2026.";
            bv2.NoiDung = "Ban Chấp hành Hiệp hội thông báo kế hoạch tiếp nhận và xét duyệt đơn đăng ký của các cá nhân, chuyên gia, giảng viên và nhà nghiên cứu có nguyện vọng sinh hoạt chuyên môn trong tổ chức.";
        }

        // 6. Chuẩn hóa sự kiện
        var sk1 = context.SuKiens.Find(1);
        if (sk1 != null)
        {
            sk1.TenSuKien = "Hội thảo Khoa học Quốc tế về Trí tuệ Nhân tạo & Dữ liệu lớn";
            sk1.DiaDiem = "Hội trường A, Tòa nhà Hiệp hội, Hà Nội";
            sk1.MoTa = "Hội thảo quy tụ các chuyên gia hàng đầu trao đổi về ứng dụng AI và chuyển đổi số trong thực tiễn.";
        }

        context.SaveChanges();
    }
    catch
    {
    }
}

app.Run();
