using SPACEGO_E_COMMERCE_WEBSITE.Models;

namespace SPACEGO_E_COMMERCE_WEBSITE.Repository
{
    public interface IUserRepository
    {
        // Lấy danh sách người dùng
        Task<IEnumerable<ApplicationUser>> GetUsersAsync();

        // Lấy thông tin người dùng bằng ID
        Task<ApplicationUser> GetUserByIdAsync(string id);

        // Lấy thông tin người dùng bằng tên
        Task<ApplicationUser> GetUserByNameAsync(string username);

        // Thêm người dùng mới
        Task AddUserAsync(ApplicationUser user);
        // Thêm người dùng với mật khẩu mặc định
        Task<bool> CreateUserWithDefaultPasswordAsync(ApplicationUser user);

        // Cập nhật thông tin người dùng
        Task UpdateUserAsync(ApplicationUser user);

        // Xóa người dùng
        Task DeleteUserAsync(string id);
    }
}
