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
        public string? ProductDescription { get; set; }
        [Required(ErrorMessage ="Giá sản phẩm là bắt buộc")]
        [Range(0, 1000000000)]
        [DisplayName("Giá Sản Phẩm")]
        public decimal ProductPrice { get; set; }
        [Required(ErrorMessage ="Số lượng sản phẩm là bắt buộc")]
        [Range(1, int.MaxValue, ErrorMessage = "...")]
        [DisplayName("Số Lượng Sản Phẩm")]
        public int ProductQuantity { get; set; }
        [Required(ErrorMessage ="Mã danh mục là bắt buộc")]
        [DisplayName("Mã Danh Mục")]
        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        [Required(ErrorMessage ="Mã thương hiệu là bắt buộc")]
        [DisplayName("Mã Thương Hiệu")]
        public int BrandId { get; set; }
        public Brand? Brand { get; set; }
        public string? ImageUrl {  get; set; }
        public List<ProductImage>? ImageUrls { get; set; }
        public bool isAvailable  { get; set; }
    }
}
