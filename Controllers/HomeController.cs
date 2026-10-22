using Microsoft.AspNetCore.Mvc;

namespace WNC15.Controllers
{
    // Controller quản lý Trang chủ và các trang cơ bản
    public class HomeController : Controller
    {
        // 1. Trang chủ chính (đường dẫn: / hoặc /Home/Index)
        public IActionResult Index()
        {
            return View(); // Trả về giao diện Views/Home/Index.cshtml
        }

        // 2. Trang giới thiệu về Hiệp hội (đường dẫn: /Home/About)
        public IActionResult About()
        {
            return View(); // Trả về giao diện giới thiệu
        }

        // 3. Trang chính sách bảo mật (đường dẫn: /Home/Privacy)
        public IActionResult Privacy()
        {
            return View();
        }
    }
}
