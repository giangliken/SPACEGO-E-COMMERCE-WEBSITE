using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SPACEGO_E_COMMERCE_WEBSITE.Models;
using SPACEGO_E_COMMERCE_WEBSITE.Models.ViewModel;
using SPACEGO_E_COMMERCE_WEBSITE.Repository;
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

        public HomeController(IProductRepository productRepository, ICategoryRepository categoryRepository,
                              IBrandRepository brandRepository, IProductImageRepository productImageRepositoryproductImage,
                              IReviewRepository reviewRepositoryreview, ICartItemRepository cartItemRepositorycartItem,
                              IDetailCartItemRepository detailCartItemRepositorydetailCartItem, IOrderRepository orderRepository,
                              IOrderProductRepository orderProductRepository)
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


        }


        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var cart = await _cartItemRepositorycartItem.GetActiveCartByUserIdAsync(userId);
            ViewBag.CartCount = cart?.DetailCartItems?.Sum(x => x.Quanity) ?? 0;

            var products = await _productRepository.GetAllAsync();
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
            var cart = await _cartItemRepositorycartItem.GetActiveCartByUserIdAsync(userId);
            ViewBag.CartCount = cart?.DetailCartItems?.Sum(x => x.Quanity) ?? 0;
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            var productImages = await _productImageRepositoryproductImage.GetByProductIdAsync(id);
            var reviews = await _reviewRepositoryreview.GetByProductIdAsync(id);

            var model = new ProductDetailViewModel
            {
                Product = product,
                ProductImages = productImages.ToList(),
                Reviews = reviews.ToList()
            };

            return View(model);
        }
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

        public async Task<IActionResult> AddToCart(int id)
        {



            var product = await _productRepository.GetByIdAsync(id);
            if (product == null || !product.isAvailable)
            {
                return NotFound();
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var cart = await _cartItemRepositorycartItem.GetActiveCartByUserIdAsync(userId);
            ViewBag.CartCount = cart?.DetailCartItems?.Sum(x => x.Quanity) ?? 0;
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
                    Quanity = 1,
                    TotalPriceProduct = product.ProductPrice
                });

                cart.TotalPrice = product.ProductPrice;

                await _cartItemRepositorycartItem.AddAsync(cart);
            }
            else
            {
                var detail = cart.DetailCartItems.FirstOrDefault(d => d.ProductId == id);
                if (detail != null)
                {
                    detail.Quanity++;
                    detail.TotalPriceProduct = detail.Quanity * product.ProductPrice;
                }
                else
                {
                    detail = new DetailCartItem
                    {
                        ProductId = id,
                        Quanity = 1,
                        TotalPriceProduct = product.ProductPrice
                    };
                    cart.DetailCartItems.Add(detail);
                }

                cart.TotalPrice = cart.DetailCartItems.Sum(d => d.TotalPriceProduct);
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

            detail.TotalPriceProduct = detail.Quanity * (detail.Product?.ProductPrice ?? 0);
            cart.TotalPrice = cart.DetailCartItems.Sum(d => d.TotalPriceProduct);

            await _cartItemRepositorycartItem.UpdateAsync(cart);

            // Trả về trực tiếp view Cart mới
            return View("Cart", cart);
        }

        public async Task<IActionResult> Checkout()
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
                return RedirectToAction("Cart");
            }

            ViewBag.Cart = cart;
            return View(new Order()); // Trả về View có form thông tin người nhận nếu có
        }
        [HttpPost]
        public async Task<IActionResult> Checkout(Order order)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            // Lấy giỏ hàng hiện tại
            var cart = await _cartItemRepositorycartItem.GetActiveCartByUserIdAsync(userId);
            ViewBag.CartCount = cart?.DetailCartItems?.Sum(x => x.Quanity) ?? 0;
            if (cart == null || cart.DetailCartItems == null || !cart.DetailCartItems.Any())
            {
                return RedirectToAction("Cart");
            }

            // Thiết lập thông tin đơn hàng
            order.UserId = userId;
            order.OrderDate = DateTime.Now;
            order.OrderStatus = false; // Đơn mới đặt
            order.Total = cart.TotalPrice;
            order.OrderProducts = new List<OrderProduct>();

            // Tạo danh sách sản phẩm trong đơn hàng
            foreach (var item in cart.DetailCartItems)
            {
                var orderProduct = new OrderProduct
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quanity
                };
                order.OrderProducts.Add(orderProduct);
            }

            // Lưu vào DB
            await _orderRepository.AddAsync(order);

            // Xoá giỏ hàng sau khi đặt
            await _cartItemRepositorycartItem.DeleteAsync(cart.CartItemId);

            return RedirectToAction("OrderSuccess");
        }
        public IActionResult OrderSuccess()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
