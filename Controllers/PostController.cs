using Microsoft.AspNetCore.Mvc;

namespace WNC15.Controllers
{
    public class PostController : Controller
    {
        private readonly ILogger<PostController> _logger;

        public PostController(ILogger<PostController> logger)
        {
            _logger = logger;
        }

        // GET: /Post
        // GET: /Post/Index
        [HttpGet]
        public async Task<IActionResult> Index(string? search, int? categoryId, int page = 1)
        {
            ViewBag.SearchTerm = search;
            ViewBag.CategoryId = categoryId;
            ViewBag.CurrentPage = page;

            // TODO: Nghiệp vụ P02 - Lấy danh sách tin tức có tìm kiếm, lọc theo danh mục và phân trang
            await Task.CompletedTask;
            return View();
        }

        // GET: /Post/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            if (id <= 0)
            {
                return NotFound();
            }

            // TODO: Nghiệp vụ P02 - Xem chi tiết nội dung bài viết, tác giả, ngày đăng, lượt xem
            await Task.CompletedTask;
            return View();
        }

        // GET: /Post/Create (Dành cho Admin)
        [HttpGet]
        public IActionResult Create()
        {
            // TODO: Kiểm tra quyền Admin (hoặc [Authorize(Roles = "Admin")])
            // Lấy danh sách chuyên mục tin tức để đổ vào dropdown
            return View();
        }

        // POST: /Post/Create (Dành cho Admin - BM04)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string title, string summary, string content, int categoryId, IFormFile? thumbnail)
        {
            // TODO: Kiểm tra quyền Admin
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(content))
            {
                ModelState.AddModelError(string.Empty, "Tiêu đề và nội dung bài viết không được để trống.");
                return View();
            }

            // TODO: Nghiệp vụ P02 - Tạo bài viết mới kèm upload ảnh đại diện theo biểu mẫu BM04:
            // 1. Upload file ảnh đại diện (thumbnail) vào thư mục /uploads/posts/
            // 2. Lưu bài viết vào CSDL với trạng thái đã xuất bản
            await Task.CompletedTask;

            TempData["SuccessMessage"] = "Đăng tin tức mới thành công!";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Post/Edit/5 (Dành cho Admin)
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            // TODO: Kiểm tra quyền Admin
            if (id <= 0)
            {
                return NotFound();
            }

            // TODO: Lấy thông tin bài viết theo id để hiển thị lên form chỉnh sửa
            await Task.CompletedTask;
            return View();
        }

        // POST: /Post/Edit/5 (Dành cho Admin)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string title, string summary, string content, int categoryId, IFormFile? thumbnail)
        {
            // TODO: Kiểm tra quyền Admin
            if (id <= 0)
            {
                return BadRequest();
            }

            // TODO: Nghiệp vụ P02 - Cập nhật bài viết và thay thế ảnh đại diện mới nếu có upload
            await Task.CompletedTask;

            TempData["SuccessMessage"] = "Cập nhật bài viết thành công!";
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: /Post/Delete/5 (Dành cho Admin)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            // TODO: Kiểm tra quyền Admin
            if (id <= 0)
            {
                return BadRequest();
            }

            // TODO: Nghiệp vụ P02 - Xóa bài viết khỏi cơ sở dữ liệu
            await Task.CompletedTask;

            TempData["SuccessMessage"] = "Đã xóa bài viết thành công.";
            return RedirectToAction(nameof(Index));
        }
    }
}
