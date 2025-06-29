using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SPACEGO_E_COMMERCE_WEBSITE.Models;
using SPACEGO_E_COMMERCE_WEBSITE.Repository;


public class OrderController : Controller
{
    private readonly IOrderRepository _orderRepository;
    private readonly UserManager<ApplicationUser> _userManager;

    public OrderController(IOrderRepository orderRepository, UserManager<ApplicationUser> userManager)
    {
        _orderRepository = orderRepository;
        _userManager = userManager;
    }
    [Authorize(SD.Role_Admin)]
    public async Task<IActionResult> Index()
    {
        var orders = await _orderRepository.GetAllAsync();
        return View(orders);
    }
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> MyOrders()
    {
        var user = await _userManager.GetUserAsync(User);
        var orders = await _orderRepository.GetOrdersByUserIdAsync(user.Id);
        return View(orders);
    }
    [Authorize(Roles = "Admin,Customer")]
    public async Task<IActionResult> Details(int id)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        if (order == null)
        {
            return NotFound();
        }

        return View(order);
    }
}
