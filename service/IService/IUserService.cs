using imarket.models;

namespace imarket.service.IService
{
    public interface IUserService
    {
        Task<int> GetUserNums();
        Task<UserModels?> GetUserByUsernameAsync(string username);
        Task<UserModels?> GetUserByEmailAsync(string email);
        Task<IEnumerable<UserModels>> GetAllUsers(int page, int pageSize);
        Task<UserModels> GetUserByIdAsync(int id);
        Task<int> CreateUserAsync(UserModels user);
        Task<int> UpdateUserAsync(int userId, UserModels user);
        Task<int> DeleteUserAsync(int id);
    }
}
