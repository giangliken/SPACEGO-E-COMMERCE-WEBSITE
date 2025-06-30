using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SPACEGO_E_COMMERCE_WEBSITE.Models;
using SPACEGO_E_COMMERCE_WEBSITE.Repository;
using System.Globalization;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;

[Authorize(SD.Role_Admin)]

public class BrandController : Controller
{
    private readonly IBrandRepository _brandRepository;
    private readonly IActivityLogService _activityLogService;
    public BrandController(IBrandRepository brandRepository, IActivityLogService activityLogService)
    {
        _brandRepository = brandRepository;
        _activityLogService = activityLogService;
    }


    public async Task<IActionResult> Index(string searchString)
    {
        var brands = await _brandRepository.GetAllAsync();

        if (!string.IsNullOrWhiteSpace(searchString))
        {
            brands = brands
                .Where(b => b.BrandName.Contains(searchString, StringComparison.OrdinalIgnoreCase)
                         || b.BrandId.ToString().Contains(searchString))
                .ToList();
        }

        return View(brands);
    }


    [HttpGet]
    public IActionResult Add()
    {
        return View(); 
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(Brand brand, IFormFile imageUrl)
    {
        if (ModelState.IsValid)
        {
            // Kiểm tra file được upload
            if (imageUrl != null && imageUrl.Length > 0)
            {
                // Lưu hình và gán đường dẫn cho ImageUrl
                brand.ImageUrl = await SaveImage(imageUrl, brand.BrandName);
            }

            await _brandRepository.AddAsync(brand);


            await _activityLogService.LogAsync(
                userId: User.FindFirstValue(ClaimTypes.NameIdentifier),
                userName: User.Identity?.Name ?? "Unknown",
                actionType: "Add",
                tableName: "Brand",
                objectId: brand.BrandId.ToString(),
                description: $"Đã thêm nhãn hiệu mới: {brand.BrandName}"
            );

            return RedirectToAction("Index");
        }

        return View(brand);
    }


    private async Task<string> SaveImage(IFormFile image, string brandName)
    {
        var extension = Path.GetExtension(image.FileName);
        var unique = Guid.NewGuid().ToString("N");
        var newFileName = $"{unique}{extension}";

        var folderPath = Path.Combine("wwwroot", "assets", "images", "brand");
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        var savePath = Path.Combine(folderPath, newFileName);
        using (var fileStream = new FileStream(savePath, FileMode.Create))
        {
            await image.CopyToAsync(fileStream);
        }

        // Trả về URL để hiển thị ảnh trên web
        return "/assets/images/brand/" + newFileName;

    }


    public async Task<IActionResult> Edit(int id)
    {
        var brand = await _brandRepository.GetByIdAsync(id);
        if (brand == null)
        {
            return NotFound();
        }
        return View(brand);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Brand brand, IFormFile imageUrl)
    {
        if (id != brand.BrandId)
            return NotFound();

        if (ModelState.IsValid)
        {
            // Lấy brand hiện tại từ DB
            var existingBrand = await _brandRepository.GetByIdAsync(id);
            if (existingBrand == null)
            {
                return NotFound();
            }

            // Nếu có ảnh mới thì:
            if (imageUrl != null && imageUrl.Length > 0)
            {
                // XÓA ảnh cũ nếu tồn tại
                if (!string.IsNullOrEmpty(existingBrand.ImageUrl))
                {
                    var relativePath = existingBrand.ImageUrl.TrimStart('/'); // "assets/images/brand/logo.jpg"
                    var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", relativePath);

                    if (System.IO.File.Exists(fullPath))
                    {
                        System.IO.File.Delete(fullPath);
                    }
                }

                // Lưu ảnh mới và gán lại đường dẫn
                brand.ImageUrl = await SaveImage(imageUrl, brand.BrandName);
            }
            else
            {
                // Nếu không có ảnh mới thì giữ nguyên ảnh cũ
                brand.ImageUrl = existingBrand.ImageUrl;
            }

            // Cập nhật các thông tin còn lại
            existingBrand.BrandName = brand.BrandName;
            existingBrand.BrandDescription = brand.BrandDescription;
            existingBrand.ImageUrl = brand.ImageUrl;

            await _brandRepository.UpdateAsync(existingBrand);

            await _activityLogService.LogAsync(
                userId: User.FindFirstValue(ClaimTypes.NameIdentifier),
                userName: User.Identity?.Name ?? "Unknown",
                actionType: "Edit",
                tableName: "Brand",
                objectId: brand.BrandId.ToString(),
                description: $"Đã sửa nhãn hiệu : {brand.BrandName}"
            );
            return RedirectToAction(nameof(Index));
        }

        return View(brand);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var brand = await _brandRepository.GetByIdAsync(id);
        if (brand == null)
        {
            return NotFound();
        }

        if (!string.IsNullOrEmpty(brand.ImageUrl))
        {
            var relativePath = brand.ImageUrl.TrimStart('/'); // "assets/images/brand/logo.jpg"
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", relativePath);

            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }
        }
        await _brandRepository.DeleteAsync(id);
        await _activityLogService.LogAsync(
                userId: User.FindFirstValue(ClaimTypes.NameIdentifier),
                userName: User.Identity?.Name ?? "Unknown",
                actionType: "Delete",
                tableName: "Brand",
                objectId: brand.BrandId.ToString(),
                description: $"Đã xóa nhãn hiệu : {brand.BrandName}"
            );
        return RedirectToAction(nameof(Index));
    }





}
