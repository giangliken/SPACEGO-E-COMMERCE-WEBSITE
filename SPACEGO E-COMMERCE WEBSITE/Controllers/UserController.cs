using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using SPACEGO_E_COMMERCE_WEBSITE.Models;
using SPACEGO_E_COMMERCE_WEBSITE.Models.ViewModel;
using SPACEGO_E_COMMERCE_WEBSITE.Repository;
using IEmailSender = SPACEGO_E_COMMERCE_WEBSITE.Repository.IEmailSender;

namespace SPACEGO_E_COMMERCE_WEBSITE.Controllers
{
    [Authorize(SD.Role_Admin)]

    public class UserController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        public UserController(IUserRepository userRepository, UserManager<ApplicationUser> userManager, IEmailSender emailSender)
        {
            _userRepository = userRepository;
            _userManager = userManager;
            _emailSender = emailSender;
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

        // GET: ResetPassword
        public async Task<IActionResult> ResetPassword(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            return View(new ResetPasswordViewModel
            {
                UserId = user.Id,
                Email = user.Email
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("ResetPassword")]
        public async Task<IActionResult> ResetPasswordConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, "Abc@123");

            if (result.Succeeded)
            {
                // ✅ Gửi email sau khi reset thành công
                string subject = "Mật khẩu của bạn đã được đặt lại";
                string body = $@"
                <p>Chào {user.FullName ?? user.Email},</p>
                <p>Mật khẩu của bạn đã được đặt lại bởi quản trị viên.</p>
                <p><strong>Tên đăng nhập:</strong>{user.Email}</p>
                <p><strong>Mật khẩu mới:</strong> Abc@123</p>
                <p>Vui lòng đăng nhập và đổi mật khẩu ngay nếu cần.</p>
                <p>Trân trọng,<br>CÔNG TY TNHH XUẤT NHẬP KHẨU SPACEGO</p>
                ";

                await _emailSender.SendEmailAsync(user.Email, subject, body);

                TempData["Success"] = $"Reset mật khẩu thành công cho {user.Email}! Mật khẩu mới đã được gửi qua email.";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(new ResetPasswordViewModel { UserId = user.Id, Email = user.Email });
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
