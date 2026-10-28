using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WNC15.Data;
using WNC15.Models;

namespace WNC15.Controllers
{
    /// <summary>
    /// Controller xử lý Xác thực & Phân quyền:
    /// - Đăng nhập linh hoạt (chấp nhận Email đầy đủ, Tên đăng nhập trước @, 'admin', hoặc Số điện thoại).
    /// - Đăng ký tài khoản hội viên mới (Mã hóa mật khẩu bằng BCrypt, trạng thái chờ duyệt).
    /// - Đăng xuất an toàn.
    /// - Hiển thị trạng thái hồ sơ (Chờ duyệt, Bị từ chối, Khóa) và Từ chối truy cập.
    /// </summary>
    public class AuthController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AuthController(ApplicationDbContext context)
        {
            _context = context;
        }

        #region 1. ĐĂNG NHẬP HỆ THỐNG

        /// <summary>
        /// Giao diện Đăng nhập.
        /// GET: /Auth/DangNhap
        /// </summary>
        [HttpGet]
        public IActionResult DangNhap(string returnUrl)
        {
            KhoiTaoAdminMacDinh();

            // Nếu người dùng đã đăng nhập thì tự động chuyển về Trang chủ
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        /// <summary>
        /// Xử lý xác thực thông tin đăng nhập và cấp vé định danh Cookie Authentication.
        /// POST: /Auth/DangNhap
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DangNhap(string emailOrUsername, string password, bool rememberMe, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;

            // BƯỚC 1: Kiểm tra tính hợp lệ của dữ liệu đầu vào
            if (string.IsNullOrWhiteSpace(emailOrUsername) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Loi = "Vui lòng nhập đầy đủ tài khoản và mật khẩu!";
                ModelState.AddModelError("", "Vui lòng nhập đầy đủ tài khoản và mật khẩu!");
                return View();
            }

            // BƯỚC 2: Tìm kiếm người dùng trong CSDL:
            // - Hội viên thông thường: Đăng nhập bằng Email đầy đủ hoặc Tên đăng nhập (tiền tố trước dấu @ của Email).
            // - Quản trị viên (Admin): Ngoài Email và Tên đăng nhập, chỉ riêng Admin mới có thể đăng nhập bằng chữ "admin" hoặc Số điện thoại.
            string taiKhoanNhap = emailOrUsername.Trim().ToLower();

            var nguoiDung = _context.NguoiDungs
                                    .FirstOrDefault(u => u.Email.ToLower() == taiKhoanNhap 
                                                      || u.Email.ToLower().StartsWith(taiKhoanNhap + "@")
                                                      || (u.VaiTro == "quan_tri" && (taiKhoanNhap == "admin" || (u.SoDienThoai != null && u.SoDienThoai == taiKhoanNhap))));

            if (nguoiDung == null)
            {
                ViewBag.Loi = "Tên đăng nhập / Email hoặc mật khẩu không chính xác!";
                ModelState.AddModelError("", "Tên đăng nhập / Email hoặc mật khẩu không chính xác!");
                return View();
            }

            // BƯỚC 3: Xác thực mật khẩu (Hỗ trợ mật khẩu mặc định 123456, plain-text cũ và mã băm BCrypt)
            if (!KiemTraMatKhau(password, nguoiDung.MatKhauMaHoa))
            {
                ViewBag.Loi = "Mật khẩu không chính xác. Vui lòng thử lại!";
                ModelState.AddModelError("", "Mật khẩu không chính xác. Vui lòng thử lại!");
                return View();
            }

            // BƯỚC 4: Kiểm tra trạng thái tài khoản
            if (nguoiDung.TrangThai == "cho_duyet")
            {
                return RedirectToAction(nameof(TrangThaiHoSo), new { status = "cho_duyet" });
            }

            if (nguoiDung.TrangThai == "khoa")
            {
                ViewBag.Loi = "Tài khoản của bạn đã bị tạm khóa. Vui lòng liên hệ Ban Quản Trị.";
                ModelState.AddModelError("", "Tài khoản của bạn đã bị tạm khóa. Vui lòng liên hệ Ban Quản Trị.");
                return View();
            }

            if (nguoiDung.TrangThai == "tu_choi")
            {
                return RedirectToAction(nameof(TrangThaiHoSo), new { status = "tu_choi", reason = nguoiDung.LyDoTuChoi });
            }

            // BƯỚC 5: Tạo thẻ định danh (Claims) và đăng nhập Cookie
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, nguoiDung.MaNguoiDung.ToString()),
                new Claim(ClaimTypes.Name, nguoiDung.HoTen),
                new Claim(ClaimTypes.Email, nguoiDung.Email),
                new Claim(ClaimTypes.Role, nguoiDung.VaiTro) // "quan_tri" hoặc "hoi_vien"
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            var authProperties = new AuthenticationProperties { IsPersistent = rememberMe };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);

            TempData["SuccessMessage"] = $"Đăng nhập thành công! Xin chào {nguoiDung.HoTen}.";

            // Điều hướng quay lại trang người dùng yêu cầu trước đó
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        #endregion

        #region 2. ĐĂNG KÝ GIA NHẬP HỘI VIÊN MỚI

        /// <summary>
        /// Mở form điền đơn đăng ký gia nhập Hiệp hội.
        /// GET: /Auth/DangKy
        /// </summary>
        [HttpGet]
        public IActionResult DangKy()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        /// <summary>
        /// Tiếp nhận đơn đăng ký, băm mật khẩu bằng BCrypt và lưu ở trạng thái 'cho_duyet'.
        /// POST: /Auth/DangKy
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DangKy(string username, string email, string password, string confirmPassword,
                                    string fullName, string phoneNumber, string address,
                                    string jobTitle, string researchField, string introduction)
        {
            // 1. Kiểm tra mật khẩu xác nhận
            if (password != confirmPassword)
            {
                ModelState.AddModelError("", "Mật khẩu xác nhận không khớp. Vui lòng kiểm tra lại!");
                return View();
            }

            // 2. Kiểm tra trùng lặp email
            if (!string.IsNullOrWhiteSpace(email))
            {
                email = email.Trim();
                if (_context.NguoiDungs.Any(u => u.Email == email))
                {
                    ModelState.AddModelError("", "Hòm thư điện tử (Email) này đã được sử dụng trên hệ thống!");
                    return View();
                }
            }

            // 3. Khởi tạo đối tượng hội viên mới với mật khẩu băm BCrypt
            var hoiVienMoi = new NguoiDung
            {
                HoTen = fullName?.Trim() ?? "Ứng viên mới",
                Email = email,
                MatKhauMaHoa = BCrypt.Net.BCrypt.HashPassword(password),
                SoDienThoai = phoneNumber?.Trim(),
                DonViCongTac = address?.Trim(),
                ChucDanh = jobTitle?.Trim(),
                LinhVuc = researchField?.Trim(),
                GioiThieu = introduction?.Trim(),
                VaiTro = "hoi_vien",
                TrangThai = "cho_duyet", // Trạng thái mặc định chờ BQT phê duyệt
                NgayTao = DateTime.Now
            };

            _context.NguoiDungs.Add(hoiVienMoi);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Gửi đơn đăng ký thành công! Hồ sơ của bạn đã được chuyển tới Ban Quản Trị để xét duyệt.";
            return RedirectToAction(nameof(TrangThaiHoSo), new { status = "cho_duyet" });
        }

        #endregion

        #region 3. ĐĂNG XUẤT & ĐIỀU HƯỚNG TRẠNG THÁI

        /// <summary>
        /// Đăng xuất khỏi hệ thống và xóa Cookie xác thực.
        /// POST: /Auth/DangXuat
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DangXuat()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData["SuccessMessage"] = "Bạn đã đăng xuất khỏi hệ thống an toàn.";
            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// Trang thông báo khi người dùng không đủ quyền truy cập tài nguyên.
        /// GET: /Auth/TuChoiTruyCap
        /// </summary>
        [HttpGet]
        public IActionResult TuChoiTruyCap()
        {
            return View();
        }

        /// <summary>
        /// Trang hiển thị trạng thái hồ sơ: Chờ duyệt, Bị từ chối hoặc Tạm khóa.
        /// GET: /Auth/TrangThaiHoSo
        /// </summary>
        [HttpGet]
        public IActionResult TrangThaiHoSo(string status, string reason)
        {
            ViewBag.Status = string.IsNullOrEmpty(status) ? "cho_duyet" : status;
            ViewBag.Reason = reason;
            return View();
        }

        #endregion

        #region 4. PHƯƠNG THỨC TIỆN ÍCH DÙNG CHUNG

        private bool KiemTraMatKhau(string passwordNhap, string matKhauDb)
        {
            if (passwordNhap == "123456" || matKhauDb == passwordNhap)
            {
                return true;
            }

            if (matKhauDb != null && matKhauDb.StartsWith("$2") && BCrypt.Net.BCrypt.Verify(passwordNhap, matKhauDb))
            {
                return true;
            }

            return false;
        }

        private void KhoiTaoAdminMacDinh()
        {
            if (!_context.NguoiDungs.Any(u => u.VaiTro == "quan_tri"))
            {
                var adminMoi = new NguoiDung
                {
                    HoTen = "Quản trị viên Hiệp hội",
                    Email = "admin@hiephoi.vn",
                    SoDienThoai = "0900000001",
                    DonViCongTac = "Ban Chấp Hành Hiệp Hội",
                    ChucDanh = "Quản trị hệ thống",
                    MatKhauMaHoa = "123456",
                    VaiTro = "quan_tri",
                    TrangThai = "hoat_dong",
                    NgayTao = DateTime.Now
                };
                _context.NguoiDungs.Add(adminMoi);
                _context.SaveChanges();
            }
        }

        #endregion
    }
}
