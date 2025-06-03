namespace SPACEGO_E_COMMERCE_WEBSITE.Models
{
    public class Order
    {
        public int OrderId { get; set; }
        public decimal? Total { get; set; }
        public DateTime? OrderDate { get; set; }
        public string ShippingAddress { get; set; }
        public string PaymentMethod { get; set; }
        public bool? OrderStatus { get; set; }
        public List<OrderProduct> OrderProducts { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }  
    }
}
