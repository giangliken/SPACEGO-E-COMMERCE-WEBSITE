using ClosedXML.Excel;
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
    //[Authorize(SD.Role_Admin)]

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
            ViewBag.Roles = new[] { SD.Role_Admin, SD.Role_Customer, SD.Role_Employee}; // Danh sách quyền hạn
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ApplicationUser user, string role)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var result = await _userRepository.CreateUserWithDefaultPasswordAsync(user); // ✅ GỌI ĐÚNG
                    if (result)
                    {
                        // Gán quyền hạn cho người dùng
                        if (!string.IsNullOrEmpty(role))
                        {
                            await _userManager.AddToRoleAsync(user, role);
                        }
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError("", "Tạo tài khoản không thành công. Vui lòng kiểm tra lại thông tin.");

                    }
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
            ViewBag.Roles = new[] { SD.Role_Admin, SD.Role_Customer, SD.Role_Employee }; // Danh sách quyền hạn
            var user = await _userRepository.GetUserByIdAsync(id);
            if (user == null) return NotFound();
            // Lấy thông tin quyền hạn hiện tại của người dùng
            var userRoles = await _userManager.GetRolesAsync(user);
            ViewBag.CurrentRoles = userRoles;
            return View(user);
        }

        // POST: Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ApplicationUser user, string role)
        {
            if (ModelState.IsValid)
            {
                var userRoles = await _userManager.GetRolesAsync(user);

                // Nếu người dùng chọn thay đổi quyền hạn và quyền mới khác với quyền hiện tại
                if (!string.IsNullOrEmpty(role) && !userRoles.Contains(role))
                {
                    // Xóa tất cả quyền hạn hiện tại của người dùng
                    foreach (var roleItem in userRoles)
                    {
                        await _userManager.RemoveFromRoleAsync(user, roleItem);
                    }

                    // Thêm quyền hạn mới cho người dùng
                    await _userManager.AddToRoleAsync(user, role);
                }
                else if (!string.IsNullOrEmpty(role) && userRoles.Contains(role))
                {
                    // Nếu quyền mới giống với quyền hiện tại, không cần làm gì
                }
                else if (string.IsNullOrEmpty(role) && userRoles.Count > 0)
                {
                    // Xóa tất cả quyền hạn hiện tại của người dùng
                    foreach (var roleItem in userRoles)
                    {
                        await _userManager.RemoveFromRoleAsync(user, roleItem);
                    }
                }

                await _userRepository.UpdateUserAsync(user);
                TempData["Success"] = "Cập nhật người dùng thành công!";
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
            TempData["Success"] = "Xóa người dùng thành công!";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> ExportToExcel()
        {
            var users = await _userRepository.GetUsersAsync();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Người dùng");

                // Header
                worksheet.Cell(1, 1).Value = "STT";
                worksheet.Cell(1, 2).Value = "UserID";
                worksheet.Cell(1, 3).Value = "UserName";
                worksheet.Cell(1, 4).Value = "Họ tên";
                worksheet.Cell(1, 5).Value = "Số điện thoại";
                worksheet.Cell(1, 6).Value = "Email";
                worksheet.Cell(1, 7).Value = "Trạng thái";
                worksheet.Cell(1, 8).Value = "Quyền hạn";

                int row = 2;
                int stt = 1;

                int count = 0;
                foreach (var user in users)
                {
                    count++;
                    var roles = await _userManager.GetRolesAsync(user);
                    var role = roles.FirstOrDefault() ?? "Không rõ";

                    string roleName = role switch
                    {
                        SD.Role_Admin => "Quản trị viên",
                        SD.Role_Customer => "Khách hàng",
                        SD.Role_Employee => "Nhân viên",
                        _ => "Khác"
                    };

                    string status = user.IsActive ? "Tạm khóa" : "Hoạt động";

                    worksheet.Cell(row, 1).Value = stt++;
                    worksheet.Cell(row, 2).Value = user.Id;
                    worksheet.Cell(row, 3).Value = user.UserName;
                    worksheet.Cell(row, 4).Value = user.FullName;
                    worksheet.Cell(row, 5).Value = user.PhoneNumber;
                    worksheet.Cell(row, 6).Value = user.Email;
                    worksheet.Cell(row, 7).Value = status;
                    worksheet.Cell(row, 8).Value = roleName;
                    row++;
                }

                // ✅ Sau vòng lặp foreach, thêm dòng tổng cuối
                worksheet.Cell(row + 1, 1).Value = $"Tổng số người dùng: {count}";
                worksheet.Range(row + 1, 1, row + 1, 8).Merge(); // Gộp 8 cột để hiển thị full dòng
                worksheet.Cell(row + 1, 1).Style.Font.Bold = true;
                worksheet.Cell(row + 1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;


                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    stream.Position = 0;

                    string fileName = $"DanhSachNguoiDung_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
        }

    }
}
