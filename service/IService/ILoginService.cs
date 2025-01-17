using imarket.models;

namespace imarket.service.IService
{
    public interface ILoginService
    {
        Task<UserModels?> LoginAsync(string username, string password);
        Task<UserModels?> LogoutAsync();
        Task<int> RegisterAsync(UserModels user);
    }
}
