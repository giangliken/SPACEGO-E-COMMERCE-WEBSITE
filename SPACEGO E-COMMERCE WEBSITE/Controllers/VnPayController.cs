using Microsoft.AspNetCore.Mvc;
using SPACEGO_E_COMMERCE_WEBSITE.Models;
using SPACEGO_E_COMMERCE_WEBSITE.Services.VNPAY;

public class VnPayController : Controller
{
    private readonly IVnPayService _vnPayService;

    public VnPayController(IVnPayService vnPayService)
    {
        _vnPayService = vnPayService;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public IActionResult CreatePayment(PaymentInformationModel model)
    {
        var paymentUrl = _vnPayService.CreatePaymentUrl(model, HttpContext);
        return Redirect(paymentUrl);
    }

    [HttpGet]
    public IActionResult PaymentReturn()
    {
        var response = _vnPayService.PaymentExecute(Request.Query);

        var responseCode = response.VnPayData.ContainsKey("vnp_ResponseCode") ? response.VnPayData["vnp_ResponseCode"] : "";
        var transactionStatus = response.VnPayData.ContainsKey("vnp_TransactionStatus") ? response.VnPayData["vnp_TransactionStatus"] : "";

        if (response.Success && responseCode == "00" && transactionStatus == "00")
        {
            ViewBag.Message = "✅ Thanh toán thành công!";

        }
        else
        {
            ViewBag.Message = "❌ Thanh toán thất bại!";
        }

        return View(response);
    }


}
