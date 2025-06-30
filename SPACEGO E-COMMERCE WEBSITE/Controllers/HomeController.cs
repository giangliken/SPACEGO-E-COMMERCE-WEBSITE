using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.VariantTypes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using SPACEGO_E_COMMERCE_WEBSITE.Models;
using SPACEGO_E_COMMERCE_WEBSITE.Models.ViewModel;
using SPACEGO_E_COMMERCE_WEBSITE.Repository;
using SPACEGO_E_COMMERCE_WEBSITE.Services.VNPAY;
using System.Diagnostics;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using IEmailSender = SPACEGO_E_COMMERCE_WEBSITE.Repository.IEmailSender;
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
        private readonly IEmailSender _emailSender;
        private readonly IVnPayService _vnPayService;


        public HomeController(IProductRepository productRepository, ICategoryRepository categoryRepository,
                              IBrandRepository brandRepository, IProductImageRepository productImageRepositoryproductImage,
                              IReviewRepository reviewRepositoryreview, ICartItemRepository cartItemRepositorycartItem,
                              IDetailCartItemRepository detailCartItemRepositorydetailCartItem, IOrderRepository orderRepository,
                              IOrderProductRepository orderProductRepository, IProvinceRepository provinceRepository,
                              IDistrictRepository districtRepository, IWardRepository wardRepository, ICommentRepository commentRepository, IProductVariantRepository productVariantRepository, ApplicationUserManager userManager, IEmailSender emailSender, IVnPayService vnPayService)
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
            _emailSender = emailSender;
            _vnPayService = vnPayService;
        }

        public IActionResult CreatePaymentUrlVnpay(PaymentInformationModel model)
        {
            var url = _vnPayService.CreatePaymentUrl(model, HttpContext);

            return Redirect(url);
        }
        
        [HttpGet]
        public async Task<IActionResult> PaymentReturn()
        {
            var response = _vnPayService.PaymentExecute(Request.Query);

            var responseCode = response.VnPayData.TryGetValue("vnp_ResponseCode", out var code) ? code : "";
            var status = response.VnPayData.TryGetValue("vnp_TransactionStatus", out var transStatus) ? transStatus : "";
            var orderIdStr = response.VnPayData.TryGetValue("vnp_TxnRef", out var txnRef) ? txnRef : null;

            if (responseCode == "00" && status == "00" && int.TryParse(orderIdStr, out int orderId))
            {

                // ✅ Update trạng thái đơn nếu cần ở đây
                ViewBag.Message = "✅ Thanh toán thành công!";
                var order = await _orderRepository.GetByIdAsync(orderId);
                if (order != null)
                {
                    order.OrderStatus = "Đã thanh toán";
                    order.PaymentMethod = "Thanh toán bằng VNPAY";
                    await _orderRepository.UpdateAsync(order);
                }

                return RedirectToAction("OrderSuccess", "Home", new { orderCode = orderId });
            }


            return RedirectToAction("PaymentFail", "Home");
        }


        public async Task<IActionResult> Index(string searchString)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var cart = await _cartItemRepositorycartItem.GetActiveCartByUserIdAsync(userId);
            ViewBag.CartCount = cart?.DetailCartItems?.Sum(x => x.Quanity) ?? 0;

            var products = await _productRepository.GetAllAsync();
            var brands = await _brandRepository.GetAllAsync();
            var categories = await _categoryRepository.GetAllAsync();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                products = products
                    .Where(p => p.ProductName.Contains(searchString, StringComparison.OrdinalIgnoreCase)
                             || p.ProductId.ToString().Contains(searchString))
                    .ToList();
                ViewData["Title"] = $"Kết quả tìm kiếm: {searchString}";
            }
            else
            {
                ViewData["Title"] = "Trang chủ";
            }

            var ratingDict = new Dictionary<int, (double average, int count)>();

            foreach (var product in products)
            {
                // Truy vấn tất cả comment theo ProductId từ _commentRepository
                var comments = await _commentRepository.GetByProductIdAsync(product.ProductId);
                comments = comments.Where(c => c.isActive).ToList();

                var count = comments.Count();
                var average = count > 0 ? Math.Round(comments.Average(c => c.Rating), 1) : 0;

                ratingDict[product.ProductId] = (average, count);
            }

            ViewBag.RatingDict = ratingDict;
            ViewData["CurrentFilter"] = searchString;
            ViewBag.TopProducts = await _productRepository.GetTopSellingProductsAsync(8);
            ViewBag.ProductsByBrand = await _productRepository.GetProductsGroupedByBrandAsync();
            ViewBag.Categories = categories;

            return View(products); // Trả về View Index.cshtml với model là danh sách Product
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
        //public async Task<IActionResult> Search(string query)
        //{
        //    if (string.IsNullOrWhiteSpace(query))
        //    {
        //        return RedirectToAction("Index");
        //    }

        //    var allProducts = await _productRepository.GetAllAsync();
        //    var matchedProducts = allProducts
        //        .Where(p => p.ProductName.Contains(query, StringComparison.OrdinalIgnoreCase))
        //        .ToList();

        //    var categories = await _categoryRepository.GetAllAsync();
        //    var brands = await _brandRepository.GetAllAsync();

        //    ViewBag.Categories = categories;
        //    ViewBag.Query = query;
        //    ViewData["Title"] = $"Kết quả tìm kiếm: {query}";

        //    return View("Index", matchedProducts); // tái sử dụng Index.cshtml
        //}
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
        [Authorize]
        public IActionResult SubmitComment()
        {
            return View();
        }

        [Authorize]
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

        [Authorize]
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

            decimal finalPrice = 0;
            int? selectedVariantId = null;

            var variants = await _productVariantRepository.GetByProductIdAsync(id);
            if (variants != null && variants.Any())
            {
                if (!variantId.HasValue)
                    return BadRequest("Vui lòng chọn phiên bản sản phẩm.");

                var selectedVariant = variants.FirstOrDefault(v => v.ProductVariantId == variantId);
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
                    .FirstOrDefault(d => d.ProductId == id && d.ProductVariantId == variantId);

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
                    await _cartItemRepositorycartItem.UpdateAsync(cart);

                }

                foreach (var item in cart.DetailCartItems)
                {
                    var unitPrice = item.ProductVariant?.Price ?? item.Product?.ProductPrice ?? 0;
                    item.Price = item.Quanity * unitPrice;
                }
                cart.TotalPrice = cart.DetailCartItems.Sum(x => x.Price);
                await _cartItemRepositorycartItem.UpdateAsync(cart);
            }

            return RedirectToAction("Cart", "Home");
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int productId, int? variantId, string actionType)
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

            var detail = cart.DetailCartItems.FirstOrDefault(d => d.ProductId == productId && (variantId == null || d.ProductVariantId == variantId));
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
            var product = await _productRepository.GetProductWithDetailsAsync(productId); // dùng hàm mới đã thêm ở repo

            if (product.HasVariants && variantId.HasValue)
            {
                // Lấy thông tin biến thể nếu có
                var variant = await _productVariantRepository.GetByIdAsync(variantId.Value);
                if (variant == null)
                {
                    return NotFound("Không tìm thấy biến thể sản phẩm.");
                }
                detail.Price = detail.Quanity * variant.Price;
            }
            else
            {
                // Nếu không có biến thể, sử dụng giá sản phẩm gốc
                detail.Price = detail.Quanity * product.ProductPrice;
            }


            await _detailCartItemRepositorydetailCartItem.UpdateAsync(detail);
            cart.TotalPrice = cart.DetailCartItems.Sum(d => d.Price);

            await _cartItemRepositorycartItem.UpdateAsync(cart);

            // Trả về trực tiếp view Cart mới
            return View("Cart", cart);
        }

        [Authorize]

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
            order.Total = selectedItems.Sum(d => d.Price) + order.ShippingFee;
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

            //// 2. Chuẩn bị danh sách sản phẩm HTML
            //string productListHtml = "";
            //foreach (var item in order.OrderProducts)
            //{
            //    productListHtml += $"<li>{item.Product.ProductName} - SL: {item.Quantity} - Giá: {item.:N0}₫</li>";
            //}

            // 3. Gửi mail xác nhận
            string subject = "Xác nhận đơn hàng từ SPACEGO";
            string body = $@"
        <p>Chào {order.FullName},</p>

        <p>Cảm ơn bạn đã đặt hàng tại <strong>SPACEGO</strong>!</p>

        <p>Thông tin đơn hàng của bạn như sau:</p>
        <ul>
            <li><strong>Mã đơn hàng:</strong> #{order.OrderId}</li>
            <li><strong>Ngày đặt hàng:</strong> {order.OrderDate:dd/MM/yyyy HH:mm}</li>
            <li><strong>Phương thức thanh toán:</strong> {order.PaymentMethod}</li>
            <li><strong>Phí vận chuyển:</strong> {order.ShippingFee:N0}₫</li>
            <li><strong>Tổng thanh toán:</strong> {order.Total:N0}₫</li>
        </ul>

        <p><strong>Sản phẩm đã đặt:</strong></p>

        <p><strong>Địa chỉ giao hàng:</strong></p>
        <p>
            {order.FullName}<br />
            {order.PhoneNumber}<br />
            {order.AddressDetail}, {order.WardName}, {order.DistrictName}, {order.ProvinceName}
        </p>

        <p>Nếu bạn có bất kỳ câu hỏi nào, vui lòng liên hệ với chúng tôi qua email này hoặc số điện thoại hỗ trợ trên website.</p>

        <p>Trân trọng,<br><strong>SPACEGO TEAM</strong></p>
    ";

            await _emailSender.SendEmailAsync(order.Email, subject, body);
            TempData["Success"] = "Đơn hàng đã được đặt thành công. Thông tin đã được gửi qua email.";
            if (order.PaymentMethod == "Chuyển khoản ngân hàng")
            {
                order.PaymentMethod = "Chuyển khoản ngân hàng";
                order.OrderStatus = "Chưa thanh toán"; // Cập nhật trạng thái đơn hàng
                await _orderRepository.UpdateAsync(order); // Cập nhật phương thức thanh toán vào đơn hàng
                return RedirectToAction("BankTransferInfo", new { orderId = order.OrderId });
            }
            else if (order.PaymentMethod == "Thanh toán bằng VNPAY")
            {
                var paymentModel = new PaymentInformationModel
                {
                    OrderType = "other",
                    Amount = (double)order.Total,
                    Name = order.FullName,
                    OrderDescription = "Thanh toán đơn hàng",
                    OrderId = order.OrderId,

                };
                order.PaymentMethod = "Thanh toán bằng VNPAY";
                order.OrderStatus = "Chưa thanh toán"; // Cập nhật trạng thái đơn hàng
                await _orderRepository.UpdateAsync(order); // Cập nhật phương thức thanh toán vào đơn hàng
                return RedirectToAction("CreatePaymentUrlVnpay", "Home", paymentModel);
            }
            else
            {
                return RedirectToAction("OrderSuccess", new { orderCode = order.OrderId });
            }
        }

        public async Task<IActionResult> BankTransferInfo(int orderId)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null) return NotFound();

            return View(order); // View sẽ hiển thị thông tin chuyển khoản và đơn hàng
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


        public IActionResult About()
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
