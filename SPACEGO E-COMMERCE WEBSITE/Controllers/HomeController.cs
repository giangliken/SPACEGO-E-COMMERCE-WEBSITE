using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SPACEGO_E_COMMERCE_WEBSITE.Models;

namespace SPACEGO_E_COMMERCE_WEBSITE.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationUserManager _userManager;

        public HomeController(ILogger<HomeController> logger, ApplicationUserManager userManager)
        {
            _logger = logger;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var user = _userManager.GetUserAsync(User).Result;
            if (user != null)
            {
                var fullname = user.FullName;
                // code khác
            }
            return View();
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
