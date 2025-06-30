using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using SPACEGO_E_COMMERCE_WEBSITE.Models;
using SPACEGO_E_COMMERCE_WEBSITE.Repository;
using System.Security.Claims;

public class PostsController : Controller
{
    private readonly IPostRepository _postRepo;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IActivityLogService _activityLogService;

    public PostsController(IPostRepository postRepo, UserManager<ApplicationUser> userManager, IActivityLogService activityLogService)
    {
        _postRepo = postRepo;
        _userManager = userManager;
        _activityLogService = activityLogService;
    }

    public async Task<IActionResult> Index()
    {
        IEnumerable<Post> posts;
        if (User.IsInRole("Admin"))
        {
            posts = await _postRepo.GetAllAsync(); // tất cả bài viết
        }
        else
        {
            posts = await _postRepo.GetPublishedAsync(); // chỉ bài viết đã xuất bản
        }
        return View(posts);
    }
    [Authorize(Roles = "Admin")]
    public IActionResult Create()
    {
        return View();
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Post post)
    {
        if (ModelState.IsValid)
        {
            var user = await _userManager.GetUserAsync(User);
            post.UserId = user.Id;
            post.CreatedAt = DateTime.Now;
            await _postRepo.AddAsync(post);
            await _activityLogService.LogAsync(
                userId: User.FindFirstValue(ClaimTypes.NameIdentifier),
                userName: User.Identity?.Name ?? "Unknown",
                actionType: "Add",
                tableName: "Post",
                objectId: post.PostId.ToString(),
                description: $"Người dùng {user.UserName} đã thêm bài viết mới: {post.Title}"
            );
            return RedirectToAction(nameof(Index));
        }
        return View(post);
    }

    public async Task<IActionResult> Details(int id)
    {
        var post = await _postRepo.GetByIdAsync(id);
        if (post == null) return NotFound();
        return View(post);
    }
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id)
    {
        var post = await _postRepo.GetByIdAsync(id);
        if (post == null) return NotFound();
        return View(post);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Post post)
    {
        if (ModelState.IsValid)
        {
            var existingPost = await _postRepo.GetByIdAsync(post.PostId);
            if (existingPost == null)
            {
                return NotFound();
            }

            // Cập nhật các trường có thể chỉnh sửa
            existingPost.Title = post.Title;
            existingPost.Content = post.Content;
            existingPost.UpdatedAt = DateTime.Now;

            await _postRepo.UpdateAsync(existingPost);

            var user = await _userManager.GetUserAsync(User);
            await _activityLogService.LogAsync(
                userId: User.FindFirstValue(ClaimTypes.NameIdentifier),
                userName: user?.UserName ?? "Unknown",
                actionType: "Update",
                tableName: "Post",
                objectId: post.PostId.ToString(),
                description: $"Người dùng {user.UserName} đã sửa bài viết: {post.Title}"
            );

            return RedirectToAction(nameof(Index));
        }

        return View(post);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var post = await _postRepo.GetByIdAsync(id);
        if (post == null) return NotFound();
        return View(post);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        var post = await _postRepo.GetByIdAsync(id);
        await _postRepo.DeleteAsync(id);
        await _activityLogService.LogAsync(
                userId: User.FindFirstValue(ClaimTypes.NameIdentifier),
                userName: User.Identity?.Name ?? "Unknown",
                actionType: "Delete",
                tableName: "Post",
                objectId: post.PostId.ToString(),
                description: $"Người dùng {user.UserName} đã xóa bài viết : {post.Title}"
            );
        return RedirectToAction(nameof(Index));
    }
}
