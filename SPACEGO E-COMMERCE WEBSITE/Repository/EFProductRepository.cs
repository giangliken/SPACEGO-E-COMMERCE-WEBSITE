﻿using Microsoft.EntityFrameworkCore;
using SPACEGO_E_COMMERCE_WEBSITE.Models;

namespace SPACEGO_E_COMMERCE_WEBSITE.Repository
{
    public class EFProductRepository : IProductRepository
    {
        private readonly ApplicationDbContext _context;

        public EFProductRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _context.Products
           .Include(p => p.Brand)
           .Include(p => p.Category)
           .Include(p => p.ImageUrls)
           .Include(p => p.Variants )
           .ToListAsync();
        }

        public async Task<Product> GetByIdAsync(int id)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.ImageUrls)
                .FirstOrDefaultAsync(p => p.ProductId == id);
        }

        public async Task AddAsync(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Product product)
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
        }
        public async Task<Product> GetProductWithDetailsAsync(int id)
        {
            return await _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .Include(p => p.ImageUrls)
                .Include(p => p.Variants).ThenInclude(v => v.Color)
                .Include(p => p.Variants).ThenInclude(v => v.Capacity)
                .FirstOrDefaultAsync(p => p.ProductId == id);
        }

        //top 8 san pham
        public async Task<IEnumerable<Product>> GetTopSellingProductsAsync(int count)
        {
            var topProductIds = await _context.OrderProducts
                .GroupBy(op => op.ProductId)
                .OrderByDescending(g => g.Count())
                .Take(count)
                .Select(g => g.Key)
                .ToListAsync();

            return await _context.Products
                .Include(p => p.Brand)
                .Where(p => topProductIds.Contains(p.ProductId) && p.isAvailable)
                .ToListAsync();
        }
        //san pham theo nhan hang
        public async Task<Dictionary<string, List<Product>>> GetProductsGroupedByBrandAsync()
        {
            return await _context.Products
                .Include(p => p.Brand)
                .Where(p => p.isAvailable)
                .GroupBy(p => p.Brand.BrandName)
                .ToDictionaryAsync(g => g.Key, g => g.ToList());
        }

    }
}
