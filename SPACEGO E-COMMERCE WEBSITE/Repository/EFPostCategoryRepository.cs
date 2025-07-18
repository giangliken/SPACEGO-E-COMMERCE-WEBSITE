﻿using Microsoft.EntityFrameworkCore;
using SPACEGO_E_COMMERCE_WEBSITE.Models;

namespace SPACEGO_E_COMMERCE_WEBSITE.Repository
{
    public class EFPostCategoryRepository : IPostCategoryRepository
    {
        private readonly ApplicationDbContext _context;

        public EFPostCategoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PostCategory>> GetAllAsync()
        {
            return await _context.PostCategories.ToListAsync();
        }

        public async Task<PostCategory?> GetByIdAsync(int id)
        {
            return await _context.PostCategories.FindAsync(id);
        }

        public async Task AddAsync(PostCategory category)
        {
            _context.PostCategories.Add(category);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(PostCategory category)
        {
            _context.PostCategories.Update(category);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var category = await GetByIdAsync(id);
            if (category != null)
            {
                _context.PostCategories.Remove(category);
                await _context.SaveChangesAsync();
            }
        }
    }
}
