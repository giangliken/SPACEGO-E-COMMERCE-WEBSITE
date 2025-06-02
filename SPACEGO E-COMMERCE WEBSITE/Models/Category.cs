using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SPACEGO_E_COMMERCE_WEBSITE.Models
{
    public class Category
    {
        public int CategoryId { get; set; }

        [Required(ErrorMessage ="Tên danh mục là bắt buộc")]
        [MaxLength(50,ErrorMessage ="Tên danh mục không được vượt quá 50 ký tự")]
        [DisplayName("Tên danh mục")]
        public string CategoryName { get; set; }
        
        public string? ImageUrl { get; set; }
    }
}
