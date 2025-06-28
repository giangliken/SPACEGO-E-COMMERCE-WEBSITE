using Microsoft.AspNetCore.Mvc;
using SPACEGO_E_COMMERCE_WEBSITE.Repository;

namespace SPACEGO_E_COMMERCE_WEBSITE.Controllers
{
    public class OrderController : Controller
    {
        private readonly IOrderRepository _orderRepository;
        public OrderController(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }
        public async Task<IActionResult> Index()
        {
            var orders = await _orderRepository.GetAllAsync();
            return View(orders);
        }

    }
}
