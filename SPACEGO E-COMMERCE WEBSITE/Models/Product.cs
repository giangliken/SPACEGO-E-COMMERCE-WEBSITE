using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SPACEGO_E_COMMERCE_WEBSITE.Models
{
    public class Product
    {
        [Required]
        public int ProductId { get; set; }
        [Required(ErrorMessage ="Tên sản phẩm là bắt buộc")]
        [StringLength(100)]
        [DisplayName("Tên Sản Phẩm")]
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
        [Range(0, 1000000000)]
        public decimal? ProductPrice { get; set; }
        public int ProductQuantity { get; set; }
        public int CategoryId { get; set; }
        public Category? Category { get; set; }
        public int BrandId { get; set; }
        public Brand? Brand { get; set; }
        public string? ImageUrl {  get; set; }
        public List<ProductImage> ImageUrls { get; set; }
        public bool isAvailable  { get; set; }
    }
}
