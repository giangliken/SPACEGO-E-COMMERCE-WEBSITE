using Microsoft.EntityFrameworkCore;
using SPACEGO_E_COMMERCE_WEBSITE.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SPACEGO_E_COMMERCE_WEBSITE.Repository
{
    public class EFCapacityRepository : ICapacityRepository
    {
        private readonly ApplicationDbContext _context;

        public EFCapacityRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Capacity>> GetAllAsync()
        {
            return await _context.Capacities.ToListAsync();
        }

        public async Task<Capacity> GetByIdAsync(int id)
        {
            return await _context.Capacities.FindAsync(id);
        }

        public async Task AddAsync(Capacity capacity)
        {
            _context.Capacities.Add(capacity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Capacity capacity)
        {
            _context.Capacities.Update(capacity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var capacity = await _context.Capacities.FindAsync(id);
            if (capacity != null)
            {
                _context.Capacities.Remove(capacity);
                await _context.SaveChangesAsync();
            }
        }
    }
}