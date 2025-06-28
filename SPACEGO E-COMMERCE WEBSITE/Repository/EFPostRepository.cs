using Microsoft.EntityFrameworkCore;
using SPACEGO_E_COMMERCE_WEBSITE.Models;

public class EFPostRepository : IPostRepository
{
    private readonly ApplicationDbContext _context;

    public EFPostRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Post>> GetAllAsync()
    {
        return await _context.Posts
            .Include(p => p.PostCategory)
            .Include(p => p.User)
            .ToListAsync();
    }

    public async Task<Post?> GetByIdAsync(int id)
    {
        return await _context.Posts
            .Include(p => p.PostCategory)
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.PostId == id);
    }

    public async Task AddAsync(Post post)
    {
        await _context.Posts.AddAsync(post);
    }

    public async Task UpdateAsync(Post post)
    {
        _context.Posts.Update(post);
    }

    public async Task DeleteAsync(int id)
    {
        var post = await GetByIdAsync(id);
        if (post != null)
        {
            _context.Posts.Remove(post);
        }
    }

    public async Task SaveAsync()
    {
        await _context.SaveChangesAsync();
    }
}
