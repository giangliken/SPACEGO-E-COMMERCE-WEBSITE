using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        public HomeController(IProductRepository productRepository, ICategoryRepository categoryRepository,
                              IBrandRepository brandRepository, IProductImageRepository productImageRepositoryproductImage,
                              IReviewRepository reviewRepositoryreview, ICartItemRepository cartItemRepositorycartItem,
                              IDetailCartItemRepository detailCartItemRepositorydetailCartItem)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _brandRepository = brandRepository;
            _productImageRepositoryproductImage = productImageRepositoryproductImage;
            _reviewRepositoryreview = reviewRepositoryreview;
            _cartItemRepositorycartItem = cartItemRepositorycartItem;
            _detailCartItemRepositorydetailCartItem = detailCartItemRepositorydetailCartItem;


        }

        public async Task<IActionResult> Index()
        {
            var products = await _productRepository.GetAllAsync();
            var brands = await _brandRepository.GetAllAsync();
            var categories = await _categoryRepository.GetAllAsync();

            var model = new HomeIndexViewModel
            {
                Products = products,
                Categories = categories,
                Brands = brands
            };

            return View(model);
        }

        public async Task<IActionResult> Details(int id)
        {
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
