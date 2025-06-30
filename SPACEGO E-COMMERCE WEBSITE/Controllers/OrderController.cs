using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SPACEGO_E_COMMERCE_WEBSITE.Models;
using SPACEGO_E_COMMERCE_WEBSITE.Repository;
using System.Security.Claims;


public class OrderController : Controller
{
    private readonly IOrderRepository _orderRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IActivityLogService _activityLogService;

    public OrderController(IOrderRepository orderRepository, UserManager<ApplicationUser> userManager, IActivityLogService activityLogService)
    {
        _orderRepository = orderRepository;
        _userManager = userManager;
        _activityLogService = activityLogService;
    }
    // OrderController.cs
    [Authorize(SD.Role_Admin)]
    public async Task<IActionResult> Index(string searchString, string statusFilter, DateTime? dateFrom, DateTime? dateTo)
    {
        var orders = await _orderRepository.GetAllAsync();

        // Filter theo mã đơn hàng hoặc phương thức thanh toán
        if (!string.IsNullOrWhiteSpace(searchString))
        {
            orders = orders.Where(o => o.OrderId.ToString().Contains(searchString)
                                    || (!string.IsNullOrEmpty(o.PaymentMethod) && o.PaymentMethod.Contains(searchString, StringComparison.OrdinalIgnoreCase)))
                             .ToList();
        }

        // Filter theo trạng thái
        if (!string.IsNullOrWhiteSpace(statusFilter))
        {
            orders = orders.Where(o => string.Equals(o.OrderStatus, statusFilter, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        // Filter theo ngày đặt hàng
        if (dateFrom.HasValue)
        {
            orders = orders.Where(o => o.OrderDate.HasValue && o.OrderDate.Value.Date >= dateFrom.Value.Date).ToList();
        }
        if (dateTo.HasValue)
        {
            orders = orders.Where(o => o.OrderDate.HasValue && o.OrderDate.Value.Date <= dateTo.Value.Date).ToList();
        }

        // Tạo danh sách trạng thái để lọc dropdown
        var allStatuses = orders.Select(o => o.OrderStatus).Distinct().Where(s => !string.IsNullOrEmpty(s)).ToList();

        // Tạo dictionary ánh xạ phương thức thanh toán => danh sách trạng thái
        var statusMap = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["Chuyển khoản ngân hàng"] = new() { "Chờ chuyển khoản", "Chuyển khoản thành công", "Đơn hàng bị huỷ", "Chờ đóng gói", "Chờ vận chuyển", "Chờ nhận hàng", "Hoàn tất" },
            ["Thanh toán bằng VNPAY"] = new() { "Thanh toán thất bại", "Đã thanh toán", "Chờ đóng gói", "Chờ vận chuyển", "Chờ nhận hàng", "Hoàn tất" },
            ["tiền mặt"] = new() { "Chờ đóng gói", "Chờ vận chuyển", "Chờ nhận hàng", "Hoàn tất" },
            ["Thanh toán khi nhận hàng"] = new() { "Chờ đóng gói", "Chờ vận chuyển", "Chờ nhận hàng", "Hoàn tất" },
        };

        ViewBag.StatusMap = statusMap;
        ViewBag.AllStatuses = allStatuses;
        ViewBag.CurrentFilter = searchString;
        ViewBag.CurrentStatus = statusFilter;
        ViewBag.DateFrom = dateFrom?.ToString("yyyy-MM-dd");
        ViewBag.DateTo = dateTo?.ToString("yyyy-MM-dd");

        return View(orders);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> UpdateStatus(int orderId, string status)
    {
        await _orderRepository.UpdateStatusAsync(orderId, status);
        await _activityLogService.LogAsync(
            userId: User.FindFirstValue(ClaimTypes.NameIdentifier),
            userName: User.Identity?.Name ?? "Unknown",
            actionType: "Update",
            tableName: "Order",
            objectId: orderId.ToString(),
            description: $"Cập nhật trạng thái đơn hàng #{orderId} thành: {status}"
        );
        return RedirectToAction(nameof(Index));
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
