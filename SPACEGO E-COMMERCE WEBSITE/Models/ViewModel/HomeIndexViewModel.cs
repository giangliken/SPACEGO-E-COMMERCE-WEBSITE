namespace SPACEGO_E_COMMERCE_WEBSITE.Models.ViewModel
{
    public class HomeIndexViewModel
    {
        public IEnumerable<Product> Products { get; set; }
        public IEnumerable<Category> Categories { get; set; }
        public IEnumerable<Brand> Brands { get; set; }
    }
}
