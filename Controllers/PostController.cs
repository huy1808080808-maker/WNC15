using Microsoft.AspNetCore.Mvc;

namespace WNC15.Controllers
{
    // Controller quản lý Tin tức và Bài viết
    public class PostController : Controller
    {
        // 1. Danh sách tin tức (GET: /Post/DanhSachTinTuc)
        public IActionResult DanhSachTinTuc()
        {
            return View(); // Trả về Views/Post/DanhSachTinTuc.cshtml
        }

        // 2. Chi tiết một bài viết (GET: /Post/ChiTiet/5)
        public IActionResult ChiTiet(int id)
        {
            return View(); // Trả về Views/Post/ChiTiet.cshtml
        }

        // 3. Mở form đăng bài viết mới (GET: /Post/ThemTinTuc)
        public IActionResult ThemTinTuc()
        {
            return View(); // Trả về Views/Post/ThemTinTuc.cshtml
        }

        // 4. Xử lý lưu bài viết mới (POST: /Post/ThemTinTuc)
        [HttpPost]
        public IActionResult ThemTinTuc(string title, string content)
        {
            TempData["SuccessMessage"] = "Đăng bài viết mới thành công!";
            return RedirectToAction("DanhSachTinTuc");
        }

        // 5. Mở form chỉnh sửa bài viết (GET: /Post/ChinhSua/5)
        public IActionResult ChinhSua(int id)
        {
            return View(); // Trả về Views/Post/ChinhSua.cshtml
        }

        // 6. Xử lý cập nhật bài viết (POST: /Post/ChinhSua)
        [HttpPost]
        public IActionResult ChinhSua(int id, string title, string content)
        {
            TempData["SuccessMessage"] = "Cập nhật bài viết thành công!";
            return RedirectToAction("DanhSachTinTuc");
        }

        // 7. Xóa bài viết (POST: /Post/XoaTinTuc)
        [HttpPost]
        public IActionResult XoaTinTuc(int id)
        {
            TempData["SuccessMessage"] = "Đã xóa bài viết thành công!";
            return RedirectToAction("DanhSachTinTuc");
        }
    }
}
