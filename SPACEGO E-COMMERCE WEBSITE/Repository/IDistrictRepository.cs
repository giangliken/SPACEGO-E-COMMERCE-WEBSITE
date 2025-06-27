using SPACEGO_E_COMMERCE_WEBSITE.Models;

namespace SPACEGO_E_COMMERCE_WEBSITE.Repository
{
    public interface IDistrictRepository
    {
        Task<IEnumerable<District>> GetAllAsync();
        Task<IEnumerable<District>> GetByProvinceIdAsync(int provinceId);
    }
}
