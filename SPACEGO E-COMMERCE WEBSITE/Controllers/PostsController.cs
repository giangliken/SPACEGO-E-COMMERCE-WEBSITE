using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SPACEGO_E_COMMERCE_WEBSITE.Models;

public class PostsController : Controller
{
    private readonly IPostRepository _postRepo;
    private readonly UserManager<ApplicationUser> _userManager;

    public PostsController(IPostRepository postRepo, UserManager<ApplicationUser> userManager)
    {
        _postRepo = postRepo;
        _userManager = userManager;
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
            post.UpdatedAt = DateTime.Now;
            await _postRepo.UpdateAsync(post);
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
        await _postRepo.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }
}
