using DocumentFormat.OpenXml.VariantTypes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SPACEGO_E_COMMERCE_WEBSITE.Models;
using SPACEGO_E_COMMERCE_WEBSITE.Models.ViewModel;
using SPACEGO_E_COMMERCE_WEBSITE.Repository;
using System.Diagnostics;
using System.Security.Claims;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;
namespace SPACEGO_E_COMMERCE_WEBSITE.Controllers
{
    public class HomeController : Controller
    {
        //private readonly ILogger<HomeController> _logger;
        private readonly ApplicationUserManager _userManager;
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IBrandRepository _brandRepository;
        private readonly IProductImageRepository _productImageRepositoryproductImage;
        private readonly IReviewRepository _reviewRepositoryreview;
        private readonly ICartItemRepository _cartItemRepositorycartItem;
        private readonly IDetailCartItemRepository _detailCartItemRepositorydetailCartItem;
        private readonly IOrderProductRepository _orderProductRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IProvinceRepository _provinceRepository;
        private readonly IDistrictRepository _districtRepository;
        private readonly IWardRepository _wardRepository;
        private readonly ICommentRepository _commentRepository;
        private readonly IProductVariantRepository _productVariantRepository;

        public HomeController(IProductRepository productRepository, ICategoryRepository categoryRepository,
                              IBrandRepository brandRepository, IProductImageRepository productImageRepositoryproductImage,
                              IReviewRepository reviewRepositoryreview, ICartItemRepository cartItemRepositorycartItem,
                              IDetailCartItemRepository detailCartItemRepositorydetailCartItem, IOrderRepository orderRepository,
                              IOrderProductRepository orderProductRepository, IProvinceRepository provinceRepository,
                              IDistrictRepository districtRepository, IWardRepository wardRepository, ICommentRepository commentRepository, IProductVariantRepository productVariantRepository, ApplicationUserManager userManager)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _brandRepository = brandRepository;
            _productImageRepositoryproductImage = productImageRepositoryproductImage;
            _reviewRepositoryreview = reviewRepositoryreview;
            _cartItemRepositorycartItem = cartItemRepositorycartItem;
            _detailCartItemRepositorydetailCartItem = detailCartItemRepositorydetailCartItem;
            _orderRepository = orderRepository;
            _orderProductRepository = orderProductRepository;
            _provinceRepository = provinceRepository;
            _districtRepository = districtRepository;
            _wardRepository = wardRepository;
            _commentRepository = commentRepository;
            _productVariantRepository = productVariantRepository;
            _userManager = userManager;
        }


        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var cart = await _cartItemRepositorycartItem.GetActiveCartByUserIdAsync(userId);
            ViewBag.CartCount = cart?.DetailCartItems?.Sum(x => x.Quanity) ?? 0;

            var products = await _productRepository.GetAllAsync(); // nhớ đảm bảo đã Include(p => p.Variants)
            var brands = await _brandRepository.GetAllAsync();
            var categories = await _categoryRepository.GetAllAsync();


            ViewBag.TopProducts = await _productRepository.GetTopSellingProductsAsync(8);
            ViewBag.ProductsByBrand = await _productRepository.GetProductsGroupedByBrandAsync();
            var model = new HomeIndexViewModel
            {
                Products = products,
                Categories = categories,
                Brands = brands
            };
            ViewBag.Categories = categories;
            return View(products); 
        }
        public async Task<IActionResult> Category(int id)
        {
            var products = await _productRepository.GetAllAsync();
            var categories = await _categoryRepository.GetAllAsync();

            var filtered = products.Where(p => p.CategoryId == id && p.isAvailable).ToList();

            ViewBag.CategoryName = categories.FirstOrDefault(c => c.CategoryId == id)?.CategoryName ?? "Không rõ";
            ViewBag.Categories = categories;

            return View(filtered);
        }
        public async Task<IActionResult> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return RedirectToAction("Index");
            }

            var allProducts = await _productRepository.GetAllAsync();
            var matchedProducts = allProducts
                .Where(p => p.ProductName.Contains(query, StringComparison.OrdinalIgnoreCase))
                .ToList();

            var categories = await _categoryRepository.GetAllAsync();
            var brands = await _brandRepository.GetAllAsync();

            ViewBag.Categories = categories;
            ViewBag.Query = query;
            ViewData["Title"] = $"Kết quả tìm kiếm: {query}";

            return View("Index", matchedProducts); // tái sử dụng Index.cshtml
        }
        public async Task<IActionResult> Details(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            bool daMua = false;
            bool daBinhLuan = false;
            if (!string.IsNullOrEmpty(userId))
            {
                // Giả sử bạn truyền productId là kiểu int
                daMua = await _orderRepository.HasUserPurchasedProductAsync(userId, id);
                daBinhLuan = await _commentRepository.HasUserCommentedAsync(userId, id);
            }

            ViewBag.DaMua = daMua;
            ViewBag.DaBinhLuan = daBinhLuan;

            var cart = await _cartItemRepositorycartItem.GetActiveCartByUserIdAsync(userId);
            ViewBag.CartCount = cart?.DetailCartItems?.Sum(x => x.Quanity) ?? 0;

            var product = await _productRepository.GetProductWithDetailsAsync(id); // dùng hàm mới đã thêm ở repo
            if (product == null)
            {
                return NotFound();
            }

            var comments = await _commentRepository.GetByProductIdAsync(id);
            ViewBag.Comments = comments;

            //Tính trung bình sao đánh giá sản phẩm
            double average = 0;
            if (comments.Any())
            {
                average = comments.Average(c => c.Rating);
            }

            //Đếm số khách hàng hài lòng
            double count = 0;
            if (comments.Any())
            {
                count = comments.Count(c => c.Rating >= 3);
            }

            ViewBag.Count = count;


            ViewBag.AverageRating = Math.Round(average, 1);

            return View(product); // ✅ View nhận đúng @model Product
        }

        //SubmitComment
        public IActionResult SubmitComment()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitComment(Comment comment)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            comment.UserId = userId;
            await _commentRepository.AddAsync(comment);
            // Quay lại trang sản phẩm với id
            return RedirectToAction("Details", "Home", new { id = comment.ProductId });
        }
        [Authorize]
        public async Task<IActionResult> EditComment(int productId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var comment = await _commentRepository.GetByUserAndProductAsync(userId, productId);

            if (comment == null)
                return NotFound();

            return View(comment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditComment(Comment comment)
        {
            if (!ModelState.IsValid)
                return View(comment);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (comment.UserId != userId)
                return Forbid();

            await _commentRepository.UpdateAsync(comment);
            return RedirectToAction("Details", new { id = comment.ProductId });
        }


        [Authorize]
        public async Task<IActionResult> Cart()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var cart = await _cartItemRepositorycartItem.GetActiveCartByUserIdAsync(userId);
            ViewBag.CartCount = cart?.DetailCartItems?.Sum(x => x.Quanity) ?? 0;
            if (cart == null || cart.DetailCartItems == null || !cart.DetailCartItems.Any())
            {
                ViewBag.Message = "Giỏ hàng của bạn đang trống.";
                return View(null); // Hoặc trả về một View mặc định báo trống
            }

            return View(cart);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddToCart(int id, int? variantId)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null || !product.isAvailable)
                return NotFound();

            decimal finalPrice;
            int? selectedVariantId = null;

            var variants = await _productVariantRepository.GetByProductIdAsync(id);
            if (variants != null && variants.Any())
            {
                if (!variantId.HasValue)
                    return BadRequest("Vui lòng chọn phiên bản sản phẩm.");

                var selectedVariant = variants.FirstOrDefault(v => v.ProductVariantId == variantId.Value);
                if (selectedVariant == null)
                    return NotFound("Không tìm thấy phiên bản sản phẩm.");

                finalPrice = selectedVariant.Price;
                selectedVariantId = selectedVariant.ProductVariantId;
            }
            else
            {
                finalPrice = product.ProductPrice;
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var cart = await _cartItemRepositorycartItem.GetActiveCartByUserIdAsync(userId);

            if (cart == null)
            {
                cart = new CartItem
                {
                    UserId = userId,
                    DetailCartItems = new List<DetailCartItem>()
                };
                cart.DetailCartItems.Add(new DetailCartItem
                {
                    ProductId = id,
                    ProductVariantId = selectedVariantId,
                    Quanity = 1,
                    Price = finalPrice
                });

                cart.TotalPrice = finalPrice;
                await _cartItemRepositorycartItem.AddAsync(cart);
            }
            else
            {
                // Load lại giỏ hàng từ DB có tracking
                var existingItem = cart.DetailCartItems
                    .FirstOrDefault(d => d.ProductId == id && d.ProductVariantId == selectedVariantId);

                if (existingItem != null)
                {
                    existingItem.Quanity++;
                    existingItem.Price = existingItem.Quanity * finalPrice;
                }
                else
                {
                    cart.DetailCartItems.Add(new DetailCartItem
                    {
                        ProductId = id,
                        ProductVariantId = selectedVariantId,
                        Quanity = 1,
                        Price = finalPrice
                    });
                }

                cart.TotalPrice = cart.DetailCartItems.Sum(x => x.Price);
                await _cartItemRepositorycartItem.UpdateAsync(cart);
            }

            return RedirectToAction("Cart", "Home");
        }




        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int productId, string actionType)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var cart = await _cartItemRepositorycartItem.GetActiveCartByUserIdAsync(userId);
            ViewBag.CartCount = cart?.DetailCartItems?.Sum(x => x.Quanity) ?? 0;
            if (cart == null)
            {
                return NotFound();
            }

            var detail = cart.DetailCartItems.FirstOrDefault(d => d.ProductId == productId);
            if (detail == null)
            {
                return NotFound();
            }

            if (actionType == "increase")
            {
                detail.Quanity++;
            }
            else if (actionType == "decrease")
            {
                if (detail.Quanity > 1)
                {
                    detail.Quanity--;
                }
                else
                {
                    // Xoá nếu số lượng về 0
                    cart.DetailCartItems.Remove(detail);
                }
            }

            detail.Price = detail.Quanity * (detail.Product?.ProductPrice ?? 0);
            cart.TotalPrice = cart.DetailCartItems.Sum(d => d.Price);

            await _cartItemRepositorycartItem.UpdateAsync(cart);

            // Trả về trực tiếp view Cart mới
            return View("Cart", cart);
        }

        [HttpGet]
        public async Task<IActionResult> Checkout([FromQuery] List<int> SelectedItemIds)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var cart = await _cartItemRepositorycartItem.GetActiveCartByUserIdAsync(userId);
            if (cart == null || !cart.DetailCartItems.Any()) return RedirectToAction("Cart");

            var selectedItems = cart.DetailCartItems
                                    .Where(d => SelectedItemIds.Contains(d.Id))
                                    .ToList();

            if (!selectedItems.Any())
            {
                TempData["Error"] = "Vui lòng chọn ít nhất một sản phẩm để thanh toán.";
                return RedirectToAction("Cart");
            }

            ViewBag.SelectedItemIds = SelectedItemIds;
            ViewBag.SelectedItems = selectedItems;
            ViewBag.Provinces = new SelectList(await _provinceRepository.GetAllAsync(), "ProvinceId", "ProvinceName");
            ViewBag.Districts = new SelectList(await _districtRepository.GetAllAsync(), "DistrictID", "DistrictName");
            ViewBag.Wards = new SelectList(await _wardRepository.GetAllAsync(), "WardID", "WardName");

            var shippingItems = selectedItems.Select(item => new
            {
                name = item.Product.ProductName,
                quantity = item.Quanity,
                height = 10,
                weight = 100,
                length = 10,
                width = 10
            }).ToList();
            var currentUser = _userManager.GetUserAsync(User).Result;
            // Trong controller
            ViewBag.FullName = currentUser?.FullName;
            ViewBag.Email = currentUser?.Email;
            ViewBag.PhoneNumber = currentUser?.PhoneNumber;

            ViewBag.ShippingItemsJson = JsonConvert.SerializeObject(shippingItems);
            ViewBag.Cart = cart;
            TempData["AVCD00"] = "Test";
            return View(new Order());
        }



        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Checkout(Order order, List<int> SelectedItemIds)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var cart = await _cartItemRepositorycartItem.GetActiveCartByUserIdAsync(userId);
            if (cart == null || !cart.DetailCartItems.Any())
                return RedirectToAction("Cart");

            var selectedItems = cart.DetailCartItems
                                    .Where(d => SelectedItemIds.Contains(d.Id))
                                    .ToList();

            if (!selectedItems.Any())
            {
                TempData["Error"] = "Vui lòng chọn ít nhất một sản phẩm để thanh toán.";
                return RedirectToAction("Cart");
            }

            if (string.IsNullOrWhiteSpace(order.AddressDetail))
            {
                ModelState.AddModelError("AddressDetail", "Vui lòng nhập địa chỉ chi tiết.");
            }

            if (!ModelState.IsValid)
            {
                // Load lại dữ liệu cho dropdown nếu cần
                ViewBag.Provinces = new SelectList(await _provinceRepository.GetAllAsync(), "ProvinceId", "ProvinceName");
                ViewBag.Districts = new SelectList(await _districtRepository.GetAllAsync(), "DistrictID", "DistrictName");
                ViewBag.Wards = new SelectList(await _wardRepository.GetAllAsync(), "WardID", "WardName");
                return View(order);
            }

            // ✅ Gán thêm các thông tin không có trong form
            order.UserId = userId;
            order.OrderDate = DateTime.Now;
            order.OrderStatus = "Chờ xác nhận";
            order.Total = selectedItems.Sum(d => d.Price)+order.ShippingFee;
            order.OrderProducts = selectedItems.Select(d => new OrderProduct
            {
                ProductId = d.ProductId,
                Quantity = d.Quanity,
                ProductVariantId = d.ProductVariantId,

            }).ToList();

            await _orderRepository.AddAsync(order);

            cart.DetailCartItems.RemoveAll(d => SelectedItemIds.Contains(d.Id));
            cart.TotalPrice = cart.DetailCartItems.Sum(x => x.Price);
            await _cartItemRepositorycartItem.UpdateAsync(cart);

            return RedirectToAction("OrderSuccess", new { orderCode = order.OrderId });
        }


        public IActionResult OrderSuccess(int orderCode)
        {
            ViewBag.OrderCode = orderCode;
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult NotFound()
        {
            return View("NotFound");
        }




        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
