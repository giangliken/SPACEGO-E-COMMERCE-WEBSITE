using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SPACEGO_E_COMMERCE_WEBSITE.Models;
using SPACEGO_E_COMMERCE_WEBSITE.Repository;
using System.Globalization;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;

namespace SPACEGO_E_COMMERCE_WEBSITE.Controllers
{
    [Authorize(SD.Role_Admin)]

    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly IBrandRepository _brandRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IProductImageRepository _productImageRepository;
        private readonly ICapacityRepository _capacityRepository;
        private readonly IColorRepository _colorRepository;
        private readonly IProductVariantRepository _productVariantRepository;
        private readonly IActivityLogService _activityLogService;
        public List<ProductImage> ImageUrls { get; set; } = new List<ProductImage>();
        public ProductController(
            IProductRepository productRepository,
            IBrandRepository brandRepository,
            ICategoryRepository categoryRepository,
            IProductImageRepository productImageRepository,
            ICapacityRepository capacityRepository,
            IColorRepository colorRepository,
            IProductVariantRepository productVariantRepository,
            IActivityLogService activityLogService)
        {
            _productRepository = productRepository;
            _brandRepository = brandRepository;
            _categoryRepository = categoryRepository;
            _productImageRepository = productImageRepository;
            _capacityRepository = capacityRepository;
            _colorRepository = colorRepository;
            _productVariantRepository = productVariantRepository;
            _activityLogService = activityLogService;
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
                await _activityLogService.LogAsync(
                    userId: User.FindFirstValue(ClaimTypes.NameIdentifier),
                    userName: User.Identity?.Name ?? "Unknown",
                    actionType: "Add",
                    tableName: "Product",
                    objectId: product.ProductId.ToString(),
                    description: $"Đã thêm sản phẩm mới: {product.ProductName}"
                );
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
            ViewBag.ProductId = product.ProductId;
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
                ViewBag.ProductId = variant.ProductId;
                ViewBag.Colors = new SelectList(await _colorRepository.GetAllAsync(), "ColorId", "ColorName");
                ViewBag.Capacities = new SelectList(await _capacityRepository.GetAllAsync(), "CapacityId", "CapacityName");
                ViewBag.Variants = await _productVariantRepository.GetByProductIdAsync(variant.ProductId);
                return View("AddVariants", variant);
            }

            // ❗ Logic kiểm tra trùng
            var existingVariants = await _productVariantRepository.GetByProductIdAsync(variant.ProductId);
            bool isDuplicate = existingVariants.Any(v =>
                v.ColorId == variant.ColorId &&
                v.CapacityId == variant.CapacityId
            );

            if (isDuplicate)
            {
                TempData["Error"] = "Biến thể với màu và dung lượng này đã tồn tại.";
                ViewBag.ProductName = (await _productRepository.GetByIdAsync(variant.ProductId))?.ProductName;
                ViewBag.ProductId = variant.ProductId;
                ViewBag.Colors = new SelectList(await _colorRepository.GetAllAsync(), "ColorId", "ColorName");
                ViewBag.Capacities = new SelectList(await _capacityRepository.GetAllAsync(), "CapacityId", "CapacityName");
                ViewBag.Variants = existingVariants;
                return View("AddVariants", variant);
            }

            // Nếu không trùng thì thêm
            await _productVariantRepository.AddAsync(variant);
            TempData["Success"] = "Đã thêm biến thể thành công!";
            var product = await _productRepository.GetByIdAsync(variant.ProductId);

            await _activityLogService.LogAsync(
                userId: User.FindFirstValue(ClaimTypes.NameIdentifier),
                userName: User.Identity?.Name ?? "Unknown",
                actionType: "Add",
                tableName: "ProductVariant",
                objectId: variant.ProductVariantId.ToString(),
                description: $"Đã thêm biến thể cho sản phẩm '{product?.ProductName}' - Màu: {variant.Color.ColorName}, Dung lượng: {variant.Capacity.CapacityName}"
            );
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

            var brands = await _brandRepository.GetAllAsync();
            ViewBag.Brands = new SelectList(brands, "BrandId", "BrandName");

            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "CategoryId", "CategoryName");
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product, IFormFile? mainImage, List<IFormFile>? detailImages)
        {
            try
            {
                var existingProduct = await _productRepository.GetByIdAsync(id);
                if (existingProduct == null)
                    return NotFound();

                // Parse giá sản phẩm (có format . như 1.500.000)
                var culture = new CultureInfo("vi-VN");
                if (decimal.TryParse(Request.Form["ProductPrice"], NumberStyles.Number, culture, out var price))
                {
                    product.ProductPrice = price;
                }
                else
                {
                    ModelState.AddModelError("ProductPrice", "Giá không hợp lệ.");
                }

                // Lấy isAvailable từ checkbox
                var isAvailable = Request.Form["isAvailable"].ToString().Contains("true");

                // Nếu ModelState không hợp lệ → trả lại View
                if (!ModelState.IsValid)
                {
                    product.ProductName = existingProduct.ProductName;
                    product.ProductDescription = existingProduct.ProductDescription;
                    product.ProductPrice = existingProduct.ProductPrice;
                    product.BrandId = existingProduct.BrandId;
                    product.CategoryId = existingProduct.CategoryId;
                    product.isAvailable = existingProduct.isAvailable;
                    product.ImageUrl = existingProduct.ImageUrl;
                    product.ImageUrls = (await _productImageRepository.GetByProductIdAsync(id)).ToList();

                    ViewBag.Brands = new SelectList(await _brandRepository.GetAllAsync(), "BrandId", "BrandName");
                    ViewBag.Categories = new SelectList(await _categoryRepository.GetAllAsync(), "CategoryId", "CategoryName");
                    ViewBag.Variants = (await _productVariantRepository.GetByProductIdAsync(id)).ToList();

                    return View(product);
                }

                // Cập nhật dữ liệu sản phẩm
                existingProduct.ProductName = product.ProductName;
                existingProduct.ProductDescription = product.ProductDescription;
                existingProduct.ProductPrice = product.ProductPrice;
                existingProduct.BrandId = product.BrandId;
                existingProduct.CategoryId = product.CategoryId;
                existingProduct.isAvailable = isAvailable;
                existingProduct.ProductQuantity = product.ProductQuantity;

                // Nếu có ảnh mới → cập nhật
                if (mainImage != null && mainImage.Length > 0)
                {
                    // Xoá ảnh cũ
                    if (!string.IsNullOrEmpty(existingProduct.ImageUrl))
                    {
                        var relativePath = existingProduct.ImageUrl.TrimStart('/');
                        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", relativePath);
                        if (System.IO.File.Exists(fullPath))
                        {
                            System.IO.File.Delete(fullPath);
                        }
                    }

                    // Lưu ảnh mới
                    existingProduct.ImageUrl = await SaveImage(mainImage, product.ProductName);
                }

                // Lưu ảnh chi tiết mới (nếu có)
                if (detailImages != null && detailImages.Count > 0)
                {
                    foreach (var image in detailImages)
                    {
                        var productImage = new ProductImage
                        {
                            Url = await SaveImage(image, product.ProductName),
                            ProductId = existingProduct.ProductId  // ✅ đảm bảo không bị 0
                        };
                        await _productImageRepository.AddAsync(productImage);
                    }
                }

                // Lưu vào DB
                await _productRepository.UpdateAsync(existingProduct);
                await _activityLogService.LogAsync(
                    userId: User.FindFirstValue(ClaimTypes.NameIdentifier),
                    userName: User.Identity?.Name ?? "Unknown",
                    actionType: "Update",
                    tableName: "Product",
                    objectId: existingProduct.ProductId.ToString(),
                    description: $"Đã cập nhật sản phẩm: {existingProduct.ProductName}"
                );

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                Console.WriteLine("🔥 Lỗi khi cập nhật sản phẩm: " + ex.Message);
                Console.WriteLine(ex.StackTrace);
                return StatusCode(500, "Lỗi server: " + ex.Message);
            }
        }





        [HttpGet]
        public async Task<IActionResult> EditVariant(int id)
        {
            var variant = await _productVariantRepository.GetByIdAsync(id);

            if (variant == null) return NotFound();
            var product = await _productRepository.GetByIdAsync(variant.ProductId);
            ViewBag.ProductName = product?.ProductName ?? "Không rõ";
            ViewBag.Colors = new SelectList(await _colorRepository.GetAllAsync(), "ColorId", "ColorName");
            ViewBag.Capacities = new SelectList(await _capacityRepository.GetAllAsync(), "CapacityId", "CapacityName"); return View(variant);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditVariant(ProductVariant variant)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Colors = new SelectList(await _colorRepository.GetAllAsync(), "ColorId", "ColorName");
                ViewBag.Capacities = new SelectList(await _capacityRepository.GetAllAsync(), "CapacityId", "CapacityName");
                return View(variant);
            }

            // Lấy tất cả biến thể của sản phẩm
            var variants = await _productVariantRepository.GetByProductIdAsync(variant.ProductId);

            // Kiểm tra trùng (ngoại trừ chính nó)
            bool isDuplicate = variants.Any(v =>
                v.ProductVariantId != variant.ProductVariantId &&
                v.ColorId == variant.ColorId &&
                v.CapacityId == variant.CapacityId
            );

            if (isDuplicate)
            {
                ModelState.AddModelError("", "Biến thể với màu và dung lượng này đã tồn tại.");

                ViewBag.Colors = new SelectList(await _colorRepository.GetAllAsync(), "ColorId", "ColorName");
                ViewBag.Capacities = new SelectList(await _capacityRepository.GetAllAsync(), "CapacityId", "CapacityName");
                return View(variant);
            }

            // ✅ Lấy bản gốc từ database
            var existingVariant = await _productVariantRepository.GetByIdAsync(variant.ProductVariantId);
            if (existingVariant == null) return NotFound();

            // ✅ Cập nhật thủ công field (tránh lỗi tracking)
            existingVariant.ColorId = variant.ColorId;
            existingVariant.CapacityId = variant.CapacityId;
            existingVariant.Price = variant.Price;
            existingVariant.Quantity = variant.Quantity;

            await _productVariantRepository.UpdateAsync(existingVariant);
            await _activityLogService.LogAsync(
                userId: User.FindFirstValue(ClaimTypes.NameIdentifier),
                userName: User.Identity?.Name ?? "Unknown",
                actionType: "Update",
                tableName: "ProductVariant",
                objectId: existingVariant.ProductVariantId.ToString(),
                description: $"Đã cập nhật biến thể cho sản phẩm ID {variant.ProductId} - Màu: {variant.Color.ColorName}, Dung lượng: {variant.Capacity.CapacityName}"
            );
            return RedirectToAction("Edit", new { id = variant.ProductId });
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteVariant(int id)
        {
            var variant = await _productVariantRepository.GetByIdAsync(id);
            if (variant != null)
                await _productVariantRepository.DeleteAsync(id);
            await _activityLogService.LogAsync(
                userId: User.FindFirstValue(ClaimTypes.NameIdentifier),
                userName: User.Identity?.Name ?? "Unknown",
                actionType: "Delete",
                tableName: "ProductVariant",
                objectId: id.ToString(),
                description: $"Đã xóa biến thể ID {id} của sản phẩm ID {variant?.ProductId}"
            );
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
            await _activityLogService.LogAsync(
                userId: User.FindFirstValue(ClaimTypes.NameIdentifier),
                userName: User.Identity?.Name ?? "Unknown",
                actionType: "Delete",
                tableName: "Product",
                objectId: id.ToString(),
                description: $"Đã xóa sản phẩm ID {id} - {product.ProductName}"
            );
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteImage(int imageId, int productId)
        {
            var image = await _productImageRepository.GetByIdAsync(imageId);
            if (image == null)
                return NotFound();

            // Xoá file ảnh vật lý khỏi ổ đĩa
            var relativePath = image.Url.TrimStart('/');
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", relativePath);
            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }

            // Xoá ảnh khỏi database
            await _productImageRepository.DeleteAsync(imageId);

            // Quay lại trang edit
            await _activityLogService.LogAsync(
                userId: User.FindFirstValue(ClaimTypes.NameIdentifier),
                userName: User.Identity?.Name ?? "Unknown",
                actionType: "Delete",
                tableName: "ProductImage",
                objectId: imageId.ToString(),
                description: $"Đã xóa ảnh ID {imageId} của sản phẩm ID {productId}"
            );
            return RedirectToAction("Edit", new { id = productId });
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