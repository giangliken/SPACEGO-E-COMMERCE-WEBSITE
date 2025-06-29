using SPACEGO_E_COMMERCE_WEBSITE.Models;

namespace SPACEGO_E_COMMERCE_WEBSITE.Repository
{
    public interface IOrderRepository
    {
        Task<IEnumerable<Order>> GetAllAsync();
        Task<Order> GetByIdAsync(int orderId);
        Task<IEnumerable<Order>> GetByUserIdAsync(string userId);
        Task AddAsync(Order order);
        Task UpdateAsync(Order order);
        Task DeleteAsync(int orderId);
        Task<bool> HasUserPurchasedProductAsync(string userId, int productId);
        Task<List<Order>> GetOrdersByUserIdAsync(string userId);

    }
}
