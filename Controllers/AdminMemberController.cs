using Microsoft.AspNetCore.Mvc;
using WNC15.Data;
using WNC15.Models;

namespace WNC15.Controllers
{
    // Controller dành cho Quản trị viên quản lý và duyệt hội viên
    public class AdminMemberController : Controller
    {
        // Biến lưu ngữ cảnh CSDL
        private ApplicationDbContext _context;

        public AdminMemberController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Danh sách hồ sơ đang chờ duyệt (GET: /AdminMember/HoSoChoDuyet)
        public IActionResult HoSoChoDuyet()
        {
            // Lấy toàn bộ người dùng có trạng thái 'cho_duyet', sắp xếp người nộp mới nhất lên đầu
            var danhSachChoDuyet = _context.NguoiDungs
                                           .Where(u => u.TrangThai == "cho_duyet")
                                           .OrderByDescending(u => u.NgayTao)
                                           .ToList();

            return View(danhSachChoDuyet); // Bắn danh sách thật sang View
        }

        // 2. Danh sách toàn bộ hội viên trong hệ thống (GET: /AdminMember/DanhSachHoiVien)
        public IActionResult DanhSachHoiVien(string keyword)
        {
            var query = _context.NguoiDungs
                                .Where(u => u.TrangThai != "cho_duyet");

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim().ToLower();
                query = query.Where(u => u.HoTen.ToLower().Contains(keyword)
                                      || u.Email.ToLower().Contains(keyword)
                                      || (u.DonViCongTac != null && u.DonViCongTac.ToLower().Contains(keyword)));
            }

            var danhSachHoiVien = query.OrderByDescending(u => u.NgayTao).ToList();

            // Đếm số lượng hồ sơ đang chờ duyệt để hiển thị huy hiệu trên nút bấm
            ViewBag.SoHoSoChoDuyet = _context.NguoiDungs.Count(u => u.TrangThai == "cho_duyet");
            ViewBag.Keyword = keyword;

            return View(danhSachHoiVien);
        }

        // 3. Phê duyệt hồ sơ hội viên (POST: /AdminMember/PheDuyet)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult PheDuyet(int id)
        {
            var hoiVien = _context.NguoiDungs.Find(id);
            if (hoiVien != null)
            {
                hoiVien.TrangThai = "hoat_dong"; // Đổi trạng thái sang Hoạt động chính thức
                hoiVien.LyDoTuChoi = null;
                _context.SaveChanges();
                TempData["SuccessMessage"] = $"Đã phê duyệt thành công hội viên: {hoiVien.HoTen}!";
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy hồ sơ hội viên cần phê duyệt.";
            }

            return RedirectToAction("HoSoChoDuyet");
        }

        // 4. Từ chối hồ sơ kèm lý do (POST: /AdminMember/TuChoi)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult TuChoi(int id, string lyDo)
        {
            var hoiVien = _context.NguoiDungs.Find(id);
            if (hoiVien != null)
            {
                hoiVien.TrangThai = "tu_choi"; // Đổi trạng thái sang Bị từ chối
                if (string.IsNullOrWhiteSpace(lyDo))
                {
                    hoiVien.LyDoTuChoi = "Hồ sơ chưa đáp ứng yêu cầu gia nhập";
                }
                else
                {
                    hoiVien.LyDoTuChoi = lyDo;
                }
                _context.SaveChanges();
                TempData["SuccessMessage"] = $"Đã từ chối đơn đăng ký của ứng viên: {hoiVien.HoTen}.";
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy hồ sơ hội viên.";
            }

            return RedirectToAction("HoSoChoDuyet");
        }

        // 5. Khóa tài khoản hội viên vi phạm (POST: /AdminMember/KhoaTaiKhoan)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult KhoaTaiKhoan(int id)
        {
            var hoiVien = _context.NguoiDungs.Find(id);
            if (hoiVien != null)
            {
                hoiVien.TrangThai = "khoa";
                _context.SaveChanges();
                TempData["SuccessMessage"] = $"Đã khóa tài khoản của hội viên: {hoiVien.HoTen}.";
            }
            return RedirectToAction("DanhSachHoiVien");
        }

        // 6. Mở khóa tài khoản (POST: /AdminMember/MoKhoaTaiKhoan)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MoKhoaTaiKhoan(int id)
        {
            var hoiVien = _context.NguoiDungs.Find(id);
            if (hoiVien != null)
            {
                hoiVien.TrangThai = "hoat_dong";
                _context.SaveChanges();
                TempData["SuccessMessage"] = $"Đã mở khóa tài khoản của hội viên: {hoiVien.HoTen}.";
            }
            return RedirectToAction("DanhSachHoiVien");
        }

        // 7. Xóa tài khoản (POST: /AdminMember/XoaTaiKhoan)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult XoaTaiKhoan(int id)
        {
            var hoiVien = _context.NguoiDungs.Find(id);
            if (hoiVien != null)
            {
                _context.NguoiDungs.Remove(hoiVien);
                _context.SaveChanges();
                TempData["SuccessMessage"] = $"Đã xóa hồ sơ hội viên: {hoiVien.HoTen} khỏi hệ thống.";
            }
            return RedirectToAction("DanhSachHoiVien");
        }
    }
}
