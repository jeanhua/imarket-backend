using imarket.models;

namespace imarket.service.IService
{
    public interface IFavoriteService
    {
        Task<IEnumerable<FavoriteModels>> GetPostFavoriteByUserId(string userId, int page, int pagesize);
        Task<IEnumerable<HotRankingModels.Favorite>> GetHotRankingAsync();
        Task<int> CreatePostFavoriteAsync(string postId, string userId);
        Task<int> DeletePostFavoriteAsync(string postId, string userId);
        Task<int> DeletePostFavoriteByUserIdAsyc(string userId);
        Task<int> DeletePostFavoriteByPostIdAsyc(string postId);
        Task<int> GetFavoriteNumsByPostIdAsync(string postId);
        Task<bool> CheckIsFavorite(string userId,string postId);
    }
}
