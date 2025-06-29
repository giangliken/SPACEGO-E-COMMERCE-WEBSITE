using SPACEGO_E_COMMERCE_WEBSITE.Models;

namespace SPACEGO_E_COMMERCE_WEBSITE.Repository
{
    public interface IPostCategoryRepository
    {
        Task<IEnumerable<PostCategory>> GetAllAsync();
        Task<PostCategory?> GetByIdAsync(int id);
        Task AddAsync(PostCategory category);
        Task UpdateAsync(PostCategory category);
        Task DeleteAsync(int id);
    }
}
