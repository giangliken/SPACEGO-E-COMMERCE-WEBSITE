namespace SPACEGO_E_COMMERCE_WEBSITE.Repository
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using SPACEGO_E_COMMERCE_WEBSITE.Models;

    public interface IProductVariantRepository
    {
        Task<IEnumerable<ProductVariant>> GetAllAsync();
        Task<ProductVariant?> GetByIdAsync(int id);
        Task<ProductVariant?> LayBienTheTheoID(int? id);
        Task AddAsync(ProductVariant variant);
        Task UpdateAsync(ProductVariant variant);
        Task DeleteAsync(int id);
        Task<IEnumerable<ProductVariant>> GetByProductIdAsync(int productId);
    }
}