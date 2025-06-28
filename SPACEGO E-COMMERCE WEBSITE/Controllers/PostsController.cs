using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using SPACEGO_E_COMMERCE_WEBSITE.Models;

namespace SPACEGO_E_COMMERCE_WEBSITE.Controllers
{
    [Authorize(SD.Role_Admin)]
    public class PostsController : Controller
    {
        private readonly IPostRepository _postRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public PostsController(IPostRepository postRepository, UserManager<ApplicationUser> userManager)
        {
            _postRepository = postRepository;
            _userManager = userManager;
        }

        // Hiển thị danh sách tất cả bài viết (admin xem được cả bài chưa public)
        public async Task<IActionResult> Index()
        {
            var posts = await _postRepository.GetAllAsync();
            var allPosts = posts.OrderByDescending(p => p.CreatedAt);
            return View(allPosts);
        }

        // Xem chi tiết bài viết (admin xem được tất cả)
        public async Task<IActionResult> Details(int id)
        {
            var post = await _postRepository.GetByIdAsync(id);
            if (post == null)
            {
                return NotFound();
            }
            return View(post);
        }

        // GET: Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Post post)
        {
            if (ModelState.IsValid)
            {
                post.CreatedAt = DateTime.UtcNow;
                post.UpdatedAt = DateTime.UtcNow;
                post.UserId = _userManager.GetUserId(User);
                await _postRepository.AddAsync(post);
                await _postRepository.SaveAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(post);
        }

        // GET: Edit
        public async Task<IActionResult> Edit(int id)
        {
            var post = await _postRepository.GetByIdAsync(id);
            if (post == null)
            {
                return NotFound();
            }
            return View(post);
        }

        // POST: Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Post post)
        {
            if (id != post.PostId) return NotFound();

            if (ModelState.IsValid)
            {
                var existingPost = await _postRepository.GetByIdAsync(id);
                if (existingPost == null) return NotFound();

                // Chỉ cập nhật các trường cho phép
                existingPost.Title = post.Title;
                existingPost.Content = post.Content;
                existingPost.IsPublished = post.IsPublished;
                existingPost.PostCategoryId = post.PostCategoryId;
                existingPost.UpdatedAt = DateTime.UtcNow;

                await _postRepository.UpdateAsync(existingPost);
                await _postRepository.SaveAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(post);
        }

        // GET: Delete
        public async Task<IActionResult> Delete(int id)
        {
            var post = await _postRepository.GetByIdAsync(id);
            if (post == null)
            {
                return NotFound();
            }
            return View(post);
        }

        // POST: Delete
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _postRepository.DeleteAsync(id);
            await _postRepository.SaveAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
