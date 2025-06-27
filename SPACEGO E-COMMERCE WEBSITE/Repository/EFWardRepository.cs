using Microsoft.EntityFrameworkCore;
using SPACEGO_E_COMMERCE_WEBSITE.Models;

namespace SPACEGO_E_COMMERCE_WEBSITE.Repository
{
    public class EFWardRepository : IWardRepository
    {
        private readonly ApplicationDbContext _context;
        public EFWardRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Ward>> GetAllAsync()
        {
            return await _context.Wards.ToListAsync();
        }

        public async Task<IEnumerable<Ward>> GetByDistrictIdAsync(int districtId)
        {
            return await _context.Wards
                .Where(w => w.DistrictID == districtId)
                .ToListAsync();
        }
    }
}
