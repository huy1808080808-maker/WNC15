using Microsoft.AspNetCore.Mvc;

namespace WNC15.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        // GET: /
        // GET: /Home/Index
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // TODO: Lấy dữ liệu cho trang chủ:
            // - Bài viết / Tin tức mới nhất
            // - Các sự kiện sắp diễn ra của Hiệp hội
            await Task.CompletedTask;
            return View();
        }

        // GET: /Home/About
        [HttpGet]
        public IActionResult About()
        {
            // Trang giới thiệu chung về Hiệp hội, tôn chỉ và ban điều hành
            return View();
        }

        // GET: /Home/Search
        [HttpGet]
        public async Task<IActionResult> Search(string? query)
        {
            ViewBag.SearchQuery = query;

            // TODO: Tìm kiếm cơ bản trên website (tin tức, sự kiện)
            await Task.CompletedTask;
            return View();
        }
    }
}
