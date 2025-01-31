using imarket.models;

namespace imarket.service.IService
{
    public interface ILikeService
    {
        Task<IEnumerable<LikeModels>> GetPostLikesByPostIdAsync(ulong postId);
        Task<IEnumerable<LikeModels>> GetCommentLikesByCommentIdAsync(ulong commentId);
        Task<IEnumerable<HotRankingModels.Post>> GetHotRankingAsync();
        Task<int> GetPostLikeNumsByPostIdAsync(ulong postId);
        Task<int> GetCommentLikeNumsByCommentIdAsync(ulong commentId);
        Task<int> CreateLikeAsync(LikeModels postLike);
        Task<int> DeleteLikeAsync(LikeModels postLike);
        Task<int> DeleteLikesByPostIdAsync(ulong postId);
        Task<int> DeleteLikesByUserIdAsync(ulong userId);
        Task<int> DeleteLikesByCommentIdAsync(ulong commentId);
        Task<bool> CheckUserLikeCommentAsync(ulong userId, ulong comentId);
        Task<bool> CheckUserLikePostAsync(ulong userId, ulong postId);
    }
}
