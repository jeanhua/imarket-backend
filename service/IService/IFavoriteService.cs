using imarket.models;

namespace imarket.service.IService
{
    public interface IFavoriteService
    {
        Task<IEnumerable<FavoriteModels>> GetPostFavoriteByUserId(ulong userId, int page, int pagesize);
        Task<IEnumerable<HotRankingModels.Favorite>> GetHotRankingAsync();
        Task<int> CreatePostFavoriteAsync(ulong postId, ulong userId);
        Task<int> DeletePostFavoriteAsync(ulong postId, ulong userId);
        Task<int> DeletePostFavoriteByUserIdAsyc(ulong userId);
        Task<int> DeletePostFavoriteByPostIdAsyc(ulong postId);
        Task<int> GetFavoriteNumsByPostIdAsync(ulong postId);
        Task<bool> CheckIsFavorite(ulong userId,ulong postId);
    }
}
