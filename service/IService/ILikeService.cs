using imarket.models;

namespace imarket.service.IService
{
    public interface ILikeService
    {
        Task<IEnumerable<LikeModels>> GetPostLikesByPostIdAsync(string like);
        Task<int> GetPostLikeNumsByPostIdAsync(string like);
        Task<int> CreatePostLikeAsync(LikeModels postLike);
        Task<int> DeletePostLikeAsync(LikeModels postLike);
    }
}
