using Microsoft.AspNetCore.Mvc;

namespace WNC15.Controllers
{
    // Thường sử dụng [Authorize] để đảm bảo chỉ người dùng đã đăng nhập mới truy cập được
    public class ProfileController : Controller
    {
        private readonly ILogger<ProfileController> _logger;

        public ProfileController(ILogger<ProfileController> logger)
        {
            _logger = logger;
        }

        // GET: /Profile
        // GET: /Profile/Index
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // TODO: Nghiệp vụ P04 - Xem thông tin hồ sơ cá nhân:
            // - Lấy thông tin tài khoản hiện tại từ Claims/Session/Database
            // - Hiển thị trạng thái duyệt tài khoản (cho_duyet, hoat_dong, tu_choi)
            // - Hiển thị lý do từ chối nếu trạng thái là "tu_choi"
            await Task.CompletedTask;
            return View();
        }

        // GET: /Profile/Edit
        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            // TODO: Nghiệp vụ P04 - Lấy dữ liệu hồ sơ hiện tại để nạp vào form chỉnh sửa
            await Task.CompletedTask;
            return View();
        }

        // POST: /Profile/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string fullName, string? phoneNumber, string? address, string? introduction, IFormFile? avatar)
        {
            if (string.IsNullOrWhiteSpace(fullName))
            {
                ModelState.AddModelError("fullName", "Họ và tên không được để trống.");
                return View();
            }

            // TODO: Nghiệp vụ P04 - Cập nhật thông tin cá nhân:
            // - Xử lý upload ảnh đại diện (avatar) nếu có
            // - Cập nhật số điện thoại, địa chỉ, lời giới thiệu vào Database
            await Task.CompletedTask;

            TempData["SuccessMessage"] = "Cập nhật hồ sơ cá nhân thành công!";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Profile/ChangePassword
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        // POST: /Profile/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmNewPassword)
        {
            if (string.IsNullOrWhiteSpace(currentPassword) || string.IsNullOrWhiteSpace(newPassword))
            {
                ModelState.AddModelError(string.Empty, "Vui lòng nhập đầy đủ mật khẩu hiện tại và mật khẩu mới.");
                return View();
            }

            if (newPassword != confirmNewPassword)
            {
                ModelState.AddModelError("confirmNewPassword", "Mật khẩu xác nhận không trùng khớp.");
                return View();
            }

            // TODO: Nghiệp vụ P04 - Đổi mật khẩu cá nhân:
            // 1. So khớp mật khẩu cũ với hash trong CSDL bằng BCrypt.Verify
            // 2. Hash mật khẩu mới bằng BCrypt.HashPassword và cập nhật vào CSDL
            await Task.CompletedTask;

            TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Profile/MyEvents
        [HttpGet]
        public async Task<IActionResult> MyEvents()
        {
            // TODO: Nghiệp vụ P04 - Xem danh sách các sự kiện bản thân đã đăng ký tham gia (kèm trạng thái tham dự)
            await Task.CompletedTask;
            return View();
        }
    }
}
