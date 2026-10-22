using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WNC15.Data;
using WNC15.Models;

namespace WNC15.Controllers
{
    // Controller quản lý Sự kiện, Đăng ký tham gia và Điểm danh
    public class EventController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EventController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Danh sách sự kiện (GET: /Event/DanhSachSuKien)
        public IActionResult DanhSachSuKien()
        {
            // Lấy danh sách sự kiện từ CSDL kèm số người đã đăng ký
            var danhSach = _context.SuKiens
                                  .Include(s => s.DangKySuKiens)
                                  .OrderByDescending(s => s.ThoiGianToChuc)
                                  .ToList();

            return View(danhSach);
        }

        // 2. Chi tiết sự kiện & Giữ chỗ (GET: /Event/ChiTiet/5)
        public IActionResult ChiTiet(int id)
        {
            var suKien = _context.SuKiens
                                 .Include(s => s.DangKySuKiens)
                                 .FirstOrDefault(s => s.MaSuKien == id);

            if (suKien == null)
            {
                // Nếu chưa có sự kiện trong CSDL, tạo một đối tượng hiển thị mặc định
                suKien = new SuKien
                {
                    MaSuKien = id,
                    TenSuKien = "Hội thảo Khoa học Quốc tế về Trí tuệ Nhân tạo & Dữ liệu",
                    DiaDiem = "Hội trường A, Tòa nhà Hiệp hội",
                    ThoiGianToChuc = DateTime.Now.AddDays(15),
                    HanDangKy = DateTime.Now.AddDays(10),
                    SoLuongToiDa = 100,
                    MoTa = "Hội thảo quy tụ các chuyên gia hàng đầu trao đổi về ứng dụng AI trong thực tiễn."
                };
            }

            return View(suKien);
        }

        // 3. Hội viên bấm nút "Đăng ký tham gia" (POST: /Event/DangKyThamGia)
        [HttpPost]
        public IActionResult DangKyThamGia(int id)
        {
            // Kiểm tra sự kiện có tồn tại không, nếu chưa có thì tạo sự kiện mẫu vào CSDL
            var suKien = _context.SuKiens.Find(id);
            if (suKien == null)
            {
                suKien = new SuKien
                {
                    MaSuKien = id,
                    TenSuKien = "Hội thảo Khoa học Quốc tế về Trí tuệ Nhân tạo & Dữ liệu",
                    DiaDiem = "Hội trường A, Tòa nhà Hiệp hội",
                    ThoiGianToChuc = DateTime.Now.AddDays(15),
                    HanDangKy = DateTime.Now.AddDays(10),
                    SoLuongToiDa = 100,
                    MaNguoiTao = 1
                };
                _context.SuKiens.Add(suKien);
                _context.SaveChanges();
            }

            // Đảm bảo có ít nhất 1 người dùng mẫu trong CSDL để liên kết
            var nguoiDung = _context.NguoiDungs.FirstOrDefault();
            if (nguoiDung == null)
            {
                nguoiDung = new NguoiDung
                {
                    HoTen = "Nguyễn Văn An",
                    Email = "nguyenvanan@gmail.com",
                    SoDienThoai = "0912345678",
                    DonViCongTac = "Trường Đại học Bách Khoa",
                    ChucDanh = "Tiến sĩ / Giảng viên",
                    LinhVuc = "Khoa học máy tính",
                    MatKhauMaHoa = "123456",
                    TrangThai = "hoat_dong"
                };
                _context.NguoiDungs.Add(nguoiDung);
                _context.SaveChanges();
            }

            // Kiểm tra người này đã đăng ký sự kiện này chưa
            var daDangKy = _context.DangKySuKiens.Any(d => d.MaSuKien == id && d.MaNguoiDung == nguoiDung.MaNguoiDung);
            if (!daDangKy)
            {
                // Thêm bản ghi đăng ký mới vào CSDL -> SỐ LƯỢNG TỰ ĐỘNG TĂNG!
                var dangKyMoi = new DangKySuKien
                {
                    MaSuKien = id,
                    MaNguoiDung = nguoiDung.MaNguoiDung,
                    NgayDangKy = DateTime.Now,
                    TrangThai = "da_dang_ky"
                };

                _context.DangKySuKiens.Add(dangKyMoi);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Đăng ký giữ chỗ thành công! Bạn đã được thêm vào danh sách điểm danh.";
            }
            else
            {
                TempData["ErrorMessage"] = "Bạn đã đăng ký tham gia sự kiện này trước đó rồi!";
            }

            return RedirectToAction("ChiTiet", new { id });
        }

        // 4. Hội viên bấm nút "Hủy đăng ký" (POST: /Event/HuyDangKy)
        [HttpPost]
        public IActionResult HuyDangKy(int id)
        {
            var dangKy = _context.DangKySuKiens.FirstOrDefault(d => d.MaSuKien == id);
            if (dangKy != null)
            {
                _context.DangKySuKiens.Remove(dangKy);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Đã hủy đăng ký thành công. Số lượng chỗ trống đã được hoàn lại.";
            }
            else
            {
                TempData["ErrorMessage"] = "Bạn chưa đăng ký tham gia sự kiện này.";
            }

            return RedirectToAction("ChiTiet", new { id });
        }

        // 5. Mở form thêm sự kiện mới (GET: /Event/ThemSuKien)
        public IActionResult ThemSuKien()
        {
            return View();
        }

        // 6. Xử lý lưu sự kiện mới (POST: /Event/ThemSuKien)
        [HttpPost]
        public IActionResult ThemSuKien(string title, string location, int maxParticipants, DateTime startDate)
        {
            var suKienMoi = new SuKien
            {
                TenSuKien = title,
                DiaDiem = location,
                SoLuongToiDa = maxParticipants > 0 ? maxParticipants : 100,
                ThoiGianToChuc = startDate != default ? startDate : DateTime.Now.AddDays(7),
                HanDangKy = startDate != default ? startDate.AddDays(-1) : DateTime.Now.AddDays(6),
                MaNguoiTao = 1
            };

            _context.SuKiens.Add(suKienMoi);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Tạo sự kiện mới thành công!";
            return RedirectToAction("DanhSachSuKien");
        }

        // 7. Quản trị viên xem Danh sách Điểm danh TỰ ĐỘNG TĂNG theo CSDL (GET: /Event/DanhSachDiemDanh/5)
        public IActionResult DanhSachDiemDanh(int id)
        {
            // Lấy danh sách đăng ký thực tế từ CSDL kèm thông tin hội viên
            var danhSach = _context.DangKySuKiens
                                  .Include(d => d.NguoiDung)
                                  .Include(d => d.SuKien)
                                  .Where(d => d.MaSuKien == id)
                                  .OrderByDescending(d => d.NgayDangKy)
                                  .ToList();

            var suKien = _context.SuKiens.Find(id);
            ViewBag.TenSuKien = suKien?.TenSuKien ?? "Hội thảo Khoa học Quốc tế";
            ViewBag.MaSuKien = id;

            return View(danhSach); // Bắn danh sách thật sang View
        }
    }
}
