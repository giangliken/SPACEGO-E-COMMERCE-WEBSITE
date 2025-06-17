using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SPACEGO_E_COMMERCE_WEBSITE.Models;
using SPACEGO_E_COMMERCE_WEBSITE.Repository;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace SPACEGO_E_COMMERCE_WEBSITE.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly IBrandRepository _brandRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IProductImageRepository _productImageRepository;
        public List<ProductImage> ImageUrls { get; set; } = new List<ProductImage>();
        public ProductController(
            IProductRepository productRepository,
            IBrandRepository brandRepository,
            ICategoryRepository categoryRepository,
            IProductImageRepository productImageRepository)
        {
            _productRepository = productRepository;
            _brandRepository = brandRepository;
            _categoryRepository = categoryRepository;
            _productImageRepository = productImageRepository;
        }

        public async Task<IActionResult> Index(string searchString)
        {
            var products = await _productRepository.GetAllAsync();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                products = products
                    .Where(p => p.ProductName.Contains(searchString, StringComparison.OrdinalIgnoreCase)
                             || p.ProductId.ToString().Contains(searchString))
                    .ToList();
            }

            ViewData["CurrentFilter"] = searchString;
            return View(products);
        }

        [HttpGet]
        public async Task<IActionResult> Add()
        {
            var brands = await _brandRepository.GetAllAsync();
            ViewBag.Brands = new SelectList(brands, "BrandId", "BrandName");

            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "CategoryId", "CategoryName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(Product product, IFormFile mainImage, List<IFormFile> detailImages)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Brands = new SelectList(await _brandRepository.GetAllAsync(), "BrandId", "BrandName");
                ViewBag.Categories = new SelectList(await _categoryRepository.GetAllAsync(), "CategoryId", "CategoryName");
                return View(product);
            }

            try
            {
                if (mainImage != null && mainImage.Length > 0)
                {
                    product.ImageUrl = await SaveImage(mainImage, product.ProductName);
                }

                await _productRepository.AddAsync(product);

                if (detailImages != null && detailImages.Count > 0)
                {
                    foreach (var image in detailImages)
                    {
                        var productImage = new ProductImage
                        {
                            Url = await SaveImage(image, product.ProductName),
                            ProductId = product.ProductId
                        };
                        await _productImageRepository.AddAsync(productImage);
                    }
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi khi thêm sản phẩm: " + ex.Message);
                ViewBag.Brands = await _brandRepository.GetAllAsync();
                ViewBag.Categories = await _categoryRepository.GetAllAsync();
                return View(product);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
                return NotFound();

            // Nạp danh sách ảnh chi tiết
            product.ImageUrls = (await _productImageRepository.GetByProductIdAsync(id)).ToList();

            ViewBag.Brands = await _brandRepository.GetAllAsync();
            ViewBag.Categories = await _categoryRepository.GetAllAsync();
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product, IFormFile mainImage)
        {
            if (id != product.ProductId)
                return NotFound();

            if (ModelState.IsValid)
            {
                var existingProduct = await _productRepository.GetByIdAsync(id);
                if (existingProduct == null)
                {
                    return NotFound();
                }

                // Xử lý ảnh đại diện mới
                if (mainImage != null && mainImage.Length > 0)
                {
                    if (!string.IsNullOrEmpty(existingProduct.ImageUrl))
                    {
                        var relativePath = existingProduct.ImageUrl.TrimStart('/');
                        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", relativePath);
                        if (System.IO.File.Exists(fullPath))
                        {
                            System.IO.File.Delete(fullPath);
                        }
                    }
                    product.ImageUrl = await SaveImage(mainImage, product.ProductName);
                }
                else
                {
                    product.ImageUrl = existingProduct.ImageUrl;
                }

                // Cập nhật các trường còn lại
                existingProduct.ProductName = product.ProductName;
                existingProduct.ProductDescription = product.ProductDescription;
                existingProduct.ProductPrice = product.ProductPrice;
                existingProduct.ProductQuantity = product.ProductQuantity;
                existingProduct.CategoryId = product.CategoryId;
                existingProduct.BrandId = product.BrandId;
                existingProduct.ImageUrl = product.ImageUrl;
                existingProduct.isAvailable = product.isAvailable;

                await _productRepository.UpdateAsync(existingProduct);
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Brands = await _brandRepository.GetAllAsync();
            ViewBag.Categories = await _categoryRepository.GetAllAsync();
            return View(product);
        }


        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
                return NotFound();

            // Lấy danh sách ảnh chi tiết
            var imageUrls = await _productImageRepository.GetByProductIdAsync(id);
            product.ImageUrls = imageUrls.ToList();

            // Lấy Brand, Category nếu cần truyền sang ViewBag/ViewData
            ViewBag.Brand = await _brandRepository.GetByIdAsync(product.BrandId);
            ViewBag.Category = await _categoryRepository.GetByIdAsync(product.CategoryId);

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(product.ImageUrl))
            {
                var relativePath = product.ImageUrl.TrimStart('/');
                var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", relativePath);
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }
            }
            await _productRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // Helper methods
        private async Task<string> SaveImage(IFormFile image, string productName)
        {
            var extension = Path.GetExtension(image.FileName);
            var unique = Guid.NewGuid().ToString("N");
            var newFileName = $"{unique}{extension}";

            var folderPath = Path.Combine("wwwroot", "assets", "images", "product");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            var savePath = Path.Combine(folderPath, newFileName);
            using (var fileStream = new FileStream(savePath, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }

            return "/assets/images/product/" + newFileName;
        }

    }
}