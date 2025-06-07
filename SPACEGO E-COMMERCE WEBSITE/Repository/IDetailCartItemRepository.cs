using SPACEGO_E_COMMERCE_WEBSITE.Models;

namespace SPACEGO_E_COMMERCE_WEBSITE.Repository
{
    public interface IDetailCartItemRepository
    {
        Task<IEnumerable<DetailCartItem>> GetAllAsync();
        Task<DetailCartItem> GetByIdsAsync(int cartItemId, int productId);
        Task<IEnumerable<DetailCartItem>> GetByCartItemIdAsync(int cartItemId);
        Task AddAsync(DetailCartItem detail);
        Task UpdateAsync(DetailCartItem detail);
        Task DeleteAsync(int cartItemId, int productId);
    }
}
