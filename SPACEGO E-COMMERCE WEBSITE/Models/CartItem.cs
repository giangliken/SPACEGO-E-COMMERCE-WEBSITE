namespace SPACEGO_E_COMMERCE_WEBSITE.Models
{
    public class CartItem
    {
        public int CartItemId { get; set; }
        public decimal? TotalPrice { get; set; }
        //public string UserId { get; set; }
        //public ApplicationUser? User { get; set; }
        public List<DetailCartItem> DetailCartItems { get; set; }
    }
}
