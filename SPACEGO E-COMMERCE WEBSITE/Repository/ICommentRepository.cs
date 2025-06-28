using SPACEGO_E_COMMERCE_WEBSITE.Models;

namespace SPACEGO_E_COMMERCE_WEBSITE.Repository
{
    public interface ICommentRepository
    {
        Task<IEnumerable<Comment>> GetAllAsync();
        Task<Comment?> GetByIdAsync(int id);
        Task AddAsync(Comment comment);
        Task UpdateAsync(Comment comment);
        Task DeleteAsync(int id);

        Task<IEnumerable<Comment>> GetPendingCommentsAsync(); // chưa duyệt
        Task ToggleStatusAsync(int id); // duyệt/ẩn

        Task<IEnumerable<Comment>> GetByProductIdAsync(int productId);
        Task<bool> HasUserCommentedAsync(string userId, int productId);
    }
}
