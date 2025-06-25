using Microsoft.EntityFrameworkCore;
using SPACEGO_E_COMMERCE_WEBSITE.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SPACEGO_E_COMMERCE_WEBSITE.Repository
{
    public class EFColorRepository : IColorRepository
    {
        private readonly ApplicationDbContext _context;

        public EFColorRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Color>> GetAllAsync()
        {
            return await _context.Set<Color>().ToListAsync();
        }

        public async Task<Color?> GetByIdAsync(int colorId)
        {
            return await _context.Set<Color>().FindAsync(colorId);
        }

        public async Task AddAsync(Color color)
        {
            _context.Set<Color>().Add(color);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Color color)
        {
            _context.Set<Color>().Update(color);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int colorId)
        {
            var color = await _context.Set<Color>().FindAsync(colorId);
            if (color != null)
            {
                _context.Set<Color>().Remove(color);
                await _context.SaveChangesAsync();
            }
        }
    }
}