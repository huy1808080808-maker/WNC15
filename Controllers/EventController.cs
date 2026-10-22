using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WNC15.Data;
using WNC15.Models;

namespace WNC15.Controllers
{
    // Controller quản lý Sự kiện, Đăng ký tham gia và Điểm danh
    public class EventController : Controller
    {
        // Biến lưu ngữ cảnh CSDL, không dùng readonly
        private ApplicationDbContext _context;

        public EventController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Danh sách sự kiện (GET: /Event/DanhSachSuKien)
        public IActionResult DanhSachSuKien(string search)
        {
            var query = _context.SuKiens
                                .Include(s => s.DangKySuKiens)
                                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(s => s.TenSuKien.Contains(search) || s.DiaDiem.Contains(search));
                ViewBag.Search = search;
            }

            var danhSach = query.OrderByDescending(s => s.ThoiGianToChuc).ToList();
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
                // Nếu chưa có sự kiện trong CSDL, tạo một sự kiện mẫu lưu vào CSDL
                suKien = new SuKien
                {
                    TenSuKien = "Hội thảo Khoa học Quốc tế về Trí tuệ Nhân tạo & Dữ liệu",
                    DiaDiem = "Hội trường A, Tòa nhà Hiệp hội",
                    ThoiGianToChuc = DateTime.Now.AddDays(15),
                    HanDangKy = DateTime.Now.AddDays(10),
                    SoLuongToiDa = 100,
                    MaNguoiTao = 1,
                    MoTa = "Hội thảo quy tụ các chuyên gia hàng đầu trao đổi về ứng dụng AI trong thực tiễn."
                };
                _context.SuKiens.Add(suKien);
                _context.SaveChanges();
            }

            return View(suKien);
        }

        // 3. Hội viên bấm nút "Đăng ký tham gia" (POST: /Event/DangKyThamGia)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DangKyThamGia(int id)
        {
            // Kiểm tra sự kiện có tồn tại không
            var suKien = _context.SuKiens.Find(id);
            if (suKien == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin sự kiện này.";
                return RedirectToAction("DanhSachSuKien");
            }

            // Kiểm tra xem sự kiện đã hết hạn đăng ký chưa
            if (DateTime.Now > suKien.HanDangKy)
            {
                TempData["ErrorMessage"] = "Rất tiếc, sự kiện này đã kết thúc thời hạn đăng ký tham dự!";
                return RedirectToAction("ChiTiet", new { id });
            }

            // Kiểm tra số lượng chỗ còn lại
            int soNguoiDaDangKy = _context.DangKySuKiens.Count(d => d.MaSuKien == id);
            if (soNguoiDaDangKy >= suKien.SoLuongToiDa)
            {
                TempData["ErrorMessage"] = "Sự kiện này đã đủ số lượng người đăng ký tối đa!";
                return RedirectToAction("ChiTiet", new { id });
            }

            // Xác định người dùng đăng ký (lấy từ tài khoản đang đăng nhập hoặc người dùng mặc định)
            int maNguoiDung = 0;
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var idClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (idClaim != null)
                {
                    int.TryParse(idClaim.Value, out maNguoiDung);
                }
            }

            // Nếu chưa đăng nhập hoặc không lấy được ID, dùng tài khoản hội viên đầu tiên trong CSDL
            if (maNguoiDung == 0)
            {
                var hoiVienDauTien = _context.NguoiDungs.FirstOrDefault();
                if (hoiVienDauTien == null)
                {
                    hoiVienDauTien = new NguoiDung
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
                    _context.NguoiDungs.Add(hoiVienDauTien);
                    _context.SaveChanges();
                }
                maNguoiDung = hoiVienDauTien.MaNguoiDung;
            }

            // Kiểm tra xem hội viên này đã đăng ký sự kiện này chưa
            bool daDangKy = _context.DangKySuKiens.Any(d => d.MaSuKien == id && d.MaNguoiDung == maNguoiDung);
            if (daDangKy)
            {
                TempData["ErrorMessage"] = "Bạn đã đăng ký tham gia sự kiện này trước đó rồi!";
                return RedirectToAction("ChiTiet", new { id });
            }

            // Thêm bản ghi đăng ký mới vào CSDL -> SỐ LƯỢNG TỰ ĐỘNG TĂNG!
            var dangKyMoi = new DangKySuKien
            {
                MaSuKien = id,
                MaNguoiDung = maNguoiDung,
                NgayDangKy = DateTime.Now,
                TrangThai = "da_dang_ky"
            };

            _context.DangKySuKiens.Add(dangKyMoi);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Đăng ký giữ chỗ thành công! Bạn đã có tên trong danh sách điểm danh.";
            return RedirectToAction("ChiTiet", new { id });
        }

        // 4. Hội viên bấm nút "Hủy đăng ký" (POST: /Event/HuyDangKy)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult HuyDangKy(int id)
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

            // Tìm bản đăng ký cần hủy
            var query = _context.DangKySuKiens.Where(d => d.MaSuKien == id);
            if (maNguoiDung > 0)
            {
                query = query.Where(d => d.MaNguoiDung == maNguoiDung);
            }
            var dangKy = query.FirstOrDefault();

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
        [HttpGet]
        public IActionResult ThemSuKien()
        {
            return View();
        }

        // 6. Xử lý lưu sự kiện mới (POST: /Event/ThemSuKien)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ThemSuKien(string title, string location, int maxParticipants,
                                        string startDate, string endDate, string deadlineDate,
                                        string description)
        {
            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(location))
            {
                ViewBag.ErrorMessage = "Vui lòng nhập đầy đủ tên sự kiện và địa điểm tổ chức!";
                return View();
            }

            int soLuong = 100;
            if (maxParticipants > 0)
            {
                soLuong = maxParticipants;
            }

            DateTime ngayToChuc = DateTime.Now.AddDays(7);
            if (!string.IsNullOrEmpty(startDate))
            {
                DateTime dtBatDau;
                if (DateTime.TryParse(startDate, out dtBatDau))
                {
                    ngayToChuc = dtBatDau;
                }
            }

            DateTime hanDangKy = ngayToChuc.AddDays(-1);
            if (!string.IsNullOrEmpty(deadlineDate))
            {
                DateTime dtHan;
                if (DateTime.TryParse(deadlineDate, out dtHan))
                {
                    hanDangKy = dtHan;
                }
            }

            // Xác định người tạo sự kiện (admin đang đăng nhập)
            int maNguoiTao = 1;
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var idClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (idClaim != null)
                {
                    int parseId = 0;
                    if (int.TryParse(idClaim.Value, out parseId))
                    {
                        if (parseId > 0)
                        {
                            maNguoiTao = parseId;
                        }
                    }
                }
            }

            // Kiểm tra người tạo có tồn tại trong CSDL để tránh lỗi khóa ngoại FK
            var nguoiTao = _context.NguoiDungs.Find(maNguoiTao);
            if (nguoiTao == null)
            {
                var nguoiDauTien = _context.NguoiDungs.FirstOrDefault();
                if (nguoiDauTien != null)
                {
                    maNguoiTao = nguoiDauTien.MaNguoiDung;
                }
            }

            var suKienMoi = new SuKien();
            suKienMoi.TenSuKien = title;
            suKienMoi.DiaDiem = location;
            suKienMoi.SoLuongToiDa = soLuong;
            suKienMoi.ThoiGianToChuc = ngayToChuc;
            suKienMoi.HanDangKy = hanDangKy;
            suKienMoi.MoTa = description;
            suKienMoi.MaNguoiTao = maNguoiTao;
            suKienMoi.NgayTao = DateTime.Now;

            try
            {
                _context.SuKiens.Add(suKienMoi);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Tạo và phát hành sự kiện mới thành công!";
                return RedirectToAction("DanhSachSuKien");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Lỗi khi lưu sự kiện: " + ex.Message;
                return View();
            }
        }

        // 7. Xem Danh sách Điểm danh TỰ ĐỘNG TĂNG theo CSDL (GET: /Event/DanhSachDiemDanh/5)
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
            if (suKien != null)
            {
                ViewBag.TenSuKien = suKien.TenSuKien;
            }
            else
            {
                ViewBag.TenSuKien = "Hội thảo Khoa học Quốc tế";
            }
            ViewBag.MaSuKien = id;

            return View(danhSach); // Bắn danh sách thật sang View
        }

        // 8. Xóa sự kiện (POST: /Event/XoaSuKien)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult XoaSuKien(int id)
        {
            var suKien = _context.SuKiens.Find(id);
            if (suKien != null)
            {
                // Xóa các lượt đăng ký liên quan trước
                var dsDangKy = _context.DangKySuKiens.Where(d => d.MaSuKien == id).ToList();
                if (dsDangKy.Count > 0)
                {
                    _context.DangKySuKiens.RemoveRange(dsDangKy);
                }

                _context.SuKiens.Remove(suKien);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Đã xóa sự kiện thành công!";
            }
            return RedirectToAction("DanhSachSuKien");
        }
    }
}
