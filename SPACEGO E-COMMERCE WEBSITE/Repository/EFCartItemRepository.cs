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

        public async Task<CartItem> GetActiveCartByUserIdAsync(string userId)
        {
            return await _context.CartItems
                .Include(c => c.DetailCartItems)
                    .ThenInclude(d => d.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId );
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
        public async Task AddOrUpdateCartItemAsync(string userId, int productId, int quantity)
        {
            var cart = await _context.CartItems
                .Include(c => c.DetailCartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new CartItem
                {
                    UserId = userId,
                    DetailCartItems = new List<DetailCartItem>()
                };
                _context.CartItems.Add(cart);
            }

            var detail = cart.DetailCartItems.FirstOrDefault(d => d.ProductId == productId);
            if (detail != null)
            {
                detail.Quanity += quantity;
                // Cập nhật giá tiền nếu cần
            }
            else
            {
                detail = new DetailCartItem
                {
                    ProductId = productId,
                    Quanity = quantity
                };
                cart.DetailCartItems.Add(detail);
            }

            // Tính tổng tiền
            cart.TotalPrice = cart.DetailCartItems.Sum(d => d.Quanity * d.Product.ProductPrice);

            await _context.SaveChangesAsync();
        }
    }
}
