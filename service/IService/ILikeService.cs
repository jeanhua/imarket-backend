using imarket.models;

namespace imarket.service.IService
{
    public interface ILikeService
    {
        Task<IEnumerable<LikeModels>> GetPostLikesByPostIdAsync(string like);
        Task<IEnumerable<LikeModels>> GetCommentLikesByCommentIdAsync(string like);
        Task<int> GetPostLikeNumsByPostIdAsync(string postId);
        Task<int> GetCommentLikeNumsByCommentIdAsync(string commentId);
        Task<int> CreatePostLikeAsync(LikeModels postLike);
        Task<int> DeletePostLikeAsync(LikeModels postLike);
    }
}
