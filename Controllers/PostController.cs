using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WNC15.Data;
using WNC15.Models;

namespace WNC15.Controllers
{
    // Controller quản lý Tin tức và Bài viết
    public class PostController : Controller
    {
        // Biến lưu ngữ cảnh CSDL, không dùng readonly
        private ApplicationDbContext _context;

        public PostController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Danh sách tin tức (GET: /Post/DanhSachTinTuc)
        public IActionResult DanhSachTinTuc(string search, int categoryId)
        {
            // Đảm bảo có ít nhất 1 danh mục và 1 bài viết mẫu trong CSDL
            KiemTraVaKhoiTaoDuLieuMau();

            var query = _context.BaiViets
                                .Include(b => b.DanhMuc)
                                .Include(b => b.TacGia)
                                .AsQueryable();

            // Lọc theo từ khóa tìm kiếm
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(b => b.TieuDe.ToLower().Contains(search) 
                                      || (b.TomTat != null && b.TomTat.ToLower().Contains(search)));
            }

            // Lọc theo chuyên mục
            if (categoryId > 0)
            {
                query = query.Where(b => b.MaDanhMuc == categoryId);
            }

            var danhSach = query.OrderByDescending(b => b.NgayTao).ToList();

            ViewBag.Search = search;
            ViewBag.CategoryId = categoryId;

            return View(danhSach); // Bắn danh sách thật sang View
        }

        // 2. Chi tiết một bài viết (GET: /Post/ChiTiet/5)
        public IActionResult ChiTiet(int id)
        {
            KiemTraVaKhoiTaoDuLieuMau();

            var baiViet = _context.BaiViets
                                  .Include(b => b.DanhMuc)
                                  .Include(b => b.TacGia)
                                  .FirstOrDefault(b => b.MaBaiViet == id);

            if (baiViet == null)
            {
                baiViet = _context.BaiViets
                                  .Include(b => b.DanhMuc)
                                  .Include(b => b.TacGia)
                                  .FirstOrDefault();
            }

            if (baiViet != null)
            {
                // Tăng số lượt xem bài viết
                baiViet.LuotXem = baiViet.LuotXem + 1;
                _context.SaveChanges();
            }

            return View(baiViet);
        }

        // 3. Mở form đăng bài viết mới (GET: /Post/ThemTinTuc)
        [HttpGet]
        public IActionResult ThemTinTuc()
        {
            return View(); // Trả về Views/Post/ThemTinTuc.cshtml
        }

        // 4. Xử lý lưu bài viết mới (POST: /Post/ThemTinTuc)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ThemTinTuc(string title, int categoryId, string summary, string content)
        {
            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(content))
            {
                ModelState.AddModelError("", "Vui lòng nhập đầy đủ tiêu đề và nội dung bài viết!");
                return View();
            }

            // Xác định tác giả đăng bài
            int maTacGia = 1;
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var idClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (idClaim != null)
                {
                    int.TryParse(idClaim.Value, out maTacGia);
                }
            }

            // Đảm bảo tác giả tồn tại trong CSDL
            var tacGia = _context.NguoiDungs.Find(maTacGia);
            if (tacGia == null)
            {
                var nguoiDungDau = _context.NguoiDungs.FirstOrDefault();
                if (nguoiDungDau != null)
                {
                    maTacGia = nguoiDungDau.MaNguoiDung;
                }
            }

            // Đảm bảo có danh mục hợp lệ
            if (categoryId <= 0)
            {
                var danhMucDau = _context.DanhMucs.FirstOrDefault();
                if (danhMucDau != null)
                {
                    categoryId = danhMucDau.MaDanhMuc;
                }
                else
                {
                    categoryId = 1;
                }
            }

            var baiVietMoi = new BaiViet
            {
                TieuDe = title,
                MaDanhMuc = categoryId,
                TomTat = summary,
                NoiDung = content,
                MaTacGia = maTacGia,
                TrangThai = "da_dang",
                LuotXem = 1,
                NgayTao = DateTime.Now
            };

            _context.BaiViets.Add(baiVietMoi);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Đăng bài viết mới thành công!";
            return RedirectToAction("DanhSachTinTuc");
        }

        // 5. Xóa bài viết (POST: /Post/XoaTinTuc)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult XoaTinTuc(int id)
        {
            var baiViet = _context.BaiViets.Find(id);
            if (baiViet != null)
            {
                _context.BaiViets.Remove(baiViet);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Đã xóa bài viết thành công!";
            }
            return RedirectToAction("DanhSachTinTuc");
        }

        // Hàm phụ: Tự động khởi tạo danh mục và bài viết mẫu nếu CSDL hoàn toàn trống
        private void KiemTraVaKhoiTaoDuLieuMau()
        {
            if (!_context.DanhMucs.Any())
            {
                _context.DanhMucs.AddRange(
                    new DanhMuc { TenDanhMuc = "Nghiên cứu & Công nghệ", MoTa = "Các công trình và xu hướng công nghệ mới" },
                    new DanhMuc { TenDanhMuc = "Thông báo Hiệp hội", MoTa = "Tin tức điều hành và kế hoạch hoạt động" },
                    new DanhMuc { TenDanhMuc = "Sinh hoạt Hội viên", MoTa = "Các buổi giao lưu và hoạt động học thuật" }
                );
                _context.SaveChanges();
            }

            if (!_context.BaiViets.Any())
            {
                var admin = _context.NguoiDungs.FirstOrDefault();
                int maAdmin = 1;
                if (admin != null)
                {
                    maAdmin = admin.MaNguoiDung;
                }
                else
                {
                    admin = new NguoiDung
                    {
                        HoTen = "Ban Thư Ký Hiệp Hội",
                        Email = "banquantri@hiephoi.vn",
                        MatKhauMaHoa = "123456",
                        VaiTro = "quan_tri",
                        TrangThai = "hoat_dong"
                    };
                    _context.NguoiDungs.Add(admin);
                    _context.SaveChanges();
                    maAdmin = admin.MaNguoiDung;
                }

                var danhMucDau = _context.DanhMucs.FirstOrDefault();
                int maDanhMuc = danhMucDau != null ? danhMucDau.MaDanhMuc : 1;

                _context.BaiViets.Add(new BaiViet
                {
                    TieuDe = "Xu hướng ứng dụng Trí tuệ nhân tạo trong chuyển đổi số và nâng cao năng suất",
                    MaDanhMuc = maDanhMuc,
                    TomTat = "Tổng hợp các nghiên cứu mới nhất về ứng dụng AI trong tự động hóa quy trình làm việc và phân tích dữ liệu lớn.",
                    NoiDung = "Trong bối cảnh kỷ nguyên số bùng nổ, việc nghiên cứu và ứng dụng trí tuệ nhân tạo đã trở thành một yêu cầu thực tiễn cấp bách. Các hội viên và nhà khoa học của Hiệp hội đã không ngừng đóng góp các sáng kiến, giải pháp công nghệ nhằm hỗ trợ thiết thực cho hoạt động của cộng đồng.",
                    MaTacGia = maAdmin,
                    TrangThai = "da_dang",
                    LuotXem = 125,
                    NgayTao = DateTime.Now
                });
                _context.SaveChanges();
            }
        }
    }
}
