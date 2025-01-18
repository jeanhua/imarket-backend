using imarket.models;

namespace imarket.service.IService
{
    public interface IUserService
    {
        Task<int> GetUserNums();
        Task<UserModels?> GetUserByUsernameAsync(string? username);
        Task<UserModels?> GetUserByEmailAsync(string email);
        Task<IEnumerable<UserModels>> GetAllUsers(int page, int pageSize);
        Task<UserModels?> GetUserByIdAsync(string id);
        Task<int> CreateUserAsync(UserModels user);
        Task<int> UpdateUserAsync(string userId, UserModels user);
        Task<int> DeleteUserAsync(string id);
    }
}
