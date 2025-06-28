using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SPACEGO_E_COMMERCE_WEBSITE.Models;

namespace SPACEGO_E_COMMERCE_WEBSITE.Repository
{
    public class EFUserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public EFUserRepository(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IEnumerable<ApplicationUser>> GetUsersAsync()
        {
            return await _context.ApplicationUsers.ToListAsync();
        }

        public async Task<ApplicationUser> GetUserByIdAsync(string id)
        {
            return await _context.ApplicationUsers.FindAsync(id);
        }

        public async Task<ApplicationUser> GetUserByNameAsync(string username)
        {
            return await _context.ApplicationUsers.FirstOrDefaultAsync(u => u.UserName == username);
        }

        public async Task AddUserAsync(ApplicationUser user)
        {
            _context.ApplicationUsers.Add(user);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> CreateUserWithDefaultPasswordAsync(ApplicationUser user)
        {
            user.UserName = user.Email.Split('@')[0];
            user.CreatedDate = DateTime.Now;
            user.IsActive = true;

            var result = await _userManager.CreateAsync(user, "Abc@123");
            if (!result.Succeeded)
            {
                return false;
            }

            // Gán role nếu cần
            //await _userManager.AddToRoleAsync(user, SD.Role_Customer);
            return true;
        }

        public async Task UpdateUserAsync(ApplicationUser user)
        {
            var existingUser = await _context.ApplicationUsers.FindAsync(user.Id);
            if (existingUser != null)
            {
                existingUser.FullName = user.FullName;
                existingUser.Email = user.Email;
                existingUser.PhoneNumber = user.PhoneNumber;
                existingUser.IsActive = user.IsActive;

                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteUserAsync(string id)
        {
            var user = await _context.ApplicationUsers.FindAsync(id);
            if (user != null)
            {
                _context.ApplicationUsers.Remove(user);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<ApplicationUser>> GetAllUserAsync()
        {
            return await _context.ApplicationUsers.ToListAsync();
        }
    }
}
