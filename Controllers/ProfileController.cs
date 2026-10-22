using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WNC15.Data;
using WNC15.Models;

namespace WNC15.Controllers
{
    // Controller quản lý Hồ sơ cá nhân của Hội viên
    public class ProfileController : Controller
    {
        // Biến lưu ngữ cảnh CSDL, không dùng readonly
        private ApplicationDbContext _context;

        public ProfileController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Xem thông tin cá nhân (GET: /Profile/ThongTinCaNhan)
        public IActionResult ThongTinCaNhan()
        {
            var nguoiDung = LayNguoiDungHienTai();
            if (nguoiDung == null)
            {
                return RedirectToAction("DangNhap", "Auth");
            }

            // Đếm số lượng sự kiện hội viên này đã đăng ký tham gia
            int soSuKien = _context.DangKySuKiens.Count(d => d.MaNguoiDung == nguoiDung.MaNguoiDung);
            ViewBag.SoSuKien = soSuKien;

            return View(nguoiDung); // Bắn đối tượng người dùng thật sang View
        }

        // 2. Mở form chỉnh sửa thông tin (GET: /Profile/ChinhSua)
        [HttpGet]
        public IActionResult ChinhSua()
        {
            var nguoiDung = LayNguoiDungHienTai();
            if (nguoiDung == null)
            {
                return RedirectToAction("DangNhap", "Auth");
            }

            return View(nguoiDung); // Trả về Views/Profile/ChinhSua.cshtml
        }

        // 3. Xử lý lưu thông tin chỉnh sửa (POST: /Profile/ChinhSua)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChinhSua(string fullName, string phoneNumber, string address, string jobTitle, string introduction)
        {
            var nguoiDung = LayNguoiDungHienTai();
            if (nguoiDung == null)
            {
                return RedirectToAction("DangNhap", "Auth");
            }

            if (string.IsNullOrEmpty(fullName))
            {
                ModelState.AddModelError("", "Họ và tên không được để trống!");
                return View(nguoiDung);
            }

            // Cập nhật thông tin mới vào CSDL
            nguoiDung.HoTen = fullName;
            nguoiDung.SoDienThoai = phoneNumber;
            nguoiDung.DonViCongTac = address;
            nguoiDung.ChucDanh = jobTitle;
            nguoiDung.GioiThieu = introduction;

            _context.SaveChanges();

            TempData["SuccessMessage"] = "Cập nhật thông tin cá nhân thành công!";
            return RedirectToAction("ThongTinCaNhan");
        }

        // 4. Mở form đổi mật khẩu (GET: /Profile/DoiMatKhau)
        [HttpGet]
        public IActionResult DoiMatKhau()
        {
            return View(); // Trả về Views/Profile/DoiMatKhau.cshtml
        }

        // 5. Xử lý đổi mật khẩu (POST: /Profile/DoiMatKhau)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DoiMatKhau(string currentPassword, string newPassword, string confirmNewPassword)
        {
            var nguoiDung = LayNguoiDungHienTai();
            if (nguoiDung == null)
            {
                return RedirectToAction("DangNhap", "Auth");
            }

            // Kiểm tra mật khẩu hiện tại
            if (nguoiDung.MatKhauMaHoa != currentPassword)
            {
                ModelState.AddModelError("", "Mật khẩu hiện tại không chính xác!");
                return View();
            }

            // Kiểm tra độ dài mật khẩu mới
            if (string.IsNullOrEmpty(newPassword) || newPassword.Length < 6)
            {
                ModelState.AddModelError("", "Mật khẩu mới phải có tối thiểu 6 ký tự!");
                return View();
            }

            // Kiểm tra khớp mật khẩu xác nhận
            if (newPassword != confirmNewPassword)
            {
                ModelState.AddModelError("", "Mật khẩu xác nhận không khớp với mật khẩu mới!");
                return View();
            }

            // Cập nhật mật khẩu mới vào CSDL
            nguoiDung.MatKhauMaHoa = newPassword;
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Đổi mật khẩu tài khoản thành công!";
            return RedirectToAction("ThongTinCaNhan");
        }

        // 6. Xem các sự kiện mình đã đăng ký (GET: /Profile/SuKienCuaToi)
        public IActionResult SuKienCuaToi()
        {
            var nguoiDung = LayNguoiDungHienTai();
            if (nguoiDung == null)
            {
                return RedirectToAction("DangNhap", "Auth");
            }

            // Lấy danh sách các sự kiện mà hội viên này đã đăng ký
            var danhSach = _context.DangKySuKiens
                                  .Include(d => d.SuKien)
                                  .Where(d => d.MaNguoiDung == nguoiDung.MaNguoiDung)
                                  .OrderByDescending(d => d.NgayDangKy)
                                  .ToList();

            return View(danhSach); // Bắn danh sách sự kiện thật sang View
        }

        // Hàm phụ: Lấy tài khoản người dùng đang đăng nhập hoặc người dùng mẫu đầu tiên
        private NguoiDung LayNguoiDungHienTai()
        {
            int maNguoiDung = 0;
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var idClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (idClaim != null)
                {
                    int.TryParse(idClaim.Value, out maNguoiDung);
                }
            }

            if (maNguoiDung > 0)
            {
                var timKiem = _context.NguoiDungs.Find(maNguoiDung);
                if (timKiem != null)
                {
                    return timKiem;
                }
            }

            // Fallback lấy người dùng đầu tiên trong CSDL hoặc tạo mặc định
            var macDinh = _context.NguoiDungs.FirstOrDefault();
            if (macDinh != null)
            {
                return macDinh;
            }

            return new NguoiDung();
        }
    }
}
