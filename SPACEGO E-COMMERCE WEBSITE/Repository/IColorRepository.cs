using System.Collections.Generic;
using System.Threading.Tasks;
using SPACEGO_E_COMMERCE_WEBSITE.Models;

namespace SPACEGO_E_COMMERCE_WEBSITE.Repository
{
    public interface IColorRepository
    {
        Task<IEnumerable<Color>> GetAllAsync();
        Task<Color?> GetByIdAsync(int colorId);
        Task AddAsync(Color color);
        Task UpdateAsync(Color color);
        Task DeleteAsync(int colorId);
    }
}