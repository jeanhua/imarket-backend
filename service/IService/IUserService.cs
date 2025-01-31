using imarket.models;

namespace imarket.service.IService
{
    public interface IUserService
    {
        Task<int> GetUserNums();
        Task<UserModels?> GetUserByUsernameAsync(string? username);
        Task<UserModels?> GetUserByEmailAsync(string email);
        Task<IEnumerable<UserModels>> GetAllUsers(int page, int pageSize);
        Task<UserModels?> GetUserByIdAsync(ulong id);
        Task<int> CreateUserAsync(UserModels user);
        Task<int> UpdateUserAsync(ulong userId, UserModels user);
        Task<int> DeleteUserAsync(ulong id);
    }
}
