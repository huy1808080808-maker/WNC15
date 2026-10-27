using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WNC15.Data;
using WNC15.Models;

namespace WNC15.Controllers
{
    /// <summary>
    /// Controller quản lý Tin tức & Bài viết:
    /// - Hiển thị danh sách tin tức công khai (kèm bộ lọc theo chuyên mục và tìm kiếm từ khóa).
    /// - Xem chi tiết bài viết (tự động đếm lượt xem).
    /// - Dành cho Quản trị viên: Đăng bài viết mới, Chỉnh sửa, Xóa bài viết.
    /// </summary>
    public class PostController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PostController(ApplicationDbContext context)
        {
            _context = context;
        }

        #region 1. XEM DANH SÁCH & CHI TIẾT BÀI VIẾT

        /// <summary>
        /// Danh sách bài viết & tin tức (Khách và hội viên chỉ xem được bài 'da_dang', Admin xem được tất cả).
        /// GET: /Post/DanhSachTinTuc
        /// </summary>
        public IActionResult DanhSachTinTuc(string search, int categoryId)
        {
            KiemTraVaKhoiTaoDuLieuMau();

            var query = _context.BaiViets
                                .Include(b => b.DanhMuc)
                                .Include(b => b.TacGia)
                                .AsQueryable();

            // Khách vãng lai và hội viên thường chỉ xem bài đã xuất bản
            bool isAdmin = User.Identity != null && (User.IsInRole("Admin") || User.IsInRole("quan_tri"));
            if (!isAdmin)
            {
                query = query.Where(b => b.TrangThai == "da_dang");
            }

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
            ViewBag.DanhMucs = _context.DanhMucs.OrderBy(d => d.TenDanhMuc).ToList();

            return View(danhSach);
        }

        /// <summary>
        /// Xem nội dung chi tiết bài viết và tự động tăng số lượt xem.
        /// GET: /Post/ChiTiet/5
        /// </summary>
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
                baiViet.LuotXem++;
                _context.SaveChanges();
            }

            return View(baiViet);
        }

        #endregion

        #region 2. QUẢN TRỊ VIÊN: ĐĂNG, SỬA & XÓA BÀI VIẾT

        /// <summary>
        /// Mở form soạn thảo bài viết mới (chỉ Admin).
        /// GET: /Post/ThemTinTuc
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin,quan_tri")]
        public IActionResult ThemTinTuc()
        {
            ViewBag.DanhMucs = _context.DanhMucs.OrderBy(d => d.TenDanhMuc).ToList();
            return View();
        }

        /// <summary>
        /// Lưu bài viết mới vào CSDL (chỉ Admin).
        /// POST: /Post/ThemTinTuc
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,quan_tri")]
        public IActionResult ThemTinTuc(string title, int categoryId, string summary, string content, IFormFile thumbnail)
        {
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(content))
            {
                ModelState.AddModelError("", "Vui lòng nhập đầy đủ tiêu đề và nội dung bài viết!");
                ViewBag.DanhMucs = _context.DanhMucs.OrderBy(d => d.TenDanhMuc).ToList();
                return View();
            }

            // Xác định tác giả đăng bài
            int maTacGia = LayMaNguoiDungHienTai();
            if (maTacGia == 0 || _context.NguoiDungs.Find(maTacGia) == null)
            {
                var nguoiDau = _context.NguoiDungs.FirstOrDefault();
                maTacGia = nguoiDau != null ? nguoiDau.MaNguoiDung : 1;
            }

            // Xác định danh mục hợp lệ
            if (categoryId <= 0)
            {
                var dmDau = _context.DanhMucs.FirstOrDefault();
                categoryId = dmDau != null ? dmDau.MaDanhMuc : 1;
            }

            // Xử lý lưu ảnh đại diện nếu có tải lên
            string duongDanAnh = LuuAnhDaiDien(thumbnail);

            string tomTat = "";
            if (!string.IsNullOrWhiteSpace(summary))
            {
                tomTat = summary.Trim();
            }

            var baiVietMoi = new BaiViet
            {
                TieuDe = title.Trim(),
                MaDanhMuc = categoryId,
                TomTat = tomTat,
                NoiDung = content,
                AnhDaiDien = duongDanAnh,
                MaTacGia = maTacGia,
                TrangThai = "da_dang",
                LuotXem = 1,
                NgayTao = DateTime.Now
            };

            _context.BaiViets.Add(baiVietMoi);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Đăng bài viết mới thành công!";
            return RedirectToAction(nameof(DanhSachTinTuc));
        }

        /// <summary>
        /// Mở form chỉnh sửa bài viết đã có (chỉ Admin).
        /// GET: /Post/SuaTinTuc/5
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin,quan_tri")]
        public IActionResult SuaTinTuc(int id)
        {
            var baiViet = _context.BaiViets.Include(b => b.DanhMuc).FirstOrDefault(b => b.MaBaiViet == id);
            if (baiViet == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy bài viết cần chỉnh sửa!";
                return RedirectToAction(nameof(DanhSachTinTuc));
            }
            ViewBag.DanhMucs = _context.DanhMucs.OrderBy(d => d.TenDanhMuc).ToList();
            return View(baiViet);
        }

        /// <summary>
        /// Lưu các thay đổi của bài viết vào CSDL (chỉ Admin).
        /// POST: /Post/SuaTinTuc
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,quan_tri")]
        public IActionResult SuaTinTuc(int id, string title, int categoryId, string summary, string content, string status, IFormFile thumbnail)
        {
            var baiViet = _context.BaiViets.Find(id);
            if (baiViet == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy bài viết cần cập nhật!";
                return RedirectToAction(nameof(DanhSachTinTuc));
            }

            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(content))
            {
                ModelState.AddModelError("", "Vui lòng nhập đầy đủ tiêu đề và nội dung bài viết!");
                ViewBag.DanhMucs = _context.DanhMucs.OrderBy(d => d.TenDanhMuc).ToList();
                return View(baiViet);
            }

            baiViet.TieuDe = title.Trim();
            baiViet.MaDanhMuc = categoryId > 0 ? categoryId : 1;

            if (!string.IsNullOrWhiteSpace(summary))
            {
                baiViet.TomTat = summary.Trim();
            }

            baiViet.NoiDung = content;
            if (!string.IsNullOrEmpty(status))
            {
                baiViet.TrangThai = status;
            }

            // Cập nhật ảnh đại diện mới nếu có tải lên
            if (thumbnail != null && thumbnail.Length > 0)
            {
                string duongDanAnh = LuuAnhDaiDien(thumbnail);
                if (!string.IsNullOrEmpty(duongDanAnh))
                {
                    baiViet.AnhDaiDien = duongDanAnh;
                }
            }

            _context.SaveChanges();

            TempData["SuccessMessage"] = "Cập nhật bài viết thành công!";
            return RedirectToAction(nameof(ChiTiet), new { id });
        }

        /// <summary>
        /// Xóa bài viết khỏi CSDL (chỉ Admin).
        /// POST: /Post/XoaTinTuc
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,quan_tri")]
        public IActionResult XoaTinTuc(int id)
        {
            var baiViet = _context.BaiViets.Find(id);
            if (baiViet != null)
            {
                _context.BaiViets.Remove(baiViet);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Đã xóa bài viết thành công!";
            }
            return RedirectToAction(nameof(DanhSachTinTuc));
        }

        #endregion

        #region 3. PHƯƠNG THỨC TIỆN ÍCH DÙNG CHUNG

        private int LayMaNguoiDungHienTai()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var idClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (idClaim != null && int.TryParse(idClaim.Value, out int id))
                {
                    return id;
                }
            }
            return 0;
        }

        /// <summary>
        /// Khởi tạo dữ liệu mẫu về danh mục và bài viết nếu CSDL chưa có dữ liệu.
        /// </summary>
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

                var dmDau = _context.DanhMucs.FirstOrDefault();
                int maDanhMuc = dmDau != null ? dmDau.MaDanhMuc : 1;

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

        /// <summary>
        /// Lưu file ảnh tải lên vào thư mục wwwroot/uploads/posts/
        /// </summary>
        private string LuuAnhDaiDien(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return "";
            }

            try
            {
                var thuMucUpload = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "posts");
                if (!Directory.Exists(thuMucUpload))
                {
                    Directory.CreateDirectory(thuMucUpload);
                }

                var tenFile = Guid.NewGuid().ToString("N") + Path.GetExtension(file.FileName);
                var duongDanVatLy = Path.Combine(thuMucUpload, tenFile);

                using (var stream = new FileStream(duongDanVatLy, FileMode.Create))
                {
                    file.CopyTo(stream);
                }

                return "/uploads/posts/" + tenFile;
            }
            catch
            {
                return "";
            }
        }

        #endregion
    }
}
