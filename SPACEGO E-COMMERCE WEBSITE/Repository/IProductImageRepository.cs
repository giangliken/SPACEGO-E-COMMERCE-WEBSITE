using SPACEGO_E_COMMERCE_WEBSITE.Models;

namespace SPACEGO_E_COMMERCE_WEBSITE.Repository
{
    public interface IProductImageRepository
    {
        Task<IEnumerable<ProductImage>> GetAllAsync();
        Task<ProductImage> GetByIdAsync(int id);
        Task<IEnumerable<ProductImage>> GetByProductIdAsync(int productId);
        Task AddAsync(ProductImage productImage);
        Task DeleteAsync(int id);
    }
}
