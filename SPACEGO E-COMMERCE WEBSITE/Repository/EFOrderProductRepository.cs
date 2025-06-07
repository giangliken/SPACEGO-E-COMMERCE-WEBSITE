using Microsoft.EntityFrameworkCore;
using SPACEGO_E_COMMERCE_WEBSITE.Models;

namespace SPACEGO_E_COMMERCE_WEBSITE.Repository
{
    public class EFOrderProductRepository : IOrderProductRepository
    {
        private readonly ApplicationDbContext _context;

        public EFOrderProductRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<OrderProduct>> GetAllAsync()
        {
            return await _context.OrderProducts
                .Include(op => op.Order)
                .Include(op => op.Product)
                .ToListAsync();
        }

        public async Task<IEnumerable<OrderProduct>> GetByOrderIdAsync(int orderId)
        {
            return await _context.OrderProducts
                .Where(op => op.OrderId == orderId)
                .Include(op => op.Product)
                .ToListAsync();
        }

        public async Task AddAsync(OrderProduct orderProduct)
        {
            _context.OrderProducts.Add(orderProduct);
            await _context.SaveChangesAsync();
        }

        public async Task AddRangeAsync(IEnumerable<OrderProduct> orderProducts)
        {
            _context.OrderProducts.AddRange(orderProducts);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int orderId, int productId)
        {
            var entity = await _context.OrderProducts
                .FirstOrDefaultAsync(op => op.OrderId == orderId && op.ProductId == productId);
            if (entity != null)
            {
                _context.OrderProducts.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }
}
