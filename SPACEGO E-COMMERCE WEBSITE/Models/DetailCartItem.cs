using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SPACEGO_E_COMMERCE_WEBSITE.Models
{
    public class DetailCartItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int CartItemId { get; set; }
        public CartItem? CartItem { get; set; }
        public int ProductId { get; set; }
        public Product? Product { get; set; }

        public int? ProductVariantId { get; set; }
        public ProductVariant? ProductVariant { get; set; }
        public int Quanity { get; set; }
        public decimal Price { get; set; }

    }
}
