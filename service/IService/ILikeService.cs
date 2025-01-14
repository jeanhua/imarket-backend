using imarket.models;

namespace imarket.service.IService
{
    public interface ILikeService
    {
        Task<IEnumerable<LikeModels>> GetPostLikesByPostIdAsync(int postId);
        Task<int> GetPostLikeNumsByPostIdAsync(int postId);
        Task<int> CreatePostLikeAsync(LikeModels postLike);
        Task<int> DeletePostLikeAsync(LikeModels postLike);
    }
}
