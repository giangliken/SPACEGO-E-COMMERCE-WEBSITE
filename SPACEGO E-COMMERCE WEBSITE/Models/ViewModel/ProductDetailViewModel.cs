namespace SPACEGO_E_COMMERCE_WEBSITE.Models.ViewModel
{
    public class ProductDetailViewModel
    {
        public Product Product { get; set; }
        public List<ProductImage> ProductImages { get; set; }
        public List<Comment> Comments { get; set; }
    }
}
