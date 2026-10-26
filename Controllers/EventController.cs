using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WNC15.Data;
using WNC15.Models;

namespace WNC15.Controllers
{
    /// <summary>
    /// Controller quản lý Sự kiện & Hội thảo:
    /// - Xem danh sách, chi tiết sự kiện.
    /// - Đăng ký / Hủy đăng ký giữ chỗ (AJAX).
    /// - Quản trị viên: Tạo sự kiện mới, Xóa sự kiện, Xem danh sách người đăng ký (Điểm danh).
    /// </summary>
    public class EventController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EventController(ApplicationDbContext context)
        {
            _context = context;
        }

        #region 1. XEM DANH SÁCH & CHI TIẾT SỰ KIỆN

        /// <summary>
        /// Danh sách toàn bộ sự kiện & hội thảo khoa học (hỗ trợ tìm kiếm theo tên hoặc địa điểm).
        /// GET: /Event/DanhSachSuKien
        /// </summary>
        public IActionResult DanhSachSuKien(string search)
        {
            var query = _context.SuKiens
                                .Include(s => s.DangKySuKiens)
                                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                query = query.Where(s => s.TenSuKien.Contains(search) || s.DiaDiem.Contains(search));
                ViewBag.Search = search;
            }

            var danhSach = query.OrderByDescending(s => s.ThoiGianToChuc).ToList();
            return View(danhSach);
        }

        /// <summary>
        /// Xem thông tin chi tiết một sự kiện và tình trạng chỗ ngồi còn lại.
        /// GET: /Event/ChiTiet/5
        /// </summary>
        public IActionResult ChiTiet(int id)
        {
            var suKien = _context.SuKiens
                                 .Include(s => s.DangKySuKiens)
                                 .FirstOrDefault(s => s.MaSuKien == id);

            // Tự động tạo sự kiện mẫu nếu cơ sở dữ liệu chưa có sự kiện nào
            if (suKien == null)
            {
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

            // Kiểm tra xem hội viên đang đăng nhập đã đăng ký sự kiện này chưa
            int maNguoiDung = LayMaNguoiDungHienTai();
            ViewBag.DaDangKy = (maNguoiDung > 0) && suKien.DangKySuKiens.Any(d => d.MaNguoiDung == maNguoiDung);

            return View(suKien);
        }

        #endregion

        #region 2. ĐĂNG KÝ & HỦY THAM GIA SỰ KIỆN (AJAX & FORM)

        /// <summary>
        /// Hội viên bấm nút "Đăng ký tham gia": Kiểm tra điều kiện và ghi nhận giữ chỗ vào CSDL.
        /// POST: /Event/DangKyThamGia (Hỗ trợ AJAX & Form)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DangKyThamGia(int id)
        {
            var suKien = _context.SuKiens.Include(s => s.DangKySuKiens).FirstOrDefault(s => s.MaSuKien == id);
            if (suKien == null)
            {
                return IsAjaxRequest()
                    ? Json(new { success = false, message = "Không tìm thấy thông tin sự kiện này." })
                    : (IActionResult)RedirectToAction(nameof(DanhSachSuKien));
            }

            // 1. Kiểm tra hạn chót đăng ký
            if (DateTime.Now > suKien.HanDangKy)
            {
                return IsAjaxRequest()
                    ? Json(new { success = false, message = "Rất tiếc, sự kiện này đã kết thúc thời hạn đăng ký tham dự!" })
                    : RedirectWithError(nameof(ChiTiet), id, "Rất tiếc, sự kiện này đã kết thúc thời hạn đăng ký tham dự!");
            }

            // 2. Kiểm tra số chỗ còn trống
            if (suKien.DangKySuKiens.Count >= suKien.SoLuongToiDa)
            {
                return IsAjaxRequest()
                    ? Json(new { success = false, message = "Sự kiện này đã đủ số lượng người đăng ký tối đa!" })
                    : RedirectWithError(nameof(ChiTiet), id, "Sự kiện này đã đủ số lượng người đăng ký tối đa!");
            }

            // 3. Kiểm tra đăng nhập
            int maNguoiDung = LayMaNguoiDungHienTai();
            if (maNguoiDung == 0)
            {
                if (IsAjaxRequest())
                {
                    return Json(new { success = false, message = "Vui lòng đăng nhập tài khoản để đăng ký tham gia sự kiện!" });
                }
                TempData["ErrorMessage"] = "Vui lòng đăng nhập tài khoản để đăng ký tham gia sự kiện!";
                return RedirectToAction("DangNhap", "Auth", new { returnUrl = Url.Action(nameof(ChiTiet), new { id }) });
            }

            // 4. Kiểm tra đã đăng ký trước đó chưa
            bool daDangKy = _context.DangKySuKiens.Any(d => d.MaSuKien == id && d.MaNguoiDung == maNguoiDung);
            if (daDangKy)
            {
                return IsAjaxRequest()
                    ? Json(new { success = false, message = "Bạn đã đăng ký tham gia sự kiện này trước đó rồi!" })
                    : RedirectWithError(nameof(ChiTiet), id, "Bạn đã đăng ký tham gia sự kiện này trước đó rồi!");
            }

            // 5. Thêm bản ghi đăng ký mới vào CSDL
            var dangKyMoi = new DangKySuKien
            {
                MaSuKien = id,
                MaNguoiDung = maNguoiDung,
                NgayDangKy = DateTime.Now,
                TrangThai = "da_dang_ky"
            };
            _context.DangKySuKiens.Add(dangKyMoi);
            _context.SaveChanges();

            int tongDaDangKy = _context.DangKySuKiens.Count(d => d.MaSuKien == id);
            int conLai = Math.Max(0, suKien.SoLuongToiDa - tongDaDangKy);

            if (IsAjaxRequest())
            {
                return Json(new
                {
                    success = true,
                    message = "Đăng ký giữ chỗ thành công! Bạn đã có tên trong danh sách điểm danh.",
                    isRegistered = true,
                    soLuongDaDangKy = tongDaDangKy,
                    soChoConLai = conLai,
                    soLuongToiDa = suKien.SoLuongToiDa
                });
            }

            TempData["SuccessMessage"] = "Đăng ký giữ chỗ thành công! Bạn đã có tên trong danh sách điểm danh.";
            return RedirectToAction(nameof(ChiTiet), new { id });
        }

        /// <summary>
        /// Hội viên bấm nút "Hủy đăng ký": Hủy chỗ và hoàn lại số lượng chỗ trống vào CSDL.
        /// POST: /Event/HuyDangKy (Hỗ trợ AJAX & Form)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult HuyDangKy(int id)
        {
            var suKien = _context.SuKiens.Find(id);
            int maNguoiDung = LayMaNguoiDungHienTai();

            if (maNguoiDung == 0)
            {
                if (IsAjaxRequest())
                {
                    return Json(new { success = false, message = "Vui lòng đăng nhập tài khoản để thực hiện thao tác này!" });
                }
                TempData["ErrorMessage"] = "Vui lòng đăng nhập tài khoản để thực hiện thao tác này!";
                return RedirectToAction("DangNhap", "Auth", new { returnUrl = Url.Action(nameof(ChiTiet), new { id }) });
            }

            var dangKy = _context.DangKySuKiens.FirstOrDefault(d => d.MaSuKien == id && d.MaNguoiDung == maNguoiDung);
            if (dangKy == null)
            {
                return IsAjaxRequest()
                    ? Json(new { success = false, message = "Bạn chưa đăng ký tham gia sự kiện này." })
                    : RedirectWithError(nameof(ChiTiet), id, "Bạn chưa đăng ký tham gia sự kiện này.");
            }

            _context.DangKySuKiens.Remove(dangKy);
            _context.SaveChanges();

            int tongDaDangKy = _context.DangKySuKiens.Count(d => d.MaSuKien == id);
            int soToiDa = suKien?.SoLuongToiDa ?? 100;
            int conLai = Math.Max(0, soToiDa - tongDaDangKy);

            if (IsAjaxRequest())
            {
                return Json(new
                {
                    success = true,
                    message = "Đã hủy đăng ký thành công. Số lượng chỗ trống đã được hoàn lại.",
                    isRegistered = false,
                    soLuongDaDangKy = tongDaDangKy,
                    soChoConLai = conLai,
                    soLuongToiDa = soToiDa
                });
            }

            TempData["SuccessMessage"] = "Đã hủy đăng ký thành công. Số lượng chỗ trống đã được hoàn lại.";
            return RedirectToAction(nameof(ChiTiet), new { id });
        }

        #endregion

        #region 3. CHỨC NĂNG QUẢN LÝ SỰ KIỆN CHO QUẢN TRỊ VIÊN

        /// <summary>
        /// Mở form tạo sự kiện mới (chỉ Admin).
        /// GET: /Event/ThemSuKien
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin,quan_tri")]
        public IActionResult ThemSuKien()
        {
            return View();
        }

        /// <summary>
        /// Lưu thông tin sự kiện mới vào CSDL.
        /// POST: /Event/ThemSuKien
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,quan_tri")]
        public IActionResult ThemSuKien(string title, string location, int maxParticipants,
                                        string startDate, string deadlineDate, string description)
        {
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(location))
            {
                ViewBag.ErrorMessage = "Vui lòng nhập đầy đủ tên sự kiện và địa điểm tổ chức!";
                return View();
            }

            DateTime ngayToChuc = DateTime.TryParse(startDate, out var dtBatDau) ? dtBatDau : DateTime.Now.AddDays(7);
            DateTime hanDangKy = DateTime.TryParse(deadlineDate, out var dtHan) ? dtHan : ngayToChuc.AddDays(-1);

            int maNguoiTao = LayMaNguoiDungHienTai();
            if (maNguoiTao == 0 || _context.NguoiDungs.Find(maNguoiTao) == null)
            {
                var nguoiDau = _context.NguoiDungs.FirstOrDefault();
                maNguoiTao = nguoiDau != null ? nguoiDau.MaNguoiDung : 1;
            }

            var suKienMoi = new SuKien
            {
                TenSuKien = title.Trim(),
                DiaDiem = location.Trim(),
                SoLuongToiDa = maxParticipants > 0 ? maxParticipants : 100,
                ThoiGianToChuc = ngayToChuc,
                HanDangKy = hanDangKy,
                MoTa = description,
                MaNguoiTao = maNguoiTao,
                NgayTao = DateTime.Now
            };

            try
            {
                _context.SuKiens.Add(suKienMoi);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Tạo và phát hành sự kiện mới thành công!";
                return RedirectToAction(nameof(DanhSachSuKien));
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Lỗi khi lưu sự kiện: " + ex.Message;
                return View();
            }
        }

        /// <summary>
        /// Xem danh sách hội viên đã đăng ký tham gia sự kiện (Danh sách điểm danh).
        /// GET: /Event/DanhSachDiemDanh/5
        /// </summary>
        public IActionResult DanhSachDiemDanh(int id)
        {
            var danhSach = _context.DangKySuKiens
                                   .Include(d => d.NguoiDung)
                                   .Include(d => d.SuKien)
                                   .Where(d => d.MaSuKien == id)
                                   .OrderByDescending(d => d.NgayDangKy)
                                   .ToList();

            var suKien = _context.SuKiens.Find(id);
            ViewBag.TenSuKien = suKien != null ? suKien.TenSuKien : "Hội thảo Khoa học";
            ViewBag.MaSuKien = id;

            return View(danhSach);
        }

        /// <summary>
        /// Xóa sự kiện và các lượt đăng ký liên quan (chỉ Admin).
        /// POST: /Event/XoaSuKien
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,quan_tri")]
        public IActionResult XoaSuKien(int id)
        {
            var suKien = _context.SuKiens.Find(id);
            if (suKien != null)
            {
                var dsDangKy = _context.DangKySuKiens.Where(d => d.MaSuKien == id).ToList();
                if (dsDangKy.Count > 0)
                {
                    _context.DangKySuKiens.RemoveRange(dsDangKy);
                }

                _context.SuKiens.Remove(suKien);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Đã xóa sự kiện thành công!";
            }

            return RedirectToAction(nameof(DanhSachSuKien));
        }

        #endregion

        #region 4. PHƯƠNG THỨC TIỆN ÍCH DÙNG CHUNG

        private bool IsAjaxRequest()
        {
            return Request.Headers["X-Requested-With"] == "XMLHttpRequest"
                || Request.Headers.Accept.ToString().Contains("application/json");
        }

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

        private IActionResult RedirectWithError(string actionName, int id, string message)
        {
            TempData["ErrorMessage"] = message;
            return RedirectToAction(actionName, new { id });
        }

        #endregion
    }
}
