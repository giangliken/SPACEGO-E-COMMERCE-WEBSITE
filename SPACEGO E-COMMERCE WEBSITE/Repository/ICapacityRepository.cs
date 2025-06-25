using System.Collections.Generic;
using System.Threading.Tasks;
using SPACEGO_E_COMMERCE_WEBSITE.Models;

namespace SPACEGO_E_COMMERCE_WEBSITE.Repository
{
    public interface ICapacityRepository
    {
        Task<IEnumerable<Capacity>> GetAllAsync();
        Task<Capacity?> GetByIdAsync(int capacityId);
        Task AddAsync(Capacity capacity);
        Task UpdateAsync(Capacity capacity);
        Task DeleteAsync(int capacityId);
    }
}