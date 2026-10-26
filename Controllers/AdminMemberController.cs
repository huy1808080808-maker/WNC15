using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WNC15.Data;
using WNC15.Models;

namespace WNC15.Controllers
{
    /// <summary>
    /// Controller dành riêng cho Quản trị viên (Admin):
    /// - Quản lý bài viết & tin tức (Thêm, Sửa, Xóa, Duyệt, Ẩn).
    /// - Quản lý hội viên (Duyệt hồ sơ, Khóa/Mở tài khoản, Xóa).
    /// - Thống kê các chỉ số hoạt động của Hiệp hội.
    /// </summary>
    [Authorize(Roles = "Admin,quan_tri")]
    public class AdminMemberController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminMemberController(ApplicationDbContext context)
        {
            _context = context;
        }

        #region 1. BẢNG ĐIỀU KHIỂN QUẢN TRỊ (ADMIN DASHBOARD)

        /// <summary>
        /// Trang tổng quan: Hiển thị 4 thẻ thống kê KPI, danh sách bài viết kiểm duyệt và hồ sơ mới nộp.
        /// GET: /AdminMember hoặc /AdminMember/BangQuanTri
        /// </summary>
        public IActionResult BangQuanTri()
        {
            // 1. Thống kê số liệu hệ thống từ CSDL
            ViewBag.TongHoiVien = _context.NguoiDungs.Count(u => u.VaiTro == "hoi_vien" && u.TrangThai == "hoat_dong");
            ViewBag.SoHoSoChoDuyet = _context.NguoiDungs.Count(u => u.TrangThai == "cho_duyet");
            ViewBag.TongBaiViet = _context.BaiViets.Count();
            ViewBag.TongSuKien = _context.SuKiens.Count();
            ViewBag.TongLuotDangKy = _context.DangKySuKiens.Count();

            // 2. Lấy tối đa 5 hồ sơ ứng viên mới nhất đang chờ duyệt
            ViewBag.DsChoDuyet = _context.NguoiDungs
                                         .Where(u => u.TrangThai == "cho_duyet")
                                         .OrderByDescending(u => u.NgayTao)
                                         .Take(5)
                                         .ToList();

            // 3. Lấy toàn bộ bài viết để kiểm duyệt
            var dsBaiViet = _context.BaiViets
                                    .Include(b => b.DanhMuc)
                                    .Include(b => b.TacGia)
                                    .OrderByDescending(b => b.NgayTao)
                                    .ToList();

            return View(dsBaiViet);
        }

        #endregion

        #region 2. QUẢN LÝ & PHÊ DUYỆT HỒ SƠ HỘI VIÊN

        /// <summary>
        /// Danh sách toàn bộ đơn đăng ký gia nhập đang chờ phê duyệt.
        /// GET: /AdminMember/HoSoChoDuyet
        /// </summary>
        public IActionResult HoSoChoDuyet()
        {
            var dsChoDuyet = _context.NguoiDungs
                                     .Where(u => u.TrangThai == "cho_duyet")
                                     .OrderByDescending(u => u.NgayTao)
                                     .ToList();

            return View(dsChoDuyet);
        }

        /// <summary>
        /// Danh sách toàn bộ hội viên (tìm kiếm theo họ tên, email, đơn vị công tác).
        /// GET: /AdminMember/DanhSachHoiVien
        /// </summary>
        public IActionResult DanhSachHoiVien(string keyword)
        {
            var query = _context.NguoiDungs.Where(u => u.TrangThai != "cho_duyet");

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim().ToLower();
                query = query.Where(u => u.HoTen.ToLower().Contains(keyword)
                                      || u.Email.ToLower().Contains(keyword)
                                      || (u.DonViCongTac != null && u.DonViCongTac.ToLower().Contains(keyword)));
            }

            var dsHoiVien = query.OrderByDescending(u => u.NgayTao).ToList();

            ViewBag.SoHoSoChoDuyet = _context.NguoiDungs.Count(u => u.TrangThai == "cho_duyet");
            ViewBag.Keyword = keyword;

            return View(dsHoiVien);
        }

        /// <summary>
        /// Phê duyệt đơn gia nhập của thành viên mới -> Chuyển sang 'hoat_dong'.
        /// POST: /AdminMember/PheDuyet (Hỗ trợ AJAX & Form)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult PheDuyet(int id)
        {
            var hoiVien = _context.NguoiDungs.Find(id);
            if (hoiVien == null)
            {
                return IsAjaxRequest()
                    ? Json(new { success = false, message = "Không tìm thấy hồ sơ hội viên cần phê duyệt." })
                    : (IActionResult)RedirectToAction(nameof(HoSoChoDuyet));
            }

            hoiVien.TrangThai = "hoat_dong";
            hoiVien.LyDoTuChoi = null;
            _context.SaveChanges();

            int soConLai = _context.NguoiDungs.Count(u => u.TrangThai == "cho_duyet");

            if (IsAjaxRequest())
            {
                return Json(new
                {
                    success = true,
                    message = $"Đã phê duyệt thành công hội viên: {hoiVien.HoTen}!",
                    id,
                    soHoSoChoDuyet = soConLai
                });
            }

            TempData["SuccessMessage"] = $"Đã phê duyệt thành công hội viên: {hoiVien.HoTen}!";
            return RedirectToAction(nameof(HoSoChoDuyet));
        }

        /// <summary>
        /// Từ chối đơn đăng ký kèm lý do.
        /// POST: /AdminMember/TuChoi (Hỗ trợ AJAX & Form)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult TuChoi(int id, string lyDo)
        {
            var hoiVien = _context.NguoiDungs.Find(id);
            if (hoiVien == null)
            {
                return IsAjaxRequest()
                    ? Json(new { success = false, message = "Không tìm thấy hồ sơ hội viên." })
                    : (IActionResult)RedirectToAction(nameof(HoSoChoDuyet));
            }

            hoiVien.TrangThai = "tu_choi";
            hoiVien.LyDoTuChoi = string.IsNullOrWhiteSpace(lyDo) ? "Hồ sơ chưa đáp ứng yêu cầu gia nhập" : lyDo.Trim();
            _context.SaveChanges();

            int soConLai = _context.NguoiDungs.Count(u => u.TrangThai == "cho_duyet");

            if (IsAjaxRequest())
            {
                return Json(new
                {
                    success = true,
                    message = $"Đã từ chối đơn đăng ký của ứng viên: {hoiVien.HoTen}.",
                    id,
                    soHoSoChoDuyet = soConLai
                });
            }

            TempData["SuccessMessage"] = $"Đã từ chối đơn đăng ký của ứng viên: {hoiVien.HoTen}.";
            return RedirectToAction(nameof(HoSoChoDuyet));
        }

        #endregion

        #region 3. KHÓA, MỞ KHÓA & XÓA TÀI KHOẢN HỘI VIÊN

        /// <summary>
        /// Tạm khóa tài khoản hội viên vi phạm.
        /// POST: /AdminMember/KhoaTaiKhoan (Hỗ trợ AJAX & Form)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult KhoaTaiKhoan(int id)
        {
            var hoiVien = _context.NguoiDungs.Find(id);
            if (hoiVien == null)
            {
                return IsAjaxRequest()
                    ? Json(new { success = false, message = "Không tìm thấy hội viên!" })
                    : (IActionResult)RedirectToAction(nameof(DanhSachHoiVien));
            }

            hoiVien.TrangThai = "khoa";
            _context.SaveChanges();

            if (IsAjaxRequest())
            {
                return Json(new { success = true, message = $"Đã khóa tài khoản của hội viên: {hoiVien.HoTen}!", id, trangThai = "khoa" });
            }

            TempData["SuccessMessage"] = $"Đã khóa tài khoản của hội viên: {hoiVien.HoTen}.";
            return RedirectToAction(nameof(DanhSachHoiVien));
        }

        /// <summary>
        /// Mở khóa tài khoản cho hội viên trở lại hoạt động bình thường.
        /// POST: /AdminMember/MoKhoaTaiKhoan (Hỗ trợ AJAX & Form)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MoKhoaTaiKhoan(int id)
        {
            var hoiVien = _context.NguoiDungs.Find(id);
            if (hoiVien == null)
            {
                return IsAjaxRequest()
                    ? Json(new { success = false, message = "Không tìm thấy hội viên!" })
                    : (IActionResult)RedirectToAction(nameof(DanhSachHoiVien));
            }

            hoiVien.TrangThai = "hoat_dong";
            _context.SaveChanges();

            if (IsAjaxRequest())
            {
                return Json(new { success = true, message = $"Đã mở khóa tài khoản của hội viên: {hoiVien.HoTen}!", id, trangThai = "hoat_dong" });
            }

            TempData["SuccessMessage"] = $"Đã mở khóa tài khoản của hội viên: {hoiVien.HoTen}.";
            return RedirectToAction(nameof(DanhSachHoiVien));
        }

        /// <summary>
        /// Xóa vĩnh viễn tài khoản hội viên khỏi hệ thống.
        /// POST: /AdminMember/XoaTaiKhoan (Hỗ trợ AJAX & Form)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult XoaTaiKhoan(int id)
        {
            var hoiVien = _context.NguoiDungs.Find(id);
            if (hoiVien == null)
            {
                return IsAjaxRequest()
                    ? Json(new { success = false, message = "Không tìm thấy hội viên!" })
                    : (IActionResult)RedirectToAction(nameof(DanhSachHoiVien));
            }

            _context.NguoiDungs.Remove(hoiVien);
            _context.SaveChanges();

            if (IsAjaxRequest())
            {
                return Json(new { success = true, message = $"Đã xóa hồ sơ hội viên: {hoiVien.HoTen} khỏi hệ thống!", id });
            }

            TempData["SuccessMessage"] = $"Đã xóa hồ sơ hội viên: {hoiVien.HoTen} khỏi hệ thống.";
            return RedirectToAction(nameof(DanhSachHoiVien));
        }

        #endregion

        #region 4. KIỂM DUYỆT & QUẢN LÝ BÀI VIẾT (TIN TỨC)

        /// <summary>
        /// Duyệt & xuất bản bài viết ra trang công khai ('da_dang').
        /// POST: /AdminMember/DuyetBaiViet (Hỗ trợ AJAX & Form)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DuyetBaiViet(int id)
        {
            var baiViet = _context.BaiViets.Find(id);
            if (baiViet == null)
            {
                return IsAjaxRequest()
                    ? Json(new { success = false, message = "Không tìm thấy bài viết!" })
                    : (IActionResult)RedirectToAction(nameof(BangQuanTri));
            }

            baiViet.TrangThai = "da_dang";
            _context.SaveChanges();

            if (IsAjaxRequest())
            {
                return Json(new { success = true, message = $"Đã xuất bản bài viết: {baiViet.TieuDe}!", id, trangThai = "da_dang" });
            }

            TempData["SuccessMessage"] = "Đã xuất bản bài viết thành công!";
            return RedirectToAction(nameof(BangQuanTri));
        }

        /// <summary>
        /// Tạm ẩn bài viết khỏi trang công khai ('an').
        /// POST: /AdminMember/AnBaiViet (Hỗ trợ AJAX & Form)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AnBaiViet(int id)
        {
            var baiViet = _context.BaiViets.Find(id);
            if (baiViet == null)
            {
                return IsAjaxRequest()
                    ? Json(new { success = false, message = "Không tìm thấy bài viết!" })
                    : (IActionResult)RedirectToAction(nameof(BangQuanTri));
            }

            baiViet.TrangThai = "an";
            _context.SaveChanges();

            if (IsAjaxRequest())
            {
                return Json(new { success = true, message = $"Đã ẩn bài viết: {baiViet.TieuDe}!", id, trangThai = "an" });
            }

            TempData["SuccessMessage"] = "Đã ẩn bài viết thành công!";
            return RedirectToAction(nameof(BangQuanTri));
        }

        /// <summary>
        /// Xóa bài viết khỏi cơ sở dữ liệu.
        /// POST: /AdminMember/XoaBaiViet (Hỗ trợ AJAX & Form)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult XoaBaiViet(int id)
        {
            var baiViet = _context.BaiViets.Find(id);
            if (baiViet == null)
            {
                return IsAjaxRequest()
                    ? Json(new { success = false, message = "Không tìm thấy bài viết!" })
                    : (IActionResult)RedirectToAction(nameof(BangQuanTri));
            }

            _context.BaiViets.Remove(baiViet);
            _context.SaveChanges();

            if (IsAjaxRequest())
            {
                return Json(new { success = true, message = "Đã xóa bài viết thành công!", id });
            }

            TempData["SuccessMessage"] = "Đã xóa bài viết thành công!";
            return RedirectToAction(nameof(BangQuanTri));
        }

        #endregion

        #region 5. PHƯƠNG THỨC HỖ TRỢ DÙNG CHUNG

        /// <summary>
        /// Kiểm tra yêu cầu gửi đến có phải là AJAX hay không.
        /// </summary>
        private bool IsAjaxRequest()
        {
            return Request.Headers["X-Requested-With"] == "XMLHttpRequest"
                || Request.Headers.Accept.ToString().Contains("application/json");
        }

        #endregion
    }
}
