using imarket.models;

namespace imarket.service.IService
{
    public interface ILikeService
    {
        Task<IEnumerable<LikeModels>> GetPostLikesByPostIdAsync(string like);
        Task<IEnumerable<LikeModels>> GetCommentLikesByCommentIdAsync(string like);
        Task<int> GetPostLikeNumsByPostIdAsync(string postId);
        Task<int> GetCommentLikeNumsByCommentIdAsync(string commentId);
        Task<int> CreateLikeAsync(LikeModels postLike);
        Task<int> DeleteLikeAsync(LikeModels postLike);
        Task<bool> CheckUserLikeCommentAsync(string userId, string comentId);
        Task<bool> CheckUserLikePostAsync(string userId, string postId);
    }
}
