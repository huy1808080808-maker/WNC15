using Microsoft.AspNetCore.Mvc;

namespace WNC15.Controllers
{
    // Dành riêng cho Quản trị viên: [Authorize(Roles = "Admin")]
    public class AdminMemberController : Controller
    {
        private readonly ILogger<AdminMemberController> _logger;

        public AdminMemberController(ILogger<AdminMemberController> logger)
        {
            _logger = logger;
        }

        // GET: /AdminMember
        // GET: /AdminMember/Index
        [HttpGet]
        public async Task<IActionResult> Index(string? keyword, int page = 1)
        {
            ViewBag.Keyword = keyword;
            ViewBag.CurrentPage = page;

            // TODO: Nghiệp vụ P01 & P04 - Lấy danh sách toàn bộ thành viên trong hệ thống có tìm kiếm và phân trang
            await Task.CompletedTask;
            return View();
        }

        // GET: /AdminMember/PendingMembers
        [HttpGet]
        public async Task<IActionResult> PendingMembers()
        {
            // TODO: Nghiệp vụ P01 & P04 - Lấy danh sách các hồ sơ đang ở trạng thái "cho_duyet" (từ biểu mẫu BM01)
            // để Admin xem xét và bấm duyệt hoặc từ chối
            await Task.CompletedTask;
            return View();
        }

        // POST: /AdminMember/Approve/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            if (id <= 0)
            {
                return BadRequest();
            }

            // TODO: Nghiệp vụ P01 & P04 - Phê duyệt hồ sơ hội viên:
            // - Cập nhật trạng thái tài khoản từ "cho_duyet" sang "hoat_dong"
            // - Ghi nhận thời gian duyệt và người duyệt
            await Task.CompletedTask;

            TempData["SuccessMessage"] = "Đã phê duyệt hồ sơ hội viên thành công!";
            return RedirectToAction(nameof(PendingMembers));
        }

        // POST: /AdminMember/Reject/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id, string lyDo)
        {
            if (id <= 0)
            {
                return BadRequest();
            }

            if (string.IsNullOrWhiteSpace(lyDo))
            {
                TempData["ErrorMessage"] = "Vui lòng nhập lý do từ chối hồ sơ.";
                return RedirectToAction(nameof(PendingMembers));
            }

            // TODO: Nghiệp vụ P01 & P04 - Từ chối hồ sơ hội viên:
            // - Cập nhật trạng thái tài khoản sang "tu_choi"
            // - Lưu lại lý do từ chối để hội viên có thể xem
            await Task.CompletedTask;

            TempData["SuccessMessage"] = "Đã từ chối hồ sơ hội viên.";
            return RedirectToAction(nameof(PendingMembers));
        }

        // POST: /AdminMember/Lock/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Lock(int id)
        {
            if (id <= 0)
            {
                return BadRequest();
            }

            // TODO: Nghiệp vụ P01 & P04 - Khóa tài khoản vi phạm quy chế Hiệp hội
            // - Cập nhật trạng thái sang "khoa"
            await Task.CompletedTask;

            TempData["SuccessMessage"] = "Đã khóa tài khoản thành viên vi phạm thành công.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /AdminMember/Unlock/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unlock(int id)
        {
            if (id <= 0)
            {
                return BadRequest();
            }

            // TODO: Nghiệp vụ P01 & P04 - Mở khóa tài khoản thành viên
            // - Đưa trạng thái trở lại "hoat_dong"
            await Task.CompletedTask;

            TempData["SuccessMessage"] = "Đã mở khóa tài khoản thành viên thành công.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /AdminMember/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
            {
                return BadRequest();
            }

            // TODO: Nghiệp vụ P01 & P04 - Xóa tài khoản thành viên khỏi hệ thống
            await Task.CompletedTask;

            TempData["SuccessMessage"] = "Đã xóa tài khoản thành viên thành công.";
            return RedirectToAction(nameof(Index));
        }
    }
}
