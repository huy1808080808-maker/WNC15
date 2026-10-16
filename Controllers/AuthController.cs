using Microsoft.AspNetCore.Mvc;

namespace WNC15.Controllers
{
    public class AuthController : Controller
    {
        private readonly ILogger<AuthController> _logger;

        public AuthController(ILogger<AuthController> logger)
        {
            _logger = logger;
        }

        // GET: /Auth/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: /Auth/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string emailOrUsername, string password, bool rememberMe, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (string.IsNullOrWhiteSpace(emailOrUsername) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError(string.Empty, "Vui lòng nhập đầy đủ tên đăng nhập/email và mật khẩu.");
                return View();
            }

            // TODO: Nghiệp vụ P01 - Kiểm tra xác thực và phân quyền tài khoản (Admin / Hội viên)
            // 1. Kiểm tra tài khoản có tồn tại, so khớp mật khẩu mã hóa BCrypt
            // 2. Kiểm tra trạng thái tài khoản:
            //    - "cho_duyet": Báo thông báo hồ sơ đang chờ xét duyệt
            //    - "khoa" / "tu_choi": Thông báo tài khoản bị khóa hoặc từ chối
            //    - "hoat_dong": Cho phép đăng nhập và thiết lập Claims/Cookie (Role: Admin / HoiVien)
            await Task.CompletedTask;

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        // GET: /Auth/Register
        [HttpGet]
        public IActionResult Register()
        {
            // Hiển thị biểu mẫu đăng ký gia nhập Hiệp hội (BM01)
            return View();
        }

        // POST: /Auth/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(string username, string fullName, string email, string phoneNumber, string password, string confirmPassword, string? address, string? introduction)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError(string.Empty, "Vui lòng điền đầy đủ các thông tin bắt buộc theo biểu mẫu BM01.");
                return View();
            }

            if (password != confirmPassword)
            {
                ModelState.AddModelError("confirmPassword", "Mật khẩu xác nhận không khớp.");
                return View();
            }

            // TODO: Nghiệp vụ P01 - Đăng ký tài khoản hội viên mới (nộp biểu mẫu BM01):
            // - Mã hóa mật khẩu bằng BCrypt
            // - Gán vai trò mặc định là "HoiVien"
            // - Thiết lập trạng thái ban đầu: "cho_duyet"
            // - Lưu thông tin đăng ký vào cơ sở dữ liệu
            await Task.CompletedTask;

            TempData["SuccessMessage"] = "Nộp hồ sơ đăng ký gia nhập Hiệp hội (BM01) thành công! Hồ sơ của bạn đang ở trạng thái chờ duyệt.";
            return RedirectToAction(nameof(Login));
        }

        // POST: /Auth/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            // TODO: Đăng xuất người dùng khỏi phiên làm việc (SignOutAsync / Clear Session)
            await Task.CompletedTask;
            return RedirectToAction("Index", "Home");
        }

        // GET: /Auth/AccessDenied
        [HttpGet]
        public IActionResult AccessDenied()
        {
            // Trang thông báo từ chối truy cập khi tài khoản không đủ quyền (Ví dụ: Hội viên cố vào trang Admin)
            return View();
        }
    }
}
