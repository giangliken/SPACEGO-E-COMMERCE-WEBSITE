using SPACEGO_E_COMMERCE_WEBSITE.Models;

namespace SPACEGO_E_COMMERCE_WEBSITE.Repository
{
    public interface IWardRepository
    {
        Task<IEnumerable<Ward>> GetAllAsync();
        Task<IEnumerable<Ward>> GetByDistrictIdAsync(int districtID);
    }
}
