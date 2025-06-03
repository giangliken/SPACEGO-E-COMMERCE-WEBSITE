using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SPACEGO_E_COMMERCE_WEBSITE.Models;

namespace SPACEGO_E_COMMERCE_WEBSITE.Controllers
{
    public class AdminController : Controller
    {
        [Authorize(SD.Role_Admin)]
        public IActionResult Index()
        {
            return View();
        }
    }
}
