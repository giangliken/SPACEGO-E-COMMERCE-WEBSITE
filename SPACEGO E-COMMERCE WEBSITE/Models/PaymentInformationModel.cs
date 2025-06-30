namespace SPACEGO_E_COMMERCE_WEBSITE.Models
{
    public class PaymentInformationModel
    {
        public string Name { get; set; }
        public string OrderType { get; set; }
        public double Amount { get; set; }
        public string OrderDescription { get; set; }
        public int OrderId { get; set; }
    }
}
