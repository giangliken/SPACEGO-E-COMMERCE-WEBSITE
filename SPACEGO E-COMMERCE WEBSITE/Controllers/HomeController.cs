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
        //private readonly ApplicationUserManager _userManager;
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
                              IDistrictRepository districtRepository, IWardRepository wardRepository, ICommentRepository commentRepository, IProductVariantRepository productVariantRepository)
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
        }


        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var cart = await _cartItemRepositorycartItem.GetActiveCartByUserIdAsync(userId);
            ViewBag.CartCount = cart?.DetailCartItems?.Sum(x => x.Quanity) ?? 0;

            var products = await _productRepository.GetAllAsync(); // nhớ đảm bảo đã Include(p => p.Variants)
            var brands = await _brandRepository.GetAllAsync();
            var categories = await _categoryRepository.GetAllAsync();

            var model = new HomeIndexViewModel
            {
                Products = products,
                Categories = categories,
                Brands = brands
            };

            return View(products); 
        }


        public async Task<IActionResult> Details(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            bool daMua = false;
            if (!string.IsNullOrEmpty(userId))
            {
                // Giả sử bạn truyền productId là kiểu int
                daMua = await _orderRepository.HasUserPurchasedProductAsync(userId, id);
            }

            ViewBag.DaMua = daMua;
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




        //[HttpPost]
        //public async Task<IActionResult> UpdateQuantity(int productId, string actionType)
        //{
        //    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //    if (string.IsNullOrEmpty(userId))
        //    {
        //        return Unauthorized();
        //    }

        //    var cart = await _cartItemRepositorycartItem.GetActiveCartByUserIdAsync(userId);
        //    ViewBag.CartCount = cart?.DetailCartItems?.Sum(x => x.Quanity) ?? 0;
        //    if (cart == null)
        //    {
        //        return NotFound();
        //    }

        //    var detail = cart.DetailCartItems.FirstOrDefault(d => d.ProductId == productId);
        //    if (detail == null)
        //    {
        //        return NotFound();
        //    }

        //    if (actionType == "increase")
        //    {
        //        detail.Quanity++;
        //    }
        //    else if (actionType == "decrease")
        //    {
        //        if (detail.Quanity > 1)
        //        {
        //            detail.Quanity--;
        //        }
        //        else
        //        {
        //            // Xoá nếu số lượng về 0
        //            cart.DetailCartItems.Remove(detail);
        //        }
        //    }

        //    detail.Price = detail.Quanity * (detail.Product?.ProductPrice ?? 0);
        //    cart.TotalPrice = cart.DetailCartItems.Sum(d => d.Price);

        //    await _cartItemRepositorycartItem.UpdateAsync(cart);

        //    // Trả về trực tiếp view Cart mới
        //    return View("Cart", cart);
        //}

        //    [HttpGet]
        //    public async Task<IActionResult> Checkout()
        //    {
        //        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        //        var cart = await _cartItemRepositorycartItem.GetActiveCartByUserIdAsync(userId);
        //        ViewBag.CartCount = cart?.DetailCartItems?.Sum(x => x.Quanity) ?? 0;
        //        if (cart == null || !cart.DetailCartItems.Any()) return RedirectToAction("Cart");

        //        // Load địa chỉ mặc định (nếu cần)
        //        ViewBag.Provinces = new SelectList(await _provinceRepository.GetAllAsync(), "ProvinceId", "ProvinceName");
        //        ViewBag.Districts = new SelectList(await _districtRepository.GetAllAsync(), "DistrictID", "DistrictName");
        //        ViewBag.Wards = new SelectList(await _wardRepository.GetAllAsync(), "WardID", "WardName");

        //        // ✅ Tạo danh sách items cho phí vận chuyển
        //        var shippingItems = cart.DetailCartItems
        //.Where(item => item.Product != null) // 👈 thêm dòng này cho an toàn
        //.Select(item => new {
        //    name = item.Product.ProductName,
        //    quantity = item.Quanity,
        //    height = 10,
        //    weight = 100,
        //    length = 10,
        //    width = 10
        //}).ToList();

        //        ViewBag.ShippingItemsJson = JsonConvert.SerializeObject(shippingItems);

        //        ViewBag.Cart = cart;
        //        return View(new Order());
        //    }

        //    [HttpPost]
        //    public async Task<IActionResult> Checkout(Order order)
        //    {
        //        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        //        var cart = await _cartItemRepositorycartItem.GetActiveCartByUserIdAsync(userId);
        //        ViewBag.CartCount = cart?.DetailCartItems?.Sum(x => x.Quanity) ?? 0;
        //        if (cart == null || !cart.DetailCartItems.Any()) return RedirectToAction("Cart");

        //        // ⚠️ Generate lại ShippingItemsJson trong cả hai trường hợp
        //        var shippingItems = cart.DetailCartItems.Select(item => new
        //        {
        //            name = item.Product.ProductName,
        //            quantity = item.Quanity,
        //            height = 10,
        //            weight = 100,
        //            length = 10,
        //            width = 10
        //        }).ToList();
        //        ViewBag.ShippingItemsJson = JsonConvert.SerializeObject(shippingItems);

        //        if (!ModelState.IsValid)
        //        {
        //            var errors = new List<string>();

        //            foreach (var entry in ModelState)
        //            {
        //                foreach (var error in entry.Value.Errors)
        //                {
        //                    errors.Add($"Field: {entry.Key} ❌ Error: {error.ErrorMessage}");
        //                }
        //            }

        //            ViewBag.Errors = errors;
        //            ViewBag.Cart = cart;
        //            ViewBag.ShippingItemsJson = JsonConvert.SerializeObject(shippingItems);


        //            return View(order);
        //        }

        //        // ✅ Đặt hàng hợp lệ
        //        order.UserId = userId;

        //        order.OrderDate = DateTime.Now;
        //        order.OrderStatus = "Chờ xác nhận";

        //        decimal shippingFee = order.ShippingFee;
        //        order.Total = (cart.TotalPrice ?? 0) + shippingFee;

        //        order.OrderProducts = cart.DetailCartItems.Select(item => new OrderProduct
        //        {
        //            ProductId = item.ProductId,
        //            Quantity = item.Quanity
        //        }).ToList();

        //        await _orderRepository.AddAsync(order);
        //        await _cartItemRepositorycartItem.DeleteAsync(cart.CartItemId);

        //        return RedirectToAction("OrderSuccess");
        //    }
        //    public IActionResult OrderSuccess()
        //    {
        //        return View();
        //    }

        //    public IActionResult Privacy()
        //    {
        //        return View();
        //    }

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
