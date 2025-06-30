using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SPACEGO_E_COMMERCE_WEBSITE.Models;
using SPACEGO_E_COMMERCE_WEBSITE.Repository;
using System.Security.Claims;

namespace SPACEGO_E_COMMERCE_WEBSITE.Controllers
{
    [Authorize(Roles = SD.Role_Admin)]
    public class CommentController : Controller
    {
        private readonly ICommentRepository _commentRepository;
        private readonly IActivityLogService _activityLogService;

        public CommentController(ICommentRepository commentRepository, IActivityLogService activityLogService)
        {
            _commentRepository = commentRepository;
            _activityLogService = activityLogService;
        }

        public async Task<IActionResult> Index()
        {
            var comments = await _commentRepository.GetAllAsync();
            return View(comments);
        }

        public async Task<IActionResult> Details(int id)
        {
            var comment = await _commentRepository.GetByIdAsync(id);
            if (comment == null) return NotFound();
            return View(comment);
        }

        public async Task<IActionResult> Toggle(int id)
        {
            await _commentRepository.ToggleStatusAsync(id);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var comment = await _commentRepository.GetByIdAsync(id);
            if (comment == null)
            {
                return NotFound();
            }
            await _commentRepository.DeleteAsync(id);
            await _activityLogService.LogAsync(
                userId: User.FindFirstValue(ClaimTypes.NameIdentifier),
                userName: User.Identity?.Name ?? "Unknown",
                actionType: "Delete",
                tableName: "Comment",
                objectId: comment.CommentId.ToString(),
                description: $"Đã xóa bình luận của user {comment.User.UserName} cho sản phẩm {comment.ProductId}"
            );
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Pending()
        {
            var pendingComments = await _commentRepository.GetPendingCommentsAsync();
            return View("Index", pendingComments); // dùng lại view Index
        }
    }
}
