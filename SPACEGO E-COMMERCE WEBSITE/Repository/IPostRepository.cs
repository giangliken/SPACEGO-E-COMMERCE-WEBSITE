using SPACEGO_E_COMMERCE_WEBSITE.Models;

public interface IPostRepository
{
    Task<IEnumerable<Post>> GetAllAsync();
    Task<IEnumerable<Post>> GetPublishedAsync();
    Task<Post?> GetByIdAsync(int id);
    Task AddAsync(Post post);
    Task UpdateAsync(Post post);
    Task DeleteAsync(int id);
}