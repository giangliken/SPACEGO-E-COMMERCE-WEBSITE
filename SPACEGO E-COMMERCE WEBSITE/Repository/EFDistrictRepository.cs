using Microsoft.EntityFrameworkCore;
using SPACEGO_E_COMMERCE_WEBSITE.Models;

namespace SPACEGO_E_COMMERCE_WEBSITE.Repository
{
    public class EFDistrictRepository : IDistrictRepository
    {
        private readonly ApplicationDbContext _context;
        public EFDistrictRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<District>> GetAllAsync()
        {
            return await _context.Districts.ToListAsync();
        }

        public async Task<IEnumerable<District>> GetByProvinceIdAsync(int provinceId)
        {
            return await _context.Districts
                .Where(d => d.ProvinceID == provinceId)
                .ToListAsync();
        }
    }
}
