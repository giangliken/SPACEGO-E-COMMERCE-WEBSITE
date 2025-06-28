using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace SPACEGO_E_COMMERCE_WEBSITE.Models
{
    public class ApplicationUser: IdentityUser
    {
        [Required(ErrorMessage = "Vui lòng nhập họ và tên")]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; }
        // Override lại PhoneNumber
        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [Phone(ErrorMessage ="Số điện thoại không đúng định dạng")]
        [Display(Name = "Số điện thoại")]
        public override string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public override string Email { get; set; }
        public string? Address { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public bool? Gender {  get; set; }   
        public DateTime CreatedDate { get; set; }
        public string? AvatarUrl { get; set; }
        public decimal? LoyaltyPoints { get; set; }
        public bool IsActive { get; set; } = false;

    }
}
