using SPACEGO_E_COMMERCE_WEBSITE.Models;

namespace SPACEGO_E_COMMERCE_WEBSITE.Repository
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetAllAsync();
        Task<Product> GetByIdAsync(int id);
        Task AddAsync(Product product);
        Task UpdateAsync(Product product);
        Task DeleteAsync(int id);
        Task<Product> GetProductWithDetailsAsync(int id); // ✅ Đúng chỗ

        Task<IEnumerable<Product>> GetTopSellingProductsAsync(int count);
        Task<Dictionary<string, List<Product>>> GetProductsGroupedByBrandAsync();
    }
}
