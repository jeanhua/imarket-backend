using imarket.models;

namespace imarket.service.IService
{
    public interface ICommentService
    {
        Task<IEnumerable<CommentModels>> GetCommentsByPostIdAsync(string postId);
        Task<IEnumerable<CommentModels>> GetCommentsByUserIdAsync(string userId);
        Task<int> CreateCommentAsync(CommentModels comment);
        Task<int> DeleteCommentAsync(string id);
        Task<int> DeleteCommentsByPostIdAsync(string postId);
        Task<int> DeleteCommentsByUserIdAsync(string userId);
        Task<CommentModels?> GetCommentByIdAsync(string id);
        Task<int> GetCommentsNumByPostIdAsync(string postId);
    }
}
