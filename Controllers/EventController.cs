using Microsoft.AspNetCore.Mvc;

namespace WNC15.Controllers
{
    public class EventController : Controller
    {
        private readonly ILogger<EventController> _logger;

        public EventController(ILogger<EventController> logger)
        {
            _logger = logger;
        }

        // GET: /Event
        // GET: /Event/Index
        [HttpGet]
        public async Task<IActionResult> Index(string? search, int page = 1)
        {
            ViewBag.SearchTerm = search;
            ViewBag.CurrentPage = page;

            // TODO: Nghiệp vụ P03 - Lấy danh sách các sự kiện của Hiệp hội có tìm kiếm và phân trang
            await Task.CompletedTask;
            return View();
        }

        // GET: /Event/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            if (id <= 0)
            {
                return NotFound();
            }

            // TODO: Nghiệp vụ P03 - Lấy chi tiết sự kiện:
            // - Thông tin: tên, thời gian, địa điểm, nội dung, ban tổ chức
            // - Tính toán số chỗ còn lại (SoLuongToiDa - SoLuongDaDangKy)
            // - Kiểm tra xem Hội viên hiện tại đã đăng ký sự kiện này hay chưa
            await Task.CompletedTask;
            return View();
        }

        // POST: /Event/Register/5 (Hội viên đăng ký tham gia)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(int id)
        {
            if (id <= 0)
            {
                return BadRequest();
            }

            // TODO: Nghiệp vụ P03 - Kiểm tra 4 điều kiện nghiệp vụ trước khi ghi nhận đăng ký:
            // 1. Tài khoản đã đăng nhập và là Hội viên có trạng thái "hoat_dong" (đã được duyệt).
            // 2. Sự kiện vẫn còn trong hạn đăng ký (thời điểm hiện tại < hạn đăng ký).
            // 3. Sự kiện vẫn còn chỗ trống (Số lượng đăng ký hiện tại < Số lượng tối đa).
            // 4. Hội viên này chưa đăng ký tham gia sự kiện này trước đó.

            await Task.CompletedTask;

            TempData["SuccessMessage"] = "Đăng ký tham gia sự kiện thành công!";
            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: /Event/CancelRegistration/5 (Hội viên hủy đăng ký tham gia)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelRegistration(int id)
        {
            if (id <= 0)
            {
                return BadRequest();
            }

            // TODO: Nghiệp vụ P03 - Xử lý hủy đăng ký tham gia:
            // - Kiểm tra sự kiện chưa diễn ra
            // - Xóa bản ghi đăng ký của hội viên và cập nhật lại số lượng chỗ trống
            await Task.CompletedTask;

            TempData["SuccessMessage"] = "Bạn đã hủy đăng ký tham gia sự kiện thành công.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // GET: /Event/Create (Dành cho Admin)
        [HttpGet]
        public IActionResult Create()
        {
            // TODO: Kiểm tra quyền Admin (hoặc [Authorize(Roles = "Admin")])
            return View();
        }

        // POST: /Event/Create (Dành cho Admin)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string title, string description, DateTime startDate, DateTime endDate, DateTime deadlineDate, string location, int maxParticipants, IFormFile? banner)
        {
            // TODO: Kiểm tra quyền Admin
            if (string.IsNullOrWhiteSpace(title) || startDate == default)
            {
                ModelState.AddModelError(string.Empty, "Vui lòng nhập tên sự kiện và thời gian bắt đầu hợp lệ.");
                return View();
            }

            if (endDate < startDate)
            {
                ModelState.AddModelError("endDate", "Thời gian kết thúc phải diễn ra sau thời gian bắt đầu.");
                return View();
            }

            // TODO: Nghiệp vụ P03 - Lưu thông tin sự kiện mới vào cơ sở dữ liệu
            await Task.CompletedTask;

            TempData["SuccessMessage"] = "Tạo sự kiện mới thành công!";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Event/Participants/5 (Dành cho Admin xem danh sách điểm danh)
        [HttpGet]
        public async Task<IActionResult> Participants(int id)
        {
            // TODO: Kiểm tra quyền Admin
            if (id <= 0)
            {
                return NotFound();
            }

            // TODO: Nghiệp vụ P03 - Lấy danh sách hội viên đã đăng ký tham gia sự kiện để quản lý và điểm danh
            await Task.CompletedTask;
            return View();
        }
    }
}
