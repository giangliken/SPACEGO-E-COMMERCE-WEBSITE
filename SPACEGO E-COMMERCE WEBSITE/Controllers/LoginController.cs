using Microsoft.AspNetCore.Mvc;

namespace SPACEGO_E_COMMERCE_WEBSITE.Controllers
{
    public class LoginController : Controller
    {
        public IActionResult Login()
        {
            return View();
        }
    }
}
