using Microsoft.EntityFrameworkCore;
using SPACEGO_E_COMMERCE_WEBSITE.Models;

namespace SPACEGO_E_COMMERCE_WEBSITE.Repository
{
    public class EFCartItemRepository : ICartItemRepository
    {
        private readonly ApplicationDbContext _context;

        public EFCartItemRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CartItem>> GetAllAsync()
        {
            return await _context.CartItems
                .Include(c => c.User)
                .Include(c => c.DetailCartItems)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<CartItem> GetByIdAsync(int id)
        {
            return await _context.CartItems
                .Include(c => c.User)
                .Include(c => c.DetailCartItems)
                .FirstOrDefaultAsync(c => c.CartItemId == id);
        }

        public async Task<IEnumerable<CartItem>> GetByUserIdAsync(string userId)
        {
            return await _context.CartItems
                .Include(c => c.DetailCartItems)
                .Where(c => c.UserId == userId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task AddAsync(CartItem cartItem)
        {
            _context.CartItems.Add(cartItem);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(CartItem cartItem)
        {
            _context.CartItems.Update(cartItem);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var cartItem = await _context.CartItems.FindAsync(id);
            if (cartItem != null)
            {
                _context.CartItems.Remove(cartItem);
                await _context.SaveChangesAsync();
            }
        }
    }
}
