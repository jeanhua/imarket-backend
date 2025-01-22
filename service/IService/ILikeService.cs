using imarket.models;

namespace imarket.service.IService
{
    public interface ILikeService
    {
        Task<IEnumerable<LikeModels>> GetPostLikesByPostIdAsync(string like);
        Task<IEnumerable<LikeModels>> GetCommentLikesByCommentIdAsync(string like);
        Task<IEnumerable<HotRankingModels.Post>> GetHotRankingAsync();
        Task<int> GetPostLikeNumsByPostIdAsync(string postId);
        Task<int> GetCommentLikeNumsByCommentIdAsync(string commentId);
        Task<int> CreateLikeAsync(LikeModels postLike);
        Task<int> DeleteLikeAsync(LikeModels postLike);
        Task<int> DeleteLikesByPostIdAsync(string postId);
        Task<int> DeleteLikesByUserIdAsync(string userId);
        Task<int> DeleteLikesByCommentIdAsync(string commentId);
        Task<bool> CheckUserLikeCommentAsync(string userId, string comentId);
        Task<bool> CheckUserLikePostAsync(string userId, string postId);
    }
}
