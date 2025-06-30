using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SPACEGO_E_COMMERCE_WEBSITE.Models;
using SPACEGO_E_COMMERCE_WEBSITE.Repository;
using System.Security.Claims;

namespace SPACEGO_E_COMMERCE_WEBSITE.Controllers
{
    [Authorize(SD.Role_Admin)]

    public class CategoryController : Controller
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IActivityLogService _activityLogService;
        public CategoryController(ICategoryRepository categoryRepository, IActivityLogService activityLogService)
        {
            _categoryRepository = categoryRepository;
            _activityLogService = activityLogService;
        }

        public async Task<IActionResult> Index(string searchString)
        {
            var category = await _categoryRepository.GetAllAsync();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                category = category
                    .Where(b => b.CategoryName.Contains(searchString, StringComparison.OrdinalIgnoreCase)
                             || b.CategoryId.ToString().Contains(searchString))
                    .ToList();
            }

            return View(category);
        }


        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(Category category)
        {
            if (ModelState.IsValid)
            {
                await _categoryRepository.AddAsync(category);
                await _activityLogService.LogAsync(
                userId: User.FindFirstValue(ClaimTypes.NameIdentifier),
                userName: User.Identity?.Name ?? "Unknown",
                actionType: "Add",
                tableName: "Category",
                objectId: category.CategoryId.ToString(),
                description: $"Đã thêm danh mục mới: {category.CategoryName}"
                );
                return RedirectToAction("Index");
            }

            return View(category);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Category category)
        {
            if (id != category.CategoryId)
                return NotFound();

            if (ModelState.IsValid)
            {
                // Lấy brand hiện tại từ DB
                var existingCategory = await _categoryRepository.GetByIdAsync(id);
                if (existingCategory == null)
                {
                    return NotFound();
                }
                // Cập nhật các thông tin còn lại
                existingCategory.CategoryName = category.CategoryName;
         
                await _categoryRepository.UpdateAsync(existingCategory);
                await _activityLogService.LogAsync(
                userId: User.FindFirstValue(ClaimTypes.NameIdentifier),
                userName: User.Identity?.Name ?? "Unknown",
                actionType: "Update",
                tableName: "Category",
                objectId: category.CategoryId.ToString(),
                description: $"Đã sửa danh mục : {category.CategoryName}"
                );
                return RedirectToAction(nameof(Index));
            }

            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            await _categoryRepository.DeleteAsync(id);
            await _activityLogService.LogAsync(
                userId: User.FindFirstValue(ClaimTypes.NameIdentifier),
                userName: User.Identity?.Name ?? "Unknown",
                actionType: "Delete",
                tableName: "Category",
                objectId: category.CategoryId.ToString(),
                description: $"Đã xóa danh mục : {category.CategoryName}"
                );
            return RedirectToAction(nameof(Index));
        }

    }
}
