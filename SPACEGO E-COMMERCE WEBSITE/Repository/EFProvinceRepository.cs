using Microsoft.EntityFrameworkCore;
using SPACEGO_E_COMMERCE_WEBSITE.Models;

namespace SPACEGO_E_COMMERCE_WEBSITE.Repository
{
    public class EFProvinceRepository : IProvinceRepository
    {
        private readonly ApplicationDbContext _context;
        public EFProvinceRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Province>> GetAllAsync()
        {
            return await _context.Provinces.ToListAsync();
        }
    }
}
