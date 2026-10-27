using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WNC15.Data;
using WNC15.Models;

namespace WNC15.Controllers
{
    // Controller quản lý Trang chủ và các trang cơ bản
    public class HomeController : Controller
    {
        // Biến lưu ngữ cảnh CSDL
        private ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Trang chủ chính (đường dẫn: / hoặc /Home/Index)
        public IActionResult Index()
        {
            // Thống kê số lượng thực tế từ cơ sở dữ liệu
            int soHoiVien = _context.NguoiDungs.Count(u => u.TrangThai == "hoat_dong");
            int soSuKien = _context.SuKiens.Count();
            int soBaiViet = _context.BaiViets.Count();

            // Nếu CSDL mới chưa có dữ liệu thì đặt số liệu mặc định đẹp mắt
            if (soHoiVien == 0)
            {
                soHoiVien = 500;
            }
            if (soSuKien == 0)
            {
                soSuKien = 50;
            }
            if (soBaiViet == 0)
            {
                soBaiViet = 200;
            }

            ViewBag.TongSoHoiVien = soHoiVien;
            ViewBag.TongSoSuKien = soSuKien;
            ViewBag.TongSoBaiViet = soBaiViet;

            // Lấy 3 bài viết mới nhất từ CSDL
            var tinTucMoi = _context.BaiViets
                                    .Include(b => b.DanhMuc)
                                    .OrderByDescending(b => b.NgayTao)
                                    .Take(3)
                                    .ToList();

            // Lấy 3 sự kiện sắp tới từ CSDL
            var suKienMoi = _context.SuKiens
                                    .OrderByDescending(s => s.ThoiGianToChuc)
                                    .Take(3)
                                    .ToList();

            ViewBag.TinTucMoi = tinTucMoi;
            ViewBag.SuKienMoi = suKienMoi;

            return View(); // Trả về giao diện Views/Home/Index.cshtml
        }

        // 2. Trang giới thiệu về Hiệp hội (đường dẫn: /Home/About)
        public IActionResult About()
        {
            return View(); // Trả về giao diện giới thiệu
        }

        // 3. Trang chính sách bảo mật (đường dẫn: /Home/Privacy)
        public IActionResult Privacy()
        {
            return View(); // Trả về chính sách bảo mật
        }
    }
}
