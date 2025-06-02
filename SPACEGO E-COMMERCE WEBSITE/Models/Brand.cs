using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SPACEGO_E_COMMERCE_WEBSITE.Models
{
    public class Brand
    {
        public int BrandId { get; set; }

        [Required(ErrorMessage ="Tên thương hiệu là bắt buộc")]
        [MaxLength(50,ErrorMessage ="Tên thương hiệu không được vượt quá 50 ký tự")]
        [DisplayName("Tên thương hiệu")]
        public string BrandName { get; set; }

        public string BrandDescription { get; set; }
        public string ImageUrl { get; set; }
    }
}
