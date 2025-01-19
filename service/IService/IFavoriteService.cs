using imarket.models;

namespace imarket.service.IService
{
    public interface IFavoriteService
    {
        Task<IEnumerable<FavoriteModels>> GetPostFavoriteByUserId(string userId, int page, int pagesize);
        Task<int> CreatePostFavoriteAsync(string postId, string userId);
        Task<int> DeletePostFavoriteAsync(string postId, string userId);
    }
}
