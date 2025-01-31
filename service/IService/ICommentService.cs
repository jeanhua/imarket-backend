using imarket.models;

namespace imarket.service.IService
{
    public interface ICommentService
    {
        Task<IEnumerable<CommentModels>> GetCommentsByPostIdAsync(ulong postId);
        Task<IEnumerable<CommentModels>> GetCommentsByUserIdAsync(ulong userId);
        Task<int> CreateCommentAsync(CommentModels comment);
        Task<int> DeleteCommentAsync(ulong id);
        Task<int> DeleteCommentsByPostIdAsync(ulong postId);
        Task<int> DeleteCommentsByUserIdAsync(ulong userId);
        Task<CommentModels?> GetCommentByIdAsync(ulong id);
        Task<int> GetCommentsNumByPostIdAsync(ulong postId);
    }
}
