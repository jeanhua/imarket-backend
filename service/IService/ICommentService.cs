using imarket.models;

namespace imarket.service.IService
{
    public interface ICommentService
    {
        Task<IEnumerable<CommentModels>> GetCommentsByPostIdAsync(int postId);
        Task<int> CreateCommentAsync(CommentModels comment);
        Task<int> DeleteCommentAsync(int id);
        Task<CommentModels> GetCommentByIdAsync(int id);
    }
}
