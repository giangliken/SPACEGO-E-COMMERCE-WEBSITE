using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SPACEGO_E_COMMERCE_WEBSITE.Models;
using SPACEGO_E_COMMERCE_WEBSITE.Models.ViewModel;
using SPACEGO_E_COMMERCE_WEBSITE.Repository;
using System.Security.Claims;

namespace SPACEGO_E_COMMERCE_WEBSITE.Controllers
{
    [Authorize(SD.Role_Admin)]

    public class WarehouseController : Controller
    {
        private readonly IProductVariantRepository _productVariantRepository;
        private readonly IProductRepository _productRepository;
        private readonly IBrandRepository _brandRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IActivityLogService _activityLogService;
        public WarehouseController(IProductVariantRepository productVariantRepository, IProductRepository productRepository, IBrandRepository brandRepository, ICategoryRepository categoryRepository, IActivityLogService activityLogService)
        {
            _productVariantRepository = productVariantRepository;
            _productRepository = productRepository;
            _brandRepository = brandRepository;
            _categoryRepository = categoryRepository;
            _activityLogService = activityLogService;
        }
        //public async Task<IActionResult> Index()
        //{
        //    var productVariants = await _productVariantRepository.GetAllAsync();
        //    return View(productVariants);
        //}
        public async Task<IActionResult> Index(string searchString, int? categoryId, int? brandId, string status)
        {
            var variants = await _productVariantRepository.GetAllAsync(); // đã include Product
            var products = await _productRepository.GetAllAsync(); // include Category, Brand

            var viewModel = new List<WarehouseItemViewModel>();

            foreach (var p in products.Where(p => !p.HasVariants))
            {
                if (!string.IsNullOrEmpty(searchString) && !p.ProductName.Contains(searchString, StringComparison.OrdinalIgnoreCase) &&!p.ProductId.ToString().Contains(searchString))
                {
                    continue;
                }

                if (categoryId.HasValue && p.CategoryId != categoryId.Value) continue;
                if (brandId.HasValue && p.BrandId != brandId.Value) continue;
                if (!string.IsNullOrEmpty(status))
                {
                    var currentStatus = GetStatus((int)p.ProductQuantity);
                    if (!string.Equals(currentStatus, status, StringComparison.OrdinalIgnoreCase)) continue;
                }
                viewModel.Add(new WarehouseItemViewModel
                {
                    VariantId = null,
                    ProductName = p.ProductName,
                    ProductId = p.ProductId.ToString(),
                    VariantDisplay = "-",
                    Quantity = (int)p.ProductQuantity,
                    Price = p.ProductPrice,
                    Status = GetStatus((int)p.ProductQuantity)
                });
            }

            foreach (var v in variants)
            {
                var p = v.Product;

                if (!string.IsNullOrEmpty(searchString) && !p.ProductName.Contains(searchString, StringComparison.OrdinalIgnoreCase) && !p.ProductId.ToString().Contains(searchString))
                {
                    continue;
                }
                if (categoryId.HasValue && p.CategoryId != categoryId.Value) continue;
                if (brandId.HasValue && p.BrandId != brandId.Value) continue;
                if (!string.IsNullOrEmpty(status))
                {
                    var currentStatus = GetStatus((int)p.ProductQuantity);
                    if (!string.Equals(currentStatus, status, StringComparison.OrdinalIgnoreCase)) continue;
                }
                viewModel.Add(new WarehouseItemViewModel
                {
                    VariantId = v.ProductVariantId,
                    ProductName = p.ProductName,
                    ProductId = p.ProductId.ToString(),
                    VariantDisplay = $"{v.Color?.ColorName ?? "-"} - {v.Capacity?.CapacityName ?? "-"}",
                    Quantity = v.Quantity,
                    Price = v.Price,
                    Status = GetStatus(v.Quantity)
                });
            }

            ViewBag.Categories = await _categoryRepository.GetAllAsync();
            ViewBag.Brands = await _brandRepository.GetAllAsync();

            return View(viewModel);
        }


        private string GetStatus(int quantity)
        {
            if (quantity == 0) return "Hết hàng";
            else if (quantity < 10) return "Sắp hết";
            return "Còn hàng";
        }

        public async Task<IActionResult> Details(int id)
        {
            // Ưu tiên tìm trong biến thể
            var variant = await _productVariantRepository.GetByIdAsync(id);
            if (variant != null)
            {
                var p = variant.Product;
                var viewModel = new WarehouseItemViewModel
                {
                    VariantId = variant.ProductVariantId,
                    ProductName = p.ProductName,
                    ProductId = p.ProductId.ToString(),
                    VariantDisplay = $"{variant.Color?.ColorName ?? "-"} - {variant.Capacity?.CapacityName ?? "-"}",
                    Quantity = variant.Quantity,
                    Price = variant.Price,
                    Status = GetStatus(variant.Quantity)
                };
                return View(viewModel);
            }

            // Nếu không có biến thể thì tìm theo ProductId
            var product = await _productRepository.GetByIdAsync(id);
            if (product != null)
            {
                var viewModel = new WarehouseItemViewModel
                {
                    VariantId = null,
                    ProductName = product.ProductName,
                    ProductId = product.ProductId.ToString(),
                    VariantDisplay = "-",
                    Quantity = (int)product.ProductQuantity,
                    Price = product.ProductPrice,
                    Status = GetStatus((int)product.ProductQuantity)
                };
                return View(viewModel);
            }

            return NotFound();
        }

        [HttpGet]
        public async Task<IActionResult> Input(int id, bool isVariant)
        {
            if (isVariant)
            {
                var variant = await _productVariantRepository.GetByIdAsync(id);
                if (variant == null) return NotFound();

                var vm = new WarehouseInputViewModel
                {
                    VariantId = variant.ProductVariantId,
                    ProductId = variant.ProductId,
                    ProductName = variant.Product.ProductName,
                    VariantDisplay = $"{variant.Color?.ColorName ?? "-"} - {variant.Capacity?.CapacityName ?? "-"}",
                    CurrentQuantity = variant.Quantity
                };
                return View(vm);
            }
            else
            {
                var product = await _productRepository.GetByIdAsync(id);
                if (product == null) return NotFound();

                var vm = new WarehouseInputViewModel
                {
                    VariantId = null,
                    ProductId = product.ProductId,
                    ProductName = product.ProductName,
                    VariantDisplay = "-",
                    CurrentQuantity = (int)product.ProductQuantity
                };
                return View(vm);
            }
        }


        [HttpPost]
        public async Task<IActionResult> InputPost([Bind("ProductId,VariantId,InputQuantity,IsVariant")] WarehouseInputViewModel model)
        {
            foreach (var kvp in ModelState)
            {
                Console.WriteLine($"Key: {kvp.Key}");
                foreach (var error in kvp.Value.Errors)
                {
                    Console.WriteLine($"- {error.ErrorMessage}");
                }
            }

            if (!ModelState.IsValid || model.InputQuantity <= 0)
            {
                // In debug tất cả lỗi của model
                var allErrors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                TempData["Check"] = "ModelState Invalid: " + string.Join(" | ", allErrors);

                if (model.IsVariant)
                {
                    var variant = await _productVariantRepository.GetByIdAsync(model.VariantId.Value);
                    if (variant == null) return NotFound();
                    model.ProductName = variant.Product.ProductName;
                    model.VariantDisplay = $"{variant.Color?.ColorName ?? "-"} - {variant.Capacity?.CapacityName ?? "-"}";
                    model.CurrentQuantity = variant.Quantity;
                }
                else
                {
                    var product = await _productRepository.GetByIdAsync(model.ProductId);
                    if (product == null) return NotFound();
                    model.ProductName = product.ProductName;
                    model.VariantDisplay = "-";
                    model.CurrentQuantity = (int)product.ProductQuantity;
                }

                return View("Input", model);
            }

            // Update số lượng
            if (model.IsVariant)
            {
                var variant = await _productVariantRepository.GetByIdAsync(model.VariantId.Value);
                if (variant == null) return NotFound();
                variant.Quantity += model.InputQuantity;
                await _productVariantRepository.UpdateAsync(variant);
            }
            else
            {
                var product = await _productRepository.GetByIdAsync(model.ProductId);
                if (product == null) return NotFound();
                product.ProductQuantity += model.InputQuantity;
                await _productRepository.UpdateAsync(product);
            }

            TempData["Success"] = "Nhập hàng thành công!";
            await _activityLogService.LogAsync(
                userId: User.FindFirstValue(ClaimTypes.NameIdentifier),
                userName: User.Identity?.Name ?? "Unknown",
                actionType: "Import",
                tableName: model.IsVariant ? "ProductVariant" : "Product",
                objectId: model.IsVariant ? model.VariantId?.ToString() : model.ProductId.ToString(),
                description: $"Nhập thêm {model.InputQuantity} đơn vị vào {(model.IsVariant ? "biến thể" : "sản phẩm")}."
            );

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> ExportToExcel()
        {
            var variants = await _productVariantRepository.GetAllAsync();
            var products = await _productRepository.GetAllAsync();

            var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Tồn kho");

            // Header
            worksheet.Cell(1, 1).Value = "STT";
            worksheet.Cell(1, 2).Value = "Mã sản phẩm";
            worksheet.Cell(1, 3).Value = "Tên sản phẩm";
            worksheet.Cell(1, 4).Value = "Biến thể";
            worksheet.Cell(1, 5).Value = "Số lượng tồn";
            worksheet.Cell(1, 6).Value = "Giá";

            int row = 2;
            int index = 1;

            // Sản phẩm không có biến thể
            foreach (var p in products.Where(p => !p.HasVariants))
            {
                worksheet.Cell(row, 1).Value = index++;
                worksheet.Cell(row, 2).Value = p.ProductId;
                worksheet.Cell(row, 3).Value = p.ProductName;
                worksheet.Cell(row, 4).Value = "-";
                worksheet.Cell(row, 5).Value = p.ProductQuantity ?? 0;
                worksheet.Cell(row, 6).Value = p.ProductPrice;
                row++;
            }

            // Biến thể
            foreach (var v in variants)
            {
                var p = v.Product;

                worksheet.Cell(row, 1).Value = index++;
                worksheet.Cell(row, 2).Value = p.ProductId;
                worksheet.Cell(row, 3).Value = p.ProductName;
                worksheet.Cell(row, 4).Value = $"{v.Color?.ColorName ?? "-"} - {v.Capacity?.CapacityName ?? "-"}";
                worksheet.Cell(row, 5).Value = v.Quantity;
                worksheet.Cell(row, 6).Value = v.Price;
                row++;
            }

            // Auto-fit
            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            var fileName = $"DanhSachTonKho_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return File(stream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        fileName);
        }

    }
}
