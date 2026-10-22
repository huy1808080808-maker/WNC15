using Microsoft.AspNetCore.Mvc;

namespace WNC15.Controllers
{
    // Controller quản lý Hồ sơ cá nhân của Hội viên
    public class ProfileController : Controller
    {
        // 1. Xem thông tin cá nhân (GET: /Profile/ThongTinCaNhan)
        public IActionResult ThongTinCaNhan()
        {
            return View(); // Trả về Views/Profile/ThongTinCaNhan.cshtml
        }

        // 2. Mở form chỉnh sửa thông tin (GET: /Profile/ChinhSua)
        public IActionResult ChinhSua()
        {
            return View(); // Trả về Views/Profile/ChinhSua.cshtml
        }

        // 3. Xử lý lưu thông tin chỉnh sửa (POST: /Profile/ChinhSua)
        [HttpPost]
        public IActionResult ChinhSua(string fullName, string phoneNumber, string address)
        {
            TempData["SuccessMessage"] = "Cập nhật thông tin cá nhân thành công!";
            return RedirectToAction("ThongTinCaNhan");
        }

        // 4. Mở form đổi mật khẩu (GET: /Profile/DoiMatKhau)
        public IActionResult DoiMatKhau()
        {
            return View(); // Trả về Views/Profile/DoiMatKhau.cshtml
        }

        // 5. Xử lý đổi mật khẩu (POST: /Profile/DoiMatKhau)
        [HttpPost]
        public IActionResult DoiMatKhau(string currentPassword, string newPassword)
        {
            TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";
            return RedirectToAction("ThongTinCaNhan");
        }

        // 6. Xem các sự kiện mình đã đăng ký (GET: /Profile/SuKienCuaToi)
        public IActionResult SuKienCuaToi()
        {
            return View(); // Trả về Views/Profile/SuKienCuaToi.cshtml
        }
    }
}
