using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SPACEGO_E_COMMERCE_WEBSITE.Models;
using SPACEGO_E_COMMERCE_WEBSITE.Repository;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

[Authorize(SD.Role_Admin)]

public class BrandController : Controller
{
    private readonly IBrandRepository _brandRepository;
    public BrandController(IBrandRepository brandRepository)
    {
        _brandRepository = brandRepository;
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

        ViewData["CurrentFilter"] = searchString; // Giữ giá trị search khi trả lại view
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
        return RedirectToAction(nameof(Index));
    }





}
