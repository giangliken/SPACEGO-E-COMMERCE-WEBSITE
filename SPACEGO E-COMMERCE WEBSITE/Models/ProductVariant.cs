using System.Drawing;

namespace SPACEGO_E_COMMERCE_WEBSITE.Models
{
    public class ProductVariant
    {
        public int ProductVariantId { get; set; }
        public int ProductId { get; set; }
        public Product? Product { get; set; }

        public int? ColorId { get; set; }
        public Color? Color { get; set; }

        public int? CapacityId { get; set; }
        public Capacity? Capacity { get; set; }

        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }

    // Thuộc tính màu sắc
    public class Color
    {
        public int ColorId { get; set; }
        public string ColorName { get; set; }
    }

    // Thuộc tính dung lượng
    public class Capacity
    {
        public int CapacityId { get; set; }
        public string CapacityName { get; set; } // VD: "128GB", "256GB"
    }
}
