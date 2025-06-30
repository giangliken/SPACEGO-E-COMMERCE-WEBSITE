using Microsoft.AspNetCore.Http;
using SPACEGO_E_COMMERCE_WEBSITE.Libraries;
using SPACEGO_E_COMMERCE_WEBSITE.Models;

namespace SPACEGO_E_COMMERCE_WEBSITE.Services.VNPAY
{
    public interface IVnPayService
    {
        string CreatePaymentUrl(PaymentInformationModel model, HttpContext context);
        PaymentResponseModel PaymentExecute(IQueryCollection collection);
    }
}
