namespace SPACEGO_E_COMMERCE_WEBSITE.Models.ViewModel
{
    public class WarehouseItemViewModel
    {
        public int? VariantId { get; set; } // null nếu là sản phẩm không biến thể
        public string ProductName { get; set; }

        public string ProductId { get; set; }
        public string VariantDisplay { get; set; } // "Màu - Dung lượng" hoặc "-"
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string Status { get; set; } // Còn hàng, Sắp hết, Hết hàng

    }
}
