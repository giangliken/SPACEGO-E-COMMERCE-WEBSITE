using SPACEGO_E_COMMERCE_WEBSITE.Models;

namespace SPACEGO_E_COMMERCE_WEBSITE.Repository
{
    public interface IReviewRepository
    {
        Task<IEnumerable<Review>> GetAllAsync();
        Task<Review> GetByIdAsync(int reviewId);
        Task<IEnumerable<Review>> GetByProductIdAsync(int productId);
        Task AddAsync(Review review);
        Task UpdateAsync(Review review);
        Task DeleteAsync(int reviewId);
    }
}
