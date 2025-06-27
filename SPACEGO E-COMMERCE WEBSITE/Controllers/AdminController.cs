using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SPACEGO_E_COMMERCE_WEBSITE.Models;

namespace SPACEGO_E_COMMERCE_WEBSITE.Controllers
{
    [Authorize(SD.Role_Admin)]

    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> ActivityLog()
        {
            var logs = await _context.ActivityLogs
                .OrderByDescending(l => l.Timestamp)
                .Take(100)
                .ToListAsync();

            return View(logs);
        }


    }
}
