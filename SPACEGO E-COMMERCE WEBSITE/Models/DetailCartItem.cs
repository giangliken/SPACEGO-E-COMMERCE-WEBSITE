namespace SPACEGO_E_COMMERCE_WEBSITE.Models
{
    public class DetailCartItem
    {
        public int CartItemId { get; set; }
        public CartItem? CartItem { get; set; }
        public int ProductId { get; set; }
        public Product? Product { get; set; }
        public int Quanity { get; set; }
        public decimal? TotalPriceProduct { get; set; }

    }
}
