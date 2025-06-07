using SPACEGO_E_COMMERCE_WEBSITE.Models;

namespace SPACEGO_E_COMMERCE_WEBSITE.Repository
{
    public interface IOrderProductRepository
    {
        Task<IEnumerable<OrderProduct>> GetAllAsync();
        Task<IEnumerable<OrderProduct>> GetByOrderIdAsync(int orderId);
        Task AddAsync(OrderProduct orderProduct);
        Task AddRangeAsync(IEnumerable<OrderProduct> orderProducts);
        Task DeleteAsync(int orderId, int productId);
    }
}
