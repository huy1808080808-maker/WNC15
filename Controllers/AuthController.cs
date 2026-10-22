using Microsoft.AspNetCore.Mvc;

namespace WNC15.Controllers
{
    // Controller xử lý Đăng nhập, Đăng ký và Trạng thái tài khoản
    public class AuthController : Controller
    {
        // 1. Giao diện Đăng nhập (GET: /Auth/DangNhap)
        [HttpGet]
        public IActionResult DangNhap()
        {
            return View(); // Trả về Views/Auth/DangNhap.cshtml
        }

        // 2. Xử lý khi bấm nút Đăng nhập (POST: /Auth/DangNhap)
        [HttpPost]
        public IActionResult DangNhap(string emailOrUsername, string password)
        {
            if (string.IsNullOrEmpty(emailOrUsername) || string.IsNullOrEmpty(password))
            {
                ViewBag.Loi = "Vui lòng nhập đầy đủ tài khoản và mật khẩu!";
                return View();
            }

            // Đăng nhập thành công -> chuyển về Trang chủ
            return RedirectToAction("Index", "Home");
        }

        // 3. Giao diện Đơn đăng ký gia nhập (GET: /Auth/DangKy)
        [HttpGet]
        public IActionResult DangKy()
        {
            return View(); // Trả về Views/Auth/DangKy.cshtml
        }

        // 4. Xử lý khi gửi đơn đăng ký (POST: /Auth/DangKy)
        [HttpPost]
        public IActionResult DangKy(string username, string fullName, string email, string password)
        {
            // Báo thành công và chuyển sang trang Đăng nhập
            TempData["SuccessMessage"] = "Gửi đơn đăng ký thành công! Hồ sơ của bạn đang chờ ban quản trị duyệt.";
            return RedirectToAction("DangNhap");
        }

        // 5. Đăng xuất khỏi hệ thống (POST: /Auth/DangXuat)
        [HttpPost]
        public IActionResult DangXuat()
        {
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
        public IActionResult TrangThaiHoSo(string? status, string? reason)
        {
            ViewBag.Status = status ?? "cho_duyet";
            ViewBag.Reason = reason;
            return View(); // Trả về Views/Auth/TrangThaiHoSo.cshtml
        }
    }
}
