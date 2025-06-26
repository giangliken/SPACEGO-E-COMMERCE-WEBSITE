using Microsoft.AspNetCore.Mvc;
using SPACEGO_E_COMMERCE_WEBSITE.Models;
using SPACEGO_E_COMMERCE_WEBSITE.Repository;

namespace SPACEGO_E_COMMERCE_WEBSITE.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserRepository _userRepository;

        public UserController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<IActionResult> Index(string searchString)
        {
            var users = await _userRepository.GetUsersAsync();
            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();
                users = users.Where(u =>
                    (!string.IsNullOrEmpty(u.FullName) && u.FullName.ToLower().Contains(searchString)) ||
                    (!string.IsNullOrEmpty(u.PhoneNumber) && u.PhoneNumber.Contains(searchString)) ||
                    (!string.IsNullOrEmpty(u.Email) && u.Email.ToLower().Contains(searchString))
                ).ToList();
            }
            return View(users);
        }

        // GET: Create
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ApplicationUser user)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _userRepository.CreateUserWithDefaultPasswordAsync(user); // ✅ GỌI ĐÚNG
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi khi tạo user: " + ex.Message);
                }
            }
            return View(user);
        }


        // GET: Edit
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userRepository.GetUserByIdAsync(id);
            if (user == null) return NotFound();
            return View(user);
        }

        // POST: Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ApplicationUser user)
        {
            if (ModelState.IsValid)
            {
                await _userRepository.UpdateUserAsync(user);
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // GET: Details
        public async Task<IActionResult> Details(string id)
        {
            var user = await _userRepository.GetUserByIdAsync(id);
            if (user == null) return NotFound();
            return View(user);
        }

        // GET: Delete
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userRepository.GetUserByIdAsync(id);
            if (user == null) return NotFound();
            return View(user);
        }

        // POST: Delete
        // POST: Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            await _userRepository.DeleteUserAsync(id);
            return RedirectToAction(nameof(Index));
        }

    }
}
