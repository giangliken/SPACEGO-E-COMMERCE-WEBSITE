using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SPACEGO_E_COMMERCE_WEBSITE.Models;

namespace SPACEGO_E_COMMERCE_WEBSITE.Repository
{
    public class EFProductVariantRepository : IProductVariantRepository
    {
        private readonly ApplicationDbContext _context;

        public EFProductVariantRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(ProductVariant variant)
        {
            await _context.ProductVariants.AddAsync(variant);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var variant = await _context.ProductVariants.FindAsync(id);
            if (variant != null)
            {
                _context.ProductVariants.Remove(variant);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<ProductVariant>> GetAllAsync()
        {
            return await _context.ProductVariants
                .Include(v => v.Color)
                .Include(v => v.Capacity)
                .Include(v => v.Product)
                .ToListAsync();
        }

        public async Task<ProductVariant?> GetByIdAsync(int id)
        {
            return await _context.ProductVariants
                .Include(v => v.Color)
                .Include(v => v.Capacity)
                .Include(v => v.Product)
                .FirstOrDefaultAsync(v => v.ProductVariantId == id);
        }

        public async Task<IEnumerable<ProductVariant>> GetByProductIdAsync(int productId)
        {
            return await _context.ProductVariants
                .Where(v => v.ProductId == productId)
                .Include(v => v.Color)
                .Include(v => v.Capacity)
                .ToListAsync();
        }

        public async Task UpdateAsync(ProductVariant variant)
        {
            _context.ProductVariants.Update(variant); // or Entry(variant).State = Modified
            await _context.SaveChangesAsync();
        }

    }
}