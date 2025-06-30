using Microsoft.EntityFrameworkCore;
using SPACEGO_E_COMMERCE_WEBSITE.Models;

namespace SPACEGO_E_COMMERCE_WEBSITE.Repository
{
    public class EFOrderRepository : IOrderRepository
    {
        private readonly ApplicationDbContext _context;

        public EFOrderRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Order>> GetAllAsync()
        {
            return await _context.Orders
                .Include(o => o.OrderProducts)
                    .ThenInclude(op => op.Product)
                .Include(o => o.User)
                .ToListAsync();
        }

        public async Task<Order> GetByIdAsync(int orderId)
        {
            return await _context.Orders
                .Include(o => o.OrderProducts)
                    .ThenInclude(op => op.Product)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);
        }

        public async Task<IEnumerable<Order>> GetByUserIdAsync(string userId)
        {
            return await _context.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.OrderProducts)
                    .ThenInclude(op => op.Product)
                .Include(o => o.User)
                .ToListAsync();
        }

        public async Task AddAsync(Order order)
        {
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Order order)
        {
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order != null)
            {
                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> HasUserPurchasedProductAsync(string userId, int productId)
        {
            if (string.IsNullOrEmpty(userId))
                return false;

            return await _context.OrderProducts
                .Include(op => op.Order)
                .AnyAsync(op => op.ProductId == productId
                             && op.Order.UserId == userId
                             && op.Order.OrderStatus == "Hoàn tất"); // <-- dùng đúng property
        }
        public async Task<List<Order>> GetOrdersByUserIdAsync(string userId)
        {
            return await _context.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.OrderProducts)
                    .ThenInclude(op => op.Product)
                .Include(o => o.OrderProducts)
                    .ThenInclude(op => op.ProductVariant)
                        .ThenInclude(pv => pv.Product)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }
        public async Task UpdateStatusAsync(int orderId, string status)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order != null)
            {
                order.OrderStatus = status;
                await _context.SaveChangesAsync();
            }
        }
        public async Task<List<Order>> GetOrdersByIdsAsync(List<int> orderIds)
        {
            return await _context.Orders
                .Where(o => orderIds.Contains(o.OrderId))
                .Include(o => o.OrderProducts) // nếu cần thông tin sản phẩm trong đơn
                .ThenInclude(op => op.Product) // nếu cần luôn tên sản phẩm
                .ToListAsync();
        }
    }
}
