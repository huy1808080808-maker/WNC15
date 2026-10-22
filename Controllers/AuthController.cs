using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WNC15.Data;
using WNC15.Models;

namespace WNC15.Controllers
{
    // Controller xử lý Đăng nhập, Đăng ký và Trạng thái tài khoản
    public class AuthController : Controller
    {
        // Biến lưu ngữ cảnh CSDL, không dùng readonly
        private ApplicationDbContext _context;

        public AuthController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Giao diện Đăng nhập (GET: /Auth/DangNhap)
        [HttpGet]
        public IActionResult DangNhap(string returnUrl)
        {
            // Tự động khởi tạo tài khoản Quản trị viên mặc định nếu CSDL chưa có
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

            // Nếu người dùng đã đăng nhập thì tự động chuyển về Trang chủ
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(); // Trả về Views/Auth/DangNhap.cshtml
        }

        // 2. Xử lý khi bấm nút Đăng nhập (POST: /Auth/DangNhap)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DangNhap(string emailOrUsername, string password, bool rememberMe, string returnUrl)
        {
            // BƯỚC 1: Kiểm tra xem có để trống tài khoản hoặc mật khẩu không
            if (string.IsNullOrEmpty(emailOrUsername) || string.IsNullOrEmpty(password))
            {
                ViewBag.Loi = "Vui lòng nhập đầy đủ tài khoản và mật khẩu!";
                ModelState.AddModelError("", "Vui lòng nhập đầy đủ tài khoản và mật khẩu!");
                return View();
            }

            emailOrUsername = emailOrUsername.Trim();

            // BƯỚC 2: Tìm người dùng trong CSDL theo Email hoặc tên 'admin'
            string taiKhoanNhap = emailOrUsername.ToLower();
            var nguoiDung = _context.NguoiDungs
                                    .FirstOrDefault(u => u.Email.ToLower() == taiKhoanNhap 
                                                      || (taiKhoanNhap == "admin" && u.VaiTro == "quan_tri"));

            if (nguoiDung == null)
            {
                ViewBag.Loi = "Tài khoản hoặc mật khẩu không chính xác!";
                ModelState.AddModelError("", "Tài khoản hoặc mật khẩu không chính xác!");
                return View();
            }

            // Kiểm tra mật khẩu (chấp nhận mật khẩu '123456', mật khẩu trùng trong CSDL hoặc mã băm BCrypt)
            bool matKhauDung = false;
            if (password == "123456")
            {
                matKhauDung = true;
            }
            else if (nguoiDung.MatKhauMaHoa == password)
            {
                matKhauDung = true;
            }
            else if (nguoiDung.MatKhauMaHoa.StartsWith("$2") && BCrypt.Net.BCrypt.Verify(password, nguoiDung.MatKhauMaHoa))
            {
                matKhauDung = true;
            }

            if (!matKhauDung)
            {
                ViewBag.Loi = "Mật khẩu không chính xác. Vui lòng thử lại!";
                ModelState.AddModelError("", "Mật khẩu không chính xác. Vui lòng thử lại!");
                return View();
            }

            // BƯỚC 3: Kiểm tra trạng thái tài khoản
            if (nguoiDung.TrangThai == "cho_duyet")
            {
                return RedirectToAction("TrangThaiHoSo", new { status = "cho_duyet" });
            }

            if (nguoiDung.TrangThai == "khoa")
            {
                ViewBag.Loi = "Tài khoản của bạn đã bị tạm khóa. Vui lòng liên hệ Ban Quản Trị.";
                ModelState.AddModelError("", "Tài khoản của bạn đã bị tạm khóa. Vui lòng liên hệ Ban Quản Trị.");
                return View();
            }

            if (nguoiDung.TrangThai == "tu_choi")
            {
                return RedirectToAction("TrangThaiHoSo", new { status = "tu_choi", reason = nguoiDung.LyDoTuChoi });
            }

            // BƯỚC 4: Tạo thẻ nhận diện (Claims) để cấp quyền đăng nhập Cookie
            var danhSachQuyen = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, nguoiDung.MaNguoiDung.ToString()),
                new Claim(ClaimTypes.Name, nguoiDung.HoTen),
                new Claim(ClaimTypes.Email, nguoiDung.Email),
                new Claim(ClaimTypes.Role, nguoiDung.VaiTro) // "quan_tri" hoặc "hoi_vien"
            };

            var chungThu = new ClaimsIdentity(danhSachQuyen, CookieAuthenticationDefaults.AuthenticationScheme);
            var nguoiDungHienTai = new ClaimsPrincipal(chungThu);

            var thuocTinhXacThuc = new AuthenticationProperties
            {
                IsPersistent = rememberMe // Ghi nhớ phiên đăng nhập
            };

            // Đăng nhập vào hệ thống
            HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, nguoiDungHienTai, thuocTinhXacThuc);

            TempData["SuccessMessage"] = "Đăng nhập thành công! Xin chào " + nguoiDung.HoTen + ".";

            // Điều hướng quay lại trang trước đó nếu hợp lệ
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        // 3. Giao diện Đơn đăng ký gia nhập (GET: /Auth/DangKy)
        [HttpGet]
        public IActionResult DangKy()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            return View(); // Trả về Views/Auth/DangKy.cshtml
        }

        // 4. Xử lý khi nộp đơn đăng ký gia nhập (POST: /Auth/DangKy)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DangKy(string username, string email, string password, string confirmPassword,
                                    string fullName, string phoneNumber, string address,
                                    string jobTitle, string researchField, string introduction)
        {
            // Kiểm tra mật khẩu xác nhận
            if (password != confirmPassword)
            {
                ModelState.AddModelError("", "Mật khẩu xác nhận không khớp. Vui lòng kiểm tra lại!");
                return View();
            }

            // Kiểm tra xem email này đã đăng ký trước đó chưa
            if (!string.IsNullOrEmpty(email))
            {
                email = email.Trim();
                bool daTonTai = _context.NguoiDungs.Any(u => u.Email == email);
                if (daTonTai)
                {
                    ModelState.AddModelError("", "Hòm thư điện tử (Email) này đã được sử dụng trên hệ thống!");
                    return View();
                }
            }

            // Tạo hồ sơ hội viên mới với trạng thái 'cho_duyet'
            var hoiVienMoi = new NguoiDung
            {
                HoTen = fullName,
                Email = email,
                MatKhauMaHoa = password,
                SoDienThoai = phoneNumber,
                DonViCongTac = address,
                ChucDanh = jobTitle,
                LinhVuc = researchField,
                GioiThieu = introduction,
                VaiTro = "hoi_vien",
                TrangThai = "cho_duyet", // Hồ sơ mới luôn ở trạng thái Chờ ban quản trị duyệt
                NgayTao = DateTime.Now
            };

            // Lưu vào cơ sở dữ liệu
            _context.NguoiDungs.Add(hoiVienMoi);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Gửi đơn đăng ký thành công! Hồ sơ của bạn đã được chuyển tới Ban Quản Trị để xét duyệt.";
            return RedirectToAction("TrangThaiHoSo", new { status = "cho_duyet" });
        }

        // 5. Đăng xuất khỏi hệ thống (POST: /Auth/DangXuat)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DangXuat()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData["SuccessMessage"] = "Bạn đã đăng xuất khỏi hệ thống an toàn.";
            return RedirectToAction("Index", "Home");
        }

        // 6. Trang thông báo không có quyền truy cập (GET: /Auth/TuChoiTruyCap)
        [HttpGet]
        public IActionResult TuChoiTruyCap()
        {
            return View(); // Trả về Views/Auth/TuChoiTruyCap.cshtml
        }

        // 7. Trang xem tình trạng hồ sơ: Chờ duyệt, từ chối, khóa (GET: /Auth/TrangThaiHoSo)
        [HttpGet]
        public IActionResult TrangThaiHoSo(string status, string reason)
        {
            if (string.IsNullOrEmpty(status))
            {
                status = "cho_duyet";
            }
            ViewBag.Status = status;
            ViewBag.Reason = reason;
            return View(); // Trả về Views/Auth/TrangThaiHoSo.cshtml
        }
    }
}
