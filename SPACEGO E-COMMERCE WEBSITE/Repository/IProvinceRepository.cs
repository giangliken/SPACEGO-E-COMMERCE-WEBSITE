using SPACEGO_E_COMMERCE_WEBSITE.Models;

namespace SPACEGO_E_COMMERCE_WEBSITE.Repository
{
    public interface IProvinceRepository
    {
        Task<IEnumerable<Province>> GetAllAsync();
    }
}
