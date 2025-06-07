using Microsoft.EntityFrameworkCore;
using SPACEGO_E_COMMERCE_WEBSITE.Models;

namespace SPACEGO_E_COMMERCE_WEBSITE.Repository
{
    public class EFDetailCartItemRepository : IDetailCartItemRepository
    {
        private readonly ApplicationDbContext _context;

        public EFDetailCartItemRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<DetailCartItem>> GetAllAsync()
        {
            return await _context.DetailsCartItems
                .Include(d => d.Product)
                .Include(d => d.CartItem)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<DetailCartItem> GetByIdsAsync(int cartItemId, int productId)
        {
            return await _context.DetailsCartItems
                .Include(d => d.Product)
                .FirstOrDefaultAsync(d => d.CartItemId == cartItemId && d.ProductId == productId);
        }

        public async Task<IEnumerable<DetailCartItem>> GetByCartItemIdAsync(int cartItemId)
        {
            return await _context.DetailsCartItems
                .Where(d => d.CartItemId == cartItemId)
                .Include(d => d.Product)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task AddAsync(DetailCartItem detail)
        {
            _context.DetailsCartItems.Add(detail);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(DetailCartItem detail)
        {
            _context.DetailsCartItems.Update(detail);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int cartItemId, int productId)
        {
            var detail = await _context.DetailsCartItems
                .FirstOrDefaultAsync(d => d.CartItemId == cartItemId && d.ProductId == productId);

            if (detail != null)
            {
                _context.DetailsCartItems.Remove(detail);
                await _context.SaveChangesAsync();
            }
        }
    }
}
