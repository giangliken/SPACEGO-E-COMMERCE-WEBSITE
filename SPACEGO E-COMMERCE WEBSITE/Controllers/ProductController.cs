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
        private readonly ICapacityRepository _capacityRepository;
        private readonly IColorRepository _colorRepository;
        private readonly IProductVariantRepository _productVariantRepository;
        public List<ProductImage> ImageUrls { get; set; } = new List<ProductImage>();
        public ProductController(
            IProductRepository productRepository,
            IBrandRepository brandRepository,
            ICategoryRepository categoryRepository,
            IProductImageRepository productImageRepository,
            ICapacityRepository capacityRepository,
            IColorRepository colorRepository,
            IProductVariantRepository productVariantRepository)
        {
            _productRepository = productRepository;
            _brandRepository = brandRepository;
            _categoryRepository = categoryRepository;
            _productImageRepository = productImageRepository;
            _capacityRepository = capacityRepository;
            _colorRepository = colorRepository;
            _productVariantRepository = productVariantRepository;
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
            // Lấy danh sách màu sắc và dung lượng
            var colors = await _colorRepository.GetAllAsync();
            var capacities = await _capacityRepository.GetAllAsync();

            // Nếu chưa có dữ liệu, tự thêm mẫu
            if (!colors.Any())
            {
                var defaultColors = new List<Color>
                {
                    new Color { ColorName = "Đen" },
                    new Color { ColorName = "Trắng" },
                    new Color { ColorName = "Xanh" },
                    new Color {ColorName = "Tím"},
                    new Color {ColorName = "Vàng"},
                    new Color {ColorName = "Đỏ"},
                };
                foreach (var color in defaultColors)
                    await _colorRepository.AddAsync(color);
                colors = await _colorRepository.GetAllAsync();
            }

            ViewBag.Colors = new SelectList(colors, "ColorId", "ColorName");

            if (!capacities.Any())
            {
                var defaultCapacities = new List<Capacity>
                {
                    new Capacity { CapacityName = "64GB" },
                    new Capacity { CapacityName = "128GB" },
                    new Capacity { CapacityName = "256GB" },
                    new Capacity { CapacityName = "512GB" },
                    new Capacity { CapacityName = "1TB" }
                };
                foreach (var cap in defaultCapacities)
                    await _capacityRepository.AddAsync(cap);
                capacities = await _capacityRepository.GetAllAsync();
            }

            ViewBag.Capacities = new SelectList(capacities, "CapacityId", "CapacityName");

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
                ViewBag.Colors = new SelectList(await _colorRepository.GetAllAsync(), "ColorId", "ColorName");
                ViewBag.Capacities = new SelectList(await _capacityRepository.GetAllAsync(), "CapacityId", "CapacityName");
                ModelState.AddModelError("", "Vui lòng điền đầy đủ thông tin sản phẩm.");

                return View(product);
            }

            try
            {
                

                if (mainImage != null && mainImage.Length > 0)
                {
                    product.ImageUrl = await SaveImage(mainImage, product.ProductName);
                }

                await _productRepository.AddAsync(product);

                // Lưu các biến thể sản phẩm
                if (product.Variants != null && product.Variants.Count > 0)
                {
                    foreach (var variant in product.Variants)
                    {
                        variant.ProductId = product.ProductId;
                        // Nếu có repository cho ProductVariant, hãy gọi:
                        await _productVariantRepository.AddAsync(variant);
                    }
                }

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
                // Nếu tick vào biến thể thì chuyển hướng
                if (product.HasVariants)
                {
                    return RedirectToAction("AddVariants", new { id = product.ProductId });
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi khi thêm sản phẩm: " + ex.Message);
                ViewBag.Brands = new SelectList(await _brandRepository.GetAllAsync(), "BrandId", "BrandName");
                ViewBag.Categories = new SelectList(await _categoryRepository.GetAllAsync(), "CategoryId", "CategoryName"); return View(product);
            }
        }

        [HttpGet]
        public async Task<IActionResult> AddVariants(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
                return NotFound();

            ViewBag.ProductName = product.ProductName;
            ViewBag.Colors = new SelectList(await _colorRepository.GetAllAsync(), "ColorId", "ColorName");
            ViewBag.Capacities = new SelectList(await _capacityRepository.GetAllAsync(), "CapacityId", "CapacityName");
            // Lấy danh sách biến thể hiện tại
            ViewBag.Variants = await _productVariantRepository.GetByProductIdAsync(id);

            return View(new ProductVariant { ProductId = id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddVariantConfirmed(ProductVariant variant)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ProductName = (await _productRepository.GetByIdAsync(variant.ProductId))?.ProductName;
                ViewBag.Colors = new SelectList(await _colorRepository.GetAllAsync(), "ColorId", "ColorName");
                ViewBag.Capacities = new SelectList(await _capacityRepository.GetAllAsync(), "CapacityId", "CapacityName");
                return View("AddVariants", variant);
            }

            await _productVariantRepository.AddAsync(variant);
            TempData["Success"] = "Đã thêm biến thể thành công!";
            return RedirectToAction("AddVariants", new { id = variant.ProductId });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
                return NotFound();

            // Lấy danh sách biến thể
            var variants = await _productVariantRepository.GetByProductIdAsync(id);
            ViewBag.Variants = variants.ToList();

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
        public async Task<IActionResult> EditVariant(int id)
        {
            var variant = await _productVariantRepository.GetByIdAsync(id);
            if (variant == null) return NotFound();
            ViewBag.Colors = new SelectList(await _colorRepository.GetAllAsync(), "ColorId", "ColorName");
            ViewBag.Capacities = new SelectList(await _capacityRepository.GetAllAsync(), "CapacityId", "CapacityName"); return View(variant);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditVariant(ProductVariant variant)
        {
            if (!ModelState.IsValid) return View(variant);
            await _productVariantRepository.UpdateAsync(variant);
            return RedirectToAction("Edit", new { id = variant.ProductId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteVariant(int id)
        {
            var variant = await _productVariantRepository.GetByIdAsync(id);
            if (variant != null)
                await _productVariantRepository.DeleteAsync(id);
            return RedirectToAction("Edit", new { id = variant?.ProductId });
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
                return NotFound();

            // Lấy danh sách biến thể
            var variants = await _productVariantRepository.GetByProductIdAsync(id);
            ViewBag.Variants = variants.ToList();

            // Lấy danh sách ảnh chi tiết
            var imageUrls = await _productImageRepository.GetByProductIdAsync(id);
            product.ImageUrls = imageUrls.ToList();

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