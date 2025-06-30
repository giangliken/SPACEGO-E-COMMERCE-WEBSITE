using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using SPACEGO_E_COMMERCE_WEBSITE.Repository;
using System.Linq;
using System.Threading.Tasks;


namespace SPACEGO_E_COMMERCE_WEBSITE.Controllers
{
    public class ReportController : Controller
    {
        private readonly IOrderRepository _orderRepository;

        public ReportController(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<IActionResult> Index(DateTime? dateFrom, DateTime? dateTo)
        {
            var orders = await _orderRepository.GetAllAsync();

            // Lọc theo thời gian nếu có
            if (dateFrom.HasValue)
            {
                orders = orders.Where(o => o.OrderDate.HasValue && o.OrderDate.Value.Date >= dateFrom.Value.Date).ToList();
            }
            if (dateTo.HasValue)
            {
                orders = orders.Where(o => o.OrderDate.HasValue && o.OrderDate.Value.Date <= dateTo.Value.Date).ToList();
            }



            var totalOrders = orders.Count();
            var totalRevenue = orders.Sum(o => o.Total);
            var totalShipping = orders.Sum(o => o.ShippingFee);
            var completedOrders = orders.Count(o => o.OrderStatus == "Hoàn tất");
            var cancelledOrders = orders.Count(o => o.OrderStatus == "Đã hủy");

            ViewBag.ChartData = new
            {
                Completed = completedOrders,
                Cancelled = cancelledOrders,
                Pending = totalOrders - completedOrders - cancelledOrders
            };
            ViewBag.TotalOrders = totalOrders;
            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.TotalShipping = totalShipping;
            ViewBag.CompletedOrders = completedOrders;
            ViewBag.CancelledOrders = cancelledOrders;

            ViewBag.DateFrom = dateFrom?.ToString("yyyy-MM-dd");
            ViewBag.DateTo = dateTo?.ToString("yyyy-MM-dd");

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ExportToExcel(DateTime? dateFrom, DateTime? dateTo)
        {
            var orders = await _orderRepository.GetAllAsync();

            // Áp dụng lọc theo ngày nếu có
            if (dateFrom.HasValue)
            {
                orders = orders.Where(o => o.OrderDate.HasValue && o.OrderDate.Value.Date >= dateFrom.Value.Date).ToList();
            }
            if (dateTo.HasValue)
            {
                orders = orders.Where(o => o.OrderDate.HasValue && o.OrderDate.Value.Date <= dateTo.Value.Date).ToList();
            }

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Đơn hàng");

                // Header
                worksheet.Cell(1, 1).Value = "STT";
                worksheet.Cell(1, 2).Value = "Mã đơn";
                worksheet.Cell(1, 3).Value = "Ngày đặt";
                worksheet.Cell(1, 4).Value = "Tổng tiền (VNĐ)";
                worksheet.Cell(1, 5).Value = "Phí vận chuyển";
                worksheet.Cell(1, 6).Value = "Trạng thái";

                int row = 2;
                int stt = 1;
                decimal totalRevenue = 0;

                foreach (var order in orders)
                {
                    decimal total = order.Total;
                    decimal shipping = order.ShippingFee;

                    totalRevenue += total;

                    worksheet.Cell(row, 1).Value = stt++;
                    worksheet.Cell(row, 2).Value = $"#{order.OrderId}";
                    worksheet.Cell(row, 3).Value = order.OrderDate?.ToString("dd/MM/yyyy HH:mm") ?? "";
                    worksheet.Cell(row, 4).Value = total;
                    worksheet.Cell(row, 5).Value = shipping;
                    worksheet.Cell(row, 6).Value = order.OrderStatus;
                    row++;
                }

                // Tổng doanh thu
                worksheet.Cell(row + 1, 1).Value = $"Tổng doanh thu: {totalRevenue:N0} ₫";
                worksheet.Range(row + 1, 1, row + 1, 6).Merge();
                worksheet.Cell(row + 1, 1).Style.Font.Bold = true;
                worksheet.Cell(row + 1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Căn chỉnh cột
                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Position = 0;

                    var fileName = $"BaoCaoDonHang_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
        }


    }
}
