// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SPACEGO_E_COMMERCE_WEBSITE.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace SPACEGO_E_COMMERCE_WEBSITE.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public IndexModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            /// 
            [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
            [Phone(ErrorMessage ="Định dạng số điện thoại không hợp lệ")]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }
            [Required(ErrorMessage = "Họ và tên là bắt buộc")]
            [StringLength(100, ErrorMessage = "Họ và tên phải có ít nhất {2} ký tự và tối đa {1} ký tự.", MinimumLength = 2)]
            [Display(Name = "Họ và tên")]
            public string FullName { get; set; }

            [Display(Name = "Ảnh đại diện")]
            public IFormFile? AvatarImage { get; set; } 

            public string? AvatarUrl { get; set; } 
        }



        private async Task LoadAsync(ApplicationUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            var fullname = user.FullName;
            var avatarUrl = user.AvatarUrl;

            Username = userName;

            Input = new InputModel
            {
                PhoneNumber = phoneNumber,
                FullName = fullname,
                AvatarUrl = avatarUrl,
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, Input.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set phone number.";
                    return RedirectToPage();
                }
            }
            user.FullName = Input.FullName;

            // Nếu người dùng upload ảnh mới
            if (Input.AvatarImage != null && Input.AvatarImage.Length > 0)
            {
                var avatarUrl = await SaveImage(Input.AvatarImage, user.UserName);
                user.AvatarUrl = avatarUrl;
            }
            await _userManager.UpdateAsync(user);
            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Cập nhật thông tin thành công";
            return RedirectToPage();
        }

        private async Task<string> SaveImage(IFormFile image, string us)
        {
            var folderPath = Path.Combine("wwwroot", "assets", "images", "avatar");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            // Đặt tên file mới để tránh trùng lặp, ví dụ: userId + đuôi file gốc
            var extension = Path.GetExtension(image.FileName);
            var newFileName = $"{us}{extension}";
            var savePath = Path.Combine(folderPath, newFileName);

            using (var fileStream = new FileStream(savePath, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }

            // Trả về URL đúng để hiển thị ảnh
            return $"/assets/images/avatar/{newFileName}";
        }



    }
}
