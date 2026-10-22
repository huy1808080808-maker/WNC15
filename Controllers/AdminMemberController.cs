using Microsoft.AspNetCore.Mvc;

namespace WNC15.Controllers
{
    // Controller dành cho Quản trị viên quản lý và duyệt hội viên
    public class AdminMemberController : Controller
    {
        // 1. Danh sách hồ sơ đang chờ duyệt (GET: /AdminMember/HoSoChoDuyet)
        public IActionResult HoSoChoDuyet()
        {
            return View(); // Trả về Views/AdminMember/HoSoChoDuyet.cshtml
        }

        // 2. Danh sách toàn bộ hội viên trong hệ thống (GET: /AdminMember/DanhSachHoiVien)
        public IActionResult DanhSachHoiVien()
        {
            return View(); // Trả về Views/AdminMember/DanhSachHoiVien.cshtml
        }

        // 3. Phê duyệt hồ sơ hội viên (POST: /AdminMember/PheDuyet)
        [HttpPost]
        public IActionResult PheDuyet(int id)
        {
            TempData["SuccessMessage"] = "Đã phê duyệt hồ sơ thành công!";
            return RedirectToAction("HoSoChoDuyet");
        }

        // 4. Từ chối hồ sơ kèm lý do (POST: /AdminMember/TuChoi)
        [HttpPost]
        public IActionResult TuChoi(int id, string lyDo)
        {
            TempData["SuccessMessage"] = "Đã từ chối hồ sơ hội viên.";
            return RedirectToAction("HoSoChoDuyet");
        }

        // 5. Khóa tài khoản hội viên vi phạm (POST: /AdminMember/KhoaTaiKhoan)
        [HttpPost]
        public IActionResult KhoaTaiKhoan(int id)
        {
            TempData["SuccessMessage"] = "Đã khóa tài khoản thành công.";
            return RedirectToAction("DanhSachHoiVien");
        }

        // 6. Mở khóa tài khoản (POST: /AdminMember/MoKhoaTaiKhoan)
        [HttpPost]
        public IActionResult MoKhoaTaiKhoan(int id)
        {
            TempData["SuccessMessage"] = "Đã mở khóa tài khoản thành công.";
            return RedirectToAction("DanhSachHoiVien");
        }

        // 7. Xóa tài khoản (POST: /AdminMember/XoaTaiKhoan)
        [HttpPost]
        public IActionResult XoaTaiKhoan(int id)
        {
            TempData["SuccessMessage"] = "Đã xóa tài khoản thành công.";
            return RedirectToAction("DanhSachHoiVien");
        }
    }
}
