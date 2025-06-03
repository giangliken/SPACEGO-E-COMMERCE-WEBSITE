using Microsoft.AspNetCore.Identity;

namespace SPACEGO_E_COMMERCE_WEBSITE.Models
{
    public class ApplicationUser: IdentityUser
    {
        public string FullName { get; set; }
        public string? Address { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public bool? Gender {  get; set; }   
        public DateTime CreatedDate { get; set; }
        public string? AvatarUrl { get; set; }
        public decimal? LoyaltyPoints { get; set; }
        public bool? IsActive { get; set; }

    }
}
